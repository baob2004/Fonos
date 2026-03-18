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
        public Book(string title, string description, string coverImageUrl, decimal price)
        {
            ValidInputs(title, description, coverImageUrl, price);

            Title = title;
            Description = description;
            CoverImageUrl = coverImageUrl;
            Price = price;
        }
        public static Book Create(string title, string description, string coverImageUrl, decimal price)
        {
            return new Book(title, description, coverImageUrl, price);
        }
        public void Update(string title, string description, string coverImageUrl, decimal price)
        {
            ValidInputs(title, description, coverImageUrl, price);

            Title = title;
            Description = description;
            CoverImageUrl = coverImageUrl;
            Price = price;

            UpdateLastModified();
        }
        private static void ValidInputs(string title, string description, string coverImageUrl, decimal price)
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
        }
    }
}
