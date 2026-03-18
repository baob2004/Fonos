namespace Fonos.API.DTOs.Payments
{
    public record PaymentCreateDto(
           string UserId,
           decimal Amount,
           string TransactionId,
           string PaymentMethod = "E-Wallet",
           string? Description = ""
    );
}
