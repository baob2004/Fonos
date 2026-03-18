namespace Fonos.API.Models
{
    public class Author : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
        public ICollection<Book> Books { get; set; } = new List<Book>();
    }
}
