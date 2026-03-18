namespace Fonos.API.Models
{
    public class Category : BaseEntity
    {
        public string Name { get; private set; } = string.Empty;
        public virtual ICollection<Book> Books { get; set; } = new List<Book>();

        private Category() { }

        private Category(string name)
        {
            ValidateInputs(name);
            Name = name;
        }

        public static Category Create(string name)
        {
            return new Category(name);
        }

        public void Update(string name)
        {
            ValidateInputs(name);
            Name = name;
            UpdateLastModified();
        }

        private static void ValidateInputs(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Name cannot be null or empty", nameof(name));
            }
        }
    }
}