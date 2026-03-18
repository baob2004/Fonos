using Fonos.API.DTOs.Chapters;

namespace Fonos.API.Services.Chapters
{
    public interface IChapterService
    {
        Task<IEnumerable<ChapterDto>> GetChaptersByBookAsync(Guid bookId);
        Task<ChapterDto> GetChapterAsync(Guid id);
        Task<ChapterDto> CreateChapterAsync(ChapterCreateDto command);
        Task UpdateChapterAsync(Guid id, ChapterUpdateDto command);
        Task DeleteChapterAsync(Guid id);
        Task UpdateAudioAsync(Guid id, ChapterAudioUpdateDto command);
    }
}