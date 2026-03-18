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
        public string UserId { get; private set; } = null!;
        public virtual ApplicationUser User { get; set; } = null!;
        public decimal Amount { get; private set; }
        public string TransactionId { get; private set; } = string.Empty;
        public PaymentStatus Status { get; private set; } = PaymentStatus.Pending;
        public string PaymentMethod { get; private set; } = "E-Wallet";
        public string? Description { get; private set; } = string.Empty;

        private Payment() { }

        private Payment(string userId, decimal amount, string transactionId, string paymentMethod, string? description)
        {
            ValidateInputs(amount, transactionId);
            UserId = userId;
            Amount = amount;
            TransactionId = transactionId;
            PaymentMethod = paymentMethod;
            Description = description;
            Status = PaymentStatus.Pending;
        }

        public static Payment Create(string userId, decimal amount, string transactionId, string paymentMethod = "E-Wallet", string? description = "")
        {
            return new Payment(userId, amount, transactionId, paymentMethod, description);
        }

        public void MarkAsCompleted()
        {
            if (Status != PaymentStatus.Pending)
                throw new InvalidOperationException("Only pending payments can be marked as completed.");

            Status = PaymentStatus.Completed;
            UpdateLastModified();
        }

        public void MarkAsFailed()
        {
            Status = PaymentStatus.Failed;
            UpdateLastModified();
        }

        public void Cancel()
        {
            if (Status == PaymentStatus.Completed)
                throw new InvalidOperationException("Cannot cancel a completed payment.");

            Status = PaymentStatus.Canceled;
            UpdateLastModified();
        }

        private static void ValidateInputs(decimal amount, string transactionId)
        {
            if (amount <= 0)
                throw new ArgumentOutOfRangeException(nameof(amount), "Payment amount must be greater than 0");

            if (string.IsNullOrWhiteSpace(transactionId))
                throw new ArgumentException("Transaction ID cannot be empty", nameof(transactionId));
        }
    }
}