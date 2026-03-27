namespace Fonos.API.DTOs.Payments
{
    public record PaymentCreateDto(
           string UserId,
           Guid BookId,
           decimal Amount,
           string TransactionId,
           string PaymentMethod = "E-Wallet",
           string? Description = ""
    );
}
