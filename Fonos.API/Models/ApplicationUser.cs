using Microsoft.AspNetCore.Identity;

namespace Fonos.API.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public decimal WalletBalance { get; set; } = 0;
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
        public ICollection<UserBook> PurchasedBooks { get; set; } = new List<UserBook>();
    }
}
