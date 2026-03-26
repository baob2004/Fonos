using Fonos.API.DTOs.Chapters;
using Microsoft.AspNetCore.Mvc;

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
        Task<ChapterDto> CreateWithUploadAsync(ChapterUploadDto dto);
    }
}