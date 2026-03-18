namespace Fonos.API.Models
{
    public class Book : BaseEntity
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string CoverImageUrl { get; set; } = string.Empty;
        public decimal Price { get; set; } = 0;
        public Guid AuthorId { get; set; }
        public Guid CategoryId { get; set; }
        public Author Author { get; set; } = null!;
        public Category Category { get; set; } = null!;
        public ICollection<Chapter> Chapters { get; set; } = new List<Chapter>();
        public ICollection<UserBook> UserBooks { get; set; } = new List<UserBook>();
    }
}
