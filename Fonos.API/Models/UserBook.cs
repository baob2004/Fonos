namespace Fonos.API.Models
{
    public class UserBook : BaseEntity
    {
        public string UserId { get; private set; } = null!;
        public virtual ApplicationUser User { get; set; } = null!;

        public Guid BookId { get; private set; }
        public virtual Book Book { get; set; } = null!;

        public DateTime PurchaseDate { get; private set; } = DateTime.UtcNow;

        private UserBook() { }

        private UserBook(string userId, Guid bookId)
        {
            ValidateInputs(userId, bookId);
            UserId = userId;
            BookId = bookId;
            PurchaseDate = DateTime.UtcNow;
        }

        public static UserBook Create(string userId, Guid bookId)
        {
            return new UserBook(userId, bookId);
        }

        private static void ValidateInputs(string userId, Guid bookId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be empty", nameof(userId));

            if (bookId == Guid.Empty)
                throw new ArgumentException("Book ID cannot be empty", nameof(bookId));
        }
    }
}