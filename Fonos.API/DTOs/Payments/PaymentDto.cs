namespace Fonos.API.DTOs.Payments
{
    public record PaymentDto(
            Guid Id,
            string UserId,
            decimal Amount,
            string TransactionId,
            string Status,
            string PaymentMethod,
            string? Description,
            DateTimeOffset Created
    );
}
