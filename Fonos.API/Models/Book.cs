namespace Fonos.API.Models
{
    public class Book : BaseEntity
    {
        public string Title { get; private set; } = string.Empty;
        public string Description { get; private set; } = string.Empty;
        public string CoverImageUrl { get; private set; } = string.Empty;
        public decimal Price { get; private set; } = 0;
        public Guid AuthorId { get; private set; }
        public Guid CategoryId { get; private set; }
        public Author Author { get; set; } = null!;
        public Category Category { get; set; } = null!;
        public ICollection<Chapter> Chapters { get; set; } = new List<Chapter>();
        public ICollection<UserBook> UserBooks { get; set; } = new List<UserBook>();
        public Book(string title, string description, string coverImageUrl, decimal price, Guid authorId, Guid categoryId)
        {
            ValidInputs(title, description, coverImageUrl, price, authorId, categoryId);

            Title = title;
            Description = description;
            CoverImageUrl = coverImageUrl;
            Price = price;
            AuthorId = authorId;
            CategoryId = categoryId;
        }
        public static Book Create(string title, string description, string coverImageUrl, decimal price, Guid authorId, Guid categoryId)
        {
            return new Book(title, description, coverImageUrl, price, authorId, categoryId);
        }
        public void Update(string title, string description, string coverImageUrl, decimal price, Guid authorId, Guid categoryId)
        {
            ValidInputs(title, description, coverImageUrl, price, authorId, categoryId);

            Title = title;
            Description = description;
            CoverImageUrl = coverImageUrl;
            Price = price;
            AuthorId = authorId;
            CategoryId = categoryId;

            UpdateLastModified();
        }
        private static void ValidInputs(string title, string description, string coverImageUrl, decimal price, Guid authorId, Guid categoryId)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                throw new ArgumentNullException("Title cannot be null or empty", nameof(title));
            }
            if (string.IsNullOrWhiteSpace(description))
            {
                throw new ArgumentNullException("Description cannot be null or empty", nameof(description));
            }
            if (string.IsNullOrWhiteSpace(coverImageUrl))
            {
                throw new ArgumentNullException("Cover Image Url cannot be null or empty", nameof(coverImageUrl));
            }
            if (price<0)
            {
                throw new ArgumentOutOfRangeException(nameof(price), "Price cannot be less than 0");
            }
            if (authorId == Guid.Empty)
            {
                throw new ArgumentException("AuthorId cannot be empty", nameof(authorId));
            }
            if (categoryId == Guid.Empty)
            {
                throw new ArgumentException("CategoryId cannot be empty", nameof(categoryId));
            }
        }
    }
}
