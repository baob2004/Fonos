namespace Fonos.API.Models
{
    public enum AudioStatus
    {
        Pending,
        Processing,
        Completed,
        Failed
    }
    public class Chapter : BaseEntity
    {
        public Guid BookId { get; set; }
        public int OrderNumber { get; set; } 
        public string Title { get; set; } = string.Empty;
        public string? ContentText { get; set; } = string.Empty;
        public string? AudioUrl { get; set; }    
        public int DurationInSeconds { get; set; }
        public AudioStatus Status { get; set; } = AudioStatus.Pending;
        public Book Book { get; set; } = null!;
    }
}
