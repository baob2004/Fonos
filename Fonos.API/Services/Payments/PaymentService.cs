using Fonos.API.DTOs.Payments;
using Fonos.API.Helpers;
using Fonos.API.Models;
using Fonos.API.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Fonos.API.Services.Payments
{
    public class PaymentService : IPaymentService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IConfiguration _configuration;
        public PaymentService(ApplicationDbContext dbContext, IConfiguration configuration)
        {
            _dbContext = dbContext;
            _configuration = configuration;
        }

        public async Task<PaymentDto> CreatePaymentAsync(PaymentCreateDto command)
        {
            // Sử dụng Transaction để đảm bảo nếu tạo UserBook lỗi thì Payment cũng không được lưu
            using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                // 1. Kiểm tra User tồn tại
                var user = await _dbContext.Users
                    .Include(u => u.PurchasedBooks)
                    .FirstOrDefaultAsync(u => u.Id == command.UserId);

                if (user == null) throw new KeyNotFoundException("Người dùng không tồn tại.");

                // 2. Kiểm tra xem sách đã có trong thư viện chưa (Tránh đăng ký trùng)
                var isAlreadyOwned = user.PurchasedBooks.Any(pb => pb.BookId == command.BookId);
                if (isAlreadyOwned)
                {
                    throw new InvalidOperationException("Sách này đã có trong thư viện của bạn.");
                }

                // 3. Tạo bản ghi Thanh toán (Payment)
                var payment = Payment.Create(
                    command.UserId,
                    command.Amount,
                    command.TransactionId, // Mã GD từ VNPay sẽ truyền vào đây
                    command.PaymentMethod,
                    command.Description
                );

                // 4. Tạo bản ghi Sở hữu sách (UserBook)
                var userBook = UserBook.Create(command.UserId, command.BookId);

                // 5. Lưu vào Database
                await _dbContext.Payments.AddAsync(payment);
                await _dbContext.UserBooks.AddAsync(userBook);

                await _dbContext.SaveChangesAsync();

                // Xác nhận hoàn tất
                await transaction.CommitAsync();

                return MapToDto(payment);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception($"Lỗi xử lý đăng ký sách: {ex.Message}");
            }
        }

        public async Task CompletePaymentAsync(Guid id)
        {
            // Sử dụng Transaction để đảm bảo tính nhất quán dữ liệu (Atomic)
            using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                // 1. Tìm bản ghi Payment kèm thông tin User
                var payment = await _dbContext.Payments
                    .Include(p => p.User)
                    .FirstOrDefaultAsync(p => p.Id == id)
                              ?? throw new KeyNotFoundException("Payment not found");

                if (payment.Status == PaymentStatus.Completed) return;

                // 2. Lấy thông tin sách từ Description (Cần ép kiểu về Guid)
                if (!Guid.TryParse(payment.Description, out Guid bookId))
                {
                    throw new InvalidOperationException("BookId not found in payment description.");
                }

                // 3. Thực hiện nghiệp vụ Domain
                // - Đánh dấu thanh toán thành công
                payment.MarkAsCompleted();

                // - Trừ tiền trong ví User (Dùng hàm DeductBalance đã viết trong Model)
                payment.User.DeductBalance(payment.Amount);

                // 4. Tạo bản ghi UserBook để cấp quyền sở hữu
                // Kiểm tra xem user đã có sách này chưa để tránh lỗi trùng lặp
                var alreadyOwned = await _dbContext.UserBooks
                    .AnyAsync(ub => ub.UserId == payment.UserId && ub.BookId == bookId);

                if (!alreadyOwned)
                {
                    var userBook = UserBook.Create(payment.UserId, bookId);
                    await _dbContext.UserBooks.AddAsync(userBook);
                }

                // 5. Lưu tất cả thay đổi
                await _dbContext.SaveChangesAsync();

                // Commit transaction
                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                // Nếu có bất kỳ lỗi nào (ví dụ: ví không đủ tiền), hệ thống sẽ Rollback
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<PaymentDto> GetPaymentAsync(Guid id)
        {
            var payment = await _dbContext.Payments.FindAsync(id)
                          ?? throw new KeyNotFoundException("Payment not found");
            return MapToDto(payment);
        }

        public async Task<IEnumerable<PaymentDto>> GetUserPaymentsAsync(string userId)
        {
            return await _dbContext.Payments
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.Created)
                .Select(p => MapToDto(p))
                .ToListAsync();
        }

        public async Task CancelPaymentAsync(Guid id)
        {
            var payment = await _dbContext.Payments.FindAsync(id)
                          ?? throw new KeyNotFoundException("Payment not found");

            payment.Cancel();
            await _dbContext.SaveChangesAsync();
        }
        public async Task<string> CreateVnPayUrlAsync(Guid bookId, string userId, string ipAddress)
        {
            // 1. Lấy thông tin sách để lấy Price (Amount)
            var book = await _dbContext.Books.FindAsync(bookId)
                       ?? throw new KeyNotFoundException("Không tìm thấy sách.");

            // 2. Kiểm tra xem User đã mua chưa (Tránh thanh toán 2 lần)
            var isOwned = await _dbContext.UserBooks.AnyAsync(ub => ub.UserId == userId && ub.BookId == bookId);
            if (isOwned) throw new InvalidOperationException("Bạn đã sở hữu sách này.");

            // 3. Khởi tạo VnPayLibrary
            var vnpay = new VnPayLibrary();
            var vnp_HashSecret = _configuration["VnPay:HashSecret"];
            var vnp_TmnCode = _configuration["VnPay:TmnCode"];
            var vnp_Url = _configuration["VnPay:BaseUrl"];
            var vnp_ReturnUrl = _configuration["VnPay:ReturnUrl"];

            // 4. Thêm các tham số chuẩn VNPay
            vnpay.AddRequestData("vnp_Version", "2.1.0");
            vnpay.AddRequestData("vnp_Command", "pay");
            vnpay.AddRequestData("vnp_TmnCode", vnp_TmnCode);

            // SỬ DỤNG PRICE CỦA BOOK (Nhân 100 theo quy định VNPay)
            vnpay.AddRequestData("vnp_Amount", ((int)book.Price * 100).ToString());

            vnpay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
            vnpay.AddRequestData("vnp_CurrCode", "VND");
            vnpay.AddRequestData("vnp_IpAddr", ipAddress);
            vnpay.AddRequestData("vnp_Locale", "vn");

            // Gắn thông tin BookId vào OrderInfo để khi quay về (Callback) ta biết là sách nào
            vnpay.AddRequestData("vnp_OrderInfo", $"Thanh toan sach:{book.Id}");

            vnpay.AddRequestData("vnp_OrderType", "other");
            vnpay.AddRequestData("vnp_ReturnUrl", vnp_ReturnUrl);

            // Mã giao dịch tạm thời (nên lưu vào bảng Payments với trạng thái Pending)
            vnpay.AddRequestData("vnp_TxnRef", DateTime.Now.Ticks.ToString());

            return vnpay.CreateRequestUrl(vnp_Url, vnp_HashSecret);
        }

        public async Task<PaymentDto> ProcessVnPayCallbackAsync(IDictionary<string, string> vnpayData, string userId)
        {
            // 1. Lấy các thông tin cần thiết từ dữ liệu VNPay trả về
            string vnp_ResponseCode = vnpayData["vnp_ResponseCode"];
            string vnp_TransactionStatus = vnpayData["vnp_TransactionStatus"];
            string vnp_OrderInfo = vnpayData["vnp_OrderInfo"];
            string vnp_Amount = vnpayData["vnp_Amount"];
            string vnp_TransactionNo = vnpayData["vnp_TransactionNo"];
            string vnp_BankCode = vnpayData["vnp_BankCode"];

            // 2. Kiểm tra trạng thái giao dịch (00 là thành công theo tài liệu VNPay)
            if (vnp_ResponseCode != "00" || vnp_TransactionStatus != "00")
            {
                throw new Exception("Giao dịch tại VNPay không thành công hoặc người dùng đã hủy.");
            }

            // 3. Tách lấy BookId từ chuỗi OrderInfo (Định dạng: "Thanh toan sach:GUID")
            var parts = vnp_OrderInfo.Split(':');
            if (parts.Length < 2 || !Guid.TryParse(parts[1], out Guid bookId))
            {
                throw new Exception("Dữ liệu OrderInfo của VNPay không chứa BookId hợp lệ.");
            }

            // 4. Bắt đầu Transaction để đảm bảo tính nhất quán (Atomicity)
            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                // 5. Kiểm tra xem sách này User đã lỡ sở hữu chưa (Đề phòng trường hợp lag gọi callback 2 lần)
                var isAlreadyOwned = await _dbContext.UserBooks
                    .AnyAsync(ub => ub.UserId == userId && ub.BookId == bookId);

                if (isAlreadyOwned)
                {
                    throw new InvalidOperationException("Sách này đã được cập nhật vào thư viện của bạn trước đó.");
                }

                // 6. Tạo bản ghi Thanh toán (Payment)
                // Lưu ý: vnp_Amount từ VNPay là số tiền * 100, nên cần chia lại 100
                var amount = decimal.Parse(vnp_Amount) / 100;

                var payment = Payment.Create(
                    userId,
                    amount,
                    vnp_TransactionNo, // Mã giao dịch của VNPay
                    $"VNPAY_{vnp_BankCode}",
                    vnp_OrderInfo
                );

                // 7. Tạo bản ghi Sở hữu sách (UserBook)
                var userBook = UserBook.Create(userId, bookId);

                // 8. Lưu tất cả vào Database
                await _dbContext.Payments.AddAsync(payment);
                await _dbContext.UserBooks.AddAsync(userBook);
                await _dbContext.SaveChangesAsync();

                // 9. Xác nhận hoàn tất Transaction
                await transaction.CommitAsync();

                // Trả về Dto để Frontend hiển thị kết quả
                return MapToDto(payment);
            }
            catch (Exception ex)
            {
                // Nếu có lỗi (VD: DB chết, trùng ID...) thì thu hồi toàn bộ
                await transaction.RollbackAsync();
                throw new Exception($"Lỗi khi cập nhật sở hữu sách sau thanh toán: {ex.Message}");
            }
        }

        private static PaymentDto MapToDto(Payment p) =>
            new(p.Id, p.UserId, p.Amount, p.TransactionId, p.Status.ToString(), p.PaymentMethod, p.Description, p.Created);
    }
}