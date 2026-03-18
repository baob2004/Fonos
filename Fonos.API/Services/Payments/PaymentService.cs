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
            var userExists = await _dbContext.Users.AnyAsync(u => u.Id.ToString() == command.UserId);
            if (!userExists) throw new KeyNotFoundException("User not found");

            var payment = Payment.Create(
                command.UserId,
                command.Amount,
                command.TransactionId,
                command.PaymentMethod,
                command.Description);

            await _dbContext.Payments.AddAsync(payment);
            await _dbContext.SaveChangesAsync();

            return MapToDto(payment);
        }

        public async Task CompletePaymentAsync(Guid id)
        {
            var payment = await _dbContext.Payments.FindAsync(id)
                          ?? throw new KeyNotFoundException("Payment not found");

            payment.MarkAsCompleted(); 
            await _dbContext.SaveChangesAsync();

            // Mẹo: Sau này Bảo có thể thêm logic tạo bản ghi UserBook tại đây 
            // để cấp quyền đọc sách cho User sau khi payment thành công.
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