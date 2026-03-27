using Fonos.API.DTOs.Payments;

namespace Fonos.API.Services.Payments
{
    public interface IPaymentService
    {
        Task<PaymentDto> CreatePaymentAsync(PaymentCreateDto command);
        Task<PaymentDto> GetPaymentAsync(Guid id);
        Task<IEnumerable<PaymentDto>> GetUserPaymentsAsync(string userId);
        Task CompletePaymentAsync(Guid id);
        Task CancelPaymentAsync(Guid id);
        Task<string> CreateVnPayUrlAsync(Guid bookId, string userId, string ipAddress);
        Task<PaymentDto> ProcessVnPayCallbackAsync(IDictionary<string, string> vnpayData, string userId);
    }
}