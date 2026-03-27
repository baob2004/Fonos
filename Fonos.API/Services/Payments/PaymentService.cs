using Fonos.API.DTOs.Payments;
using Fonos.API.Models;
using Fonos.API.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Fonos.API.Services.Payments
{
    public class PaymentService : IPaymentService
    {
        private readonly ApplicationDbContext _dbContext;

        public PaymentService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
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

        private static PaymentDto MapToDto(Payment p) =>
            new(p.Id, p.UserId, p.Amount, p.TransactionId, p.Status.ToString(), p.PaymentMethod, p.Description, p.Created);
    }
}