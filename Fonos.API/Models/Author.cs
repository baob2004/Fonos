namespace Fonos.API.Models
{
    public class Author : BaseEntity
    {
        public string Name { get; private set; } = string.Empty;
        public string? AvatarUrl { get; private set; }
        public virtual ICollection<Book> Books { get; set; } = new List<Book>();

        private Author() { }

        private Author(string name, string? avatarUrl)
        {
            ValidateInputs(name);
            Name = name;
            AvatarUrl = avatarUrl;
        }

        public static Author Create(string name, string? avatarUrl = null)
        {
            return new Author(name, avatarUrl);
        }

        public void Update(string name, string? avatarUrl)
        {
            ValidateInputs(name);
            Name = name;
            AvatarUrl = avatarUrl;
            UpdateLastModified();
        }

        private static void ValidateInputs(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Author name cannot be null or empty", nameof(name));
            }
        }
    }
}