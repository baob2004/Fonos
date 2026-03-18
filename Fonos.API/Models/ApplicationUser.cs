using Microsoft.AspNetCore.Identity;

namespace Fonos.API.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        public decimal WalletBalance { get; private set; } = 0;

        // Navigation Properties
        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
        public virtual ICollection<UserBook> PurchasedBooks { get; set; } = new List<UserBook>();

        public void UpdateProfile(string fullName, string? avatarUrl)
        {
            if (string.IsNullOrWhiteSpace(fullName)) throw new ArgumentException("Full name is required.");
            FullName = fullName;
            AvatarUrl = avatarUrl;
        }

        public void TopUp(decimal amount)
        {
            if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be positive.");
            WalletBalance += amount;
        }

        public void DeductBalance(decimal amount)
        {
            if (amount > WalletBalance) throw new InvalidOperationException("Insufficient balance.");
            WalletBalance -= amount;
        }
    }
}