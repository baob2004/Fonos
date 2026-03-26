namespace Fonos.API.DTOs.Chapters
{
    public class ChapterUploadDto
    {
        public Guid BookId { get; set; }
        public int OrderNumber { get; set; }
        public string Title { get; set; }
        public IFormFile AudioFile { get; set; }
    }
}
