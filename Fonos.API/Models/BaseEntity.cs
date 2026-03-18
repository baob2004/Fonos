namespace Fonos.API.Models
{
    public abstract class BaseEntity
    {
        public Guid Id { get; set; }
        public DateTimeOffset Created { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset LastModified { get; set; }
        public void UpdateLastModified()
        {
            LastModified = DateTimeOffset.UtcNow;
        }
    }
}
