using Fonos.API.DTOs.Chapters;
using Fonos.API.Models;
using Fonos.API.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Fonos.API.Services.Chapters
{
    public class ChapterService : IChapterService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ChapterService(IWebHostEnvironment webHostEnvironment, ApplicationDbContext dbContext)
        {
            _webHostEnvironment = webHostEnvironment;
            _dbContext = dbContext;
        }

        public async Task<ChapterDto> CreateChapterAsync(ChapterCreateDto command)
        {
            // Validate Book tồn tại
            var bookExists = await _dbContext.Books.AnyAsync(b => b.Id == command.BookId);
            if (!bookExists) throw new KeyNotFoundException("Invalid Book Id");

            var chapter = Chapter.Create(command.BookId, command.OrderNumber, command.Title, command.ContentText, command.AudioUrl);

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

        // Trong ChapterService.cs
        public async Task<ChapterDto> CreateWithUploadAsync(ChapterUploadDto dto)
        {
            // 1. Xử lý lưu file vật lý vào wwwroot/audios
            string folderPath = Path.Combine(_webHostEnvironment.WebRootPath, "audios");
            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

            string fileName = Path.GetFileName(dto.AudioFile.FileName);
            // Mẹo: Thêm Guid để tránh trùng tên file nếu nhiều sách có cùng tên "Chương 1.mp3"
            string uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
            string physicalPath = Path.Combine(folderPath, uniqueFileName);

            using (var stream = new FileStream(physicalPath, FileMode.Create))
            {
                await dto.AudioFile.CopyToAsync(stream);
            }

            // 2. Tạo Entity thông qua Factory Method của bạn
            var audioUrl = $"/audios/{uniqueFileName}";
            var chapter = Chapter.Create(dto.BookId, dto.OrderNumber, dto.Title, null, audioUrl);

            // 3. Lưu vào Database
            await _dbContext.Chapters.AddAsync(chapter);
            await _dbContext.SaveChangesAsync();

            // Trình bày kết quả trả về dạng DTO
            return MapToDto(chapter);
        }

        private static ChapterDto MapToDto(Chapter c) =>
            new ChapterDto(c.Id, c.BookId, c.OrderNumber, c.Title, c.ContentText, c.AudioUrl, c.DurationInSeconds, c.Status.ToString());
    }
}