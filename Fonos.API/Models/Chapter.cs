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
        public Guid BookId { get; private set; }
        public int OrderNumber { get; private set; }
        public string Title { get; private set; } = string.Empty;
        public string? ContentText { get; private set; } = string.Empty;
        public string? AudioUrl { get; private set; }
        public int DurationInSeconds { get; private set; }
        public AudioStatus Status { get; private set; } = AudioStatus.Pending;
        public virtual Book Book { get; set; } = null!;

        private Chapter() { }

        private Chapter(Guid bookId, int orderNumber, string title, string? contentText, string? audioUrl = null)
        {
            ValidateInputs(title, orderNumber);
            BookId = bookId;
            OrderNumber = orderNumber;
            Title = title;
            ContentText = contentText;

            if (!string.IsNullOrWhiteSpace(audioUrl))
            {
                AudioUrl = audioUrl;
                DurationInSeconds = 1;
                Status = AudioStatus.Completed;
            }
            else
            {
                Status = AudioStatus.Pending;
            }
        }

        public static Chapter Create(Guid bookId, int orderNumber, string title, string? contentText, string? audioUrl = null)
        {
            return new Chapter(bookId, orderNumber, title, contentText, audioUrl);
        }


        public void Update(int orderNumber, string title, string? contentText)
        {
            ValidateInputs(title, orderNumber);
            OrderNumber = orderNumber;
            Title = title;
            ContentText = contentText;
            UpdateLastModified();
        }

        public void SetAudio(string audioUrl, int durationInSeconds)
        {
            if (string.IsNullOrWhiteSpace(audioUrl))
                throw new ArgumentException("Audio URL cannot be empty", nameof(audioUrl));

            AudioUrl = audioUrl;
            DurationInSeconds = durationInSeconds;
            Status = AudioStatus.Completed;
            UpdateLastModified();
        }

        public void UpdateStatus(AudioStatus status)
        {
            Status = status;
            UpdateLastModified();
        }

        private static void ValidateInputs(string title, int orderNumber)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Chapter title cannot be empty", nameof(title));

            if (orderNumber < 0)
                throw new ArgumentOutOfRangeException(nameof(orderNumber), "Order number cannot be negative");
        }
    }
}