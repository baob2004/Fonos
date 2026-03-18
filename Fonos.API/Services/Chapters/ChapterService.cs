using Fonos.API.DTOs.Chapters;
using Fonos.API.Models;
using Fonos.API.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Fonos.API.Services.Chapters
{
    public class ChapterService : IChapterService
    {
        private readonly ApplicationDbContext _dbContext;

        public ChapterService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ChapterDto> CreateChapterAsync(ChapterCreateDto command)
        {
            // Validate Book tồn tại
            var bookExists = await _dbContext.Books.AnyAsync(b => b.Id == command.BookId);
            if (!bookExists) throw new KeyNotFoundException("Invalid Book Id");

            var chapter = Chapter.Create(command.BookId, command.OrderNumber, command.Title, command.ContentText);

            await _dbContext.Chapters.AddAsync(chapter);
            await _dbContext.SaveChangesAsync();

            return MapToDto(chapter);
        }

        public async Task UpdateAudioAsync(Guid id, ChapterAudioUpdateDto command)
        {
            var chapter = await _dbContext.Chapters.FindAsync(id)
                          ?? throw new KeyNotFoundException("Invalid Chapter Id");

            // Sử dụng logic nghiệp vụ từ Domain Model
            chapter.SetAudio(command.AudioUrl, command.DurationInSeconds);

            await _dbContext.SaveChangesAsync();
        }

        public async Task<IEnumerable<ChapterDto>> GetChaptersByBookAsync(Guid bookId)
        {
            return await _dbContext.Chapters
                .Where(c => c.BookId == bookId)
                .OrderBy(c => c.OrderNumber)
                .Select(c => MapToDto(c))
                .ToListAsync();
        }

        public async Task<ChapterDto> GetChapterAsync(Guid id)
        {
            var chapter = await _dbContext.Chapters.FindAsync(id)
                          ?? throw new KeyNotFoundException("Invalid Chapter Id");
            return MapToDto(chapter);
        }

        public async Task UpdateChapterAsync(Guid id, ChapterUpdateDto command)
        {
            var chapter = await _dbContext.Chapters.FindAsync(id)
                          ?? throw new KeyNotFoundException("Invalid Chapter Id");

            chapter.Update(command.OrderNumber, command.Title, command.ContentText);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteChapterAsync(Guid id)
        {
            var chapter = await _dbContext.Chapters.FindAsync(id)
                          ?? throw new KeyNotFoundException("Invalid Chapter Id");

            _dbContext.Chapters.Remove(chapter);
            await _dbContext.SaveChangesAsync();
        }

        private static ChapterDto MapToDto(Chapter c) =>
            new ChapterDto(c.Id, c.BookId, c.OrderNumber, c.Title, c.ContentText, c.AudioUrl, c.DurationInSeconds, c.Status.ToString());
    }
}