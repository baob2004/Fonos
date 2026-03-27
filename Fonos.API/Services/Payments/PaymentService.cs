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
            using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                var user = await _dbContext.Users
                    .Include(u => u.PurchasedBooks)
                    .FirstOrDefaultAsync(u => u.Id == command.UserId);

                if (user == null) throw new KeyNotFoundException("Người dùng không tồn tại.");

                var isAlreadyOwned = user.PurchasedBooks.Any(pb => pb.BookId == command.BookId);
                if (isAlreadyOwned)
                {
                    throw new InvalidOperationException("Sách này đã có trong thư viện của bạn.");
                }

                var payment = Payment.Create(
                    command.UserId,
                    command.Amount,
                    command.TransactionId, 
                    command.PaymentMethod,
                    command.Description
                );

                var userBook = UserBook.Create(command.UserId, command.BookId);

                await _dbContext.Payments.AddAsync(payment);
                await _dbContext.UserBooks.AddAsync(userBook);

                await _dbContext.SaveChangesAsync();

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
            using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                var payment = await _dbContext.Payments
                    .Include(p => p.User)
                    .FirstOrDefaultAsync(p => p.Id == id)
                              ?? throw new KeyNotFoundException("Payment not found");

                if (payment.Status == PaymentStatus.Completed) return;

                if (!Guid.TryParse(payment.Description, out Guid bookId))
                {
                    throw new InvalidOperationException("BookId not found in payment description.");
                }

                payment.MarkAsCompleted();

                payment.User.DeductBalance(payment.Amount);

                var alreadyOwned = await _dbContext.UserBooks
                    .AnyAsync(ub => ub.UserId == payment.UserId && ub.BookId == bookId);

                if (!alreadyOwned)
                {
                    var userBook = UserBook.Create(payment.UserId, bookId);
                    await _dbContext.UserBooks.AddAsync(userBook);
                }

                await _dbContext.SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch (Exception)
            {
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
            var book = await _dbContext.Books.FindAsync(bookId)
                       ?? throw new KeyNotFoundException("Không tìm thấy sách.");

            var isOwned = await _dbContext.UserBooks.AnyAsync(ub => ub.UserId == userId && ub.BookId == bookId);
            if (isOwned) throw new InvalidOperationException("Bạn đã sở hữu sách này.");

            var vnpay = new VnPayLibrary();
            var vnp_HashSecret = _configuration["VnPay:HashSecret"];
            var vnp_TmnCode = _configuration["VnPay:TmnCode"];
            var vnp_Url = _configuration["VnPay:BaseUrl"];
            var vnp_ReturnUrl = _configuration["VnPay:ReturnUrl"];

            vnpay.AddRequestData("vnp_Version", "2.1.0");
            vnpay.AddRequestData("vnp_Command", "pay");
            vnpay.AddRequestData("vnp_TmnCode", vnp_TmnCode);

            vnpay.AddRequestData("vnp_Amount", ((int)book.Price * 100).ToString());

            vnpay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
            vnpay.AddRequestData("vnp_CurrCode", "VND");
            vnpay.AddRequestData("vnp_IpAddr", ipAddress);
            vnpay.AddRequestData("vnp_Locale", "vn");

            vnpay.AddRequestData("vnp_OrderInfo", $"Thanh toan sach:{book.Id}");

            vnpay.AddRequestData("vnp_OrderType", "other");
            vnpay.AddRequestData("vnp_ReturnUrl", vnp_ReturnUrl);

            vnpay.AddRequestData("vnp_TxnRef", DateTime.Now.Ticks.ToString());

            return vnpay.CreateRequestUrl(vnp_Url, vnp_HashSecret);
        }

        public async Task<PaymentDto> ProcessVnPayCallbackAsync(IDictionary<string, string> vnpayData, string userId)
        {
            string vnp_ResponseCode = vnpayData["vnp_ResponseCode"];
            string vnp_TransactionStatus = vnpayData["vnp_TransactionStatus"];
            string vnp_OrderInfo = vnpayData["vnp_OrderInfo"];
            string vnp_Amount = vnpayData["vnp_Amount"];
            string vnp_TransactionNo = vnpayData["vnp_TransactionNo"];
            string vnp_BankCode = vnpayData["vnp_BankCode"];

            if (vnp_ResponseCode != "00" || vnp_TransactionStatus != "00")
            {
                throw new Exception("Giao dịch tại VNPay không thành công hoặc người dùng đã hủy.");
            }

            var parts = vnp_OrderInfo.Split(':');
            if (parts.Length < 2 || !Guid.TryParse(parts[1], out Guid bookId))
            {
                throw new Exception("Dữ liệu OrderInfo của VNPay không chứa BookId hợp lệ.");
            }

            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                var isAlreadyOwned = await _dbContext.UserBooks
                    .AnyAsync(ub => ub.UserId == userId && ub.BookId == bookId);

                if (isAlreadyOwned)
                {
                    throw new InvalidOperationException("Sách này đã được cập nhật vào thư viện của bạn trước đó.");
                }

                var amount = decimal.Parse(vnp_Amount) / 100;

                var payment = Payment.Create(
                    userId,
                    amount,
                    vnp_TransactionNo, // Mã giao dịch của VNPay
                    $"VNPAY_{vnp_BankCode}",
                    vnp_OrderInfo
                );

                var userBook = UserBook.Create(userId, bookId);

                await _dbContext.Payments.AddAsync(payment);
                await _dbContext.UserBooks.AddAsync(userBook);
                await _dbContext.SaveChangesAsync();

                await transaction.CommitAsync();

                return MapToDto(payment);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception($"Lỗi khi cập nhật sở hữu sách sau thanh toán: {ex.Message}");
            }
        }

        private static PaymentDto MapToDto(Payment p) =>
            new(p.Id, p.UserId, p.Amount, p.TransactionId, p.Status.ToString(), p.PaymentMethod, p.Description, p.Created);
    }
}