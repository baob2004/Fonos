namespace Fonos.API.Models
{
    public enum PaymentStatus
    {
        Pending,
        Completed,
        Failed,
        Canceled
    }
    public class Payment : BaseEntity
    {
        public string UserId { get; set; } = null!;
        public ApplicationUser User { get; set; } = null!;
        public decimal Amount { get; set; }
        public string TransactionId { get; set; } = string.Empty;
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
        public string PaymentMethod { get; set; } = "E-Wallet"; 
        public string? Description { get; set; } = string.Empty; 
    }
}
