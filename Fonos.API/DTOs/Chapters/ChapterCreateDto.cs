namespace Fonos.API.DTOs.Chapters
{
    public record ChapterCreateDto(
            Guid BookId,
            int OrderNumber,
            string Title,
            string? ContentText,
            string? AudioUrl // Thêm trường này để nhận đường dẫn /audios/sound.mp3
    );
}