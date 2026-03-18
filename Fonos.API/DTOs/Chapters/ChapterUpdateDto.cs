namespace Fonos.API.DTOs.Chapters
{
    public record ChapterUpdateDto(
            int OrderNumber,
            string Title,
            string? ContentText
    );
}
