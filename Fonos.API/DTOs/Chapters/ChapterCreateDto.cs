namespace Fonos.API.DTOs.Chapters
{
    public record ChapterCreateDto(
            Guid BookId,
            int OrderNumber,
            string Title,
            string? ContentText
    );
}
