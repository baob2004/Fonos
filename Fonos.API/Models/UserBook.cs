namespace Fonos.API.Models
{
    public class UserBook : BaseEntity
    {
        public string UserId { get; set; }
        public ApplicationUser User { get; set; } = null!;
        public Guid BookId { get; set; }
        public Book Book { get; set; } = null!;
        public DateTime PurchaseDate { get; set; } = DateTime.UtcNow;
    }
}
