namespace Fonos.API.DTOs.Chapters
{
    public record ChapterDto(
            Guid Id,
            Guid BookId,
            int OrderNumber,
            string Title,
            string? ContentText,
            string? AudioUrl,
            int DurationInSeconds,
            string Status
    );
}
