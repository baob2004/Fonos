using Fonos.API.DTOs.Chapters;
using Fonos.API.Models;
using Fonos.API.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using TagLib;
namespace Fonos.API.Services.Chapters
{
    public class ChapterService : IChapterService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly UserManager<ApplicationUser> _userManager;
        public ChapterService(IWebHostEnvironment webHostEnvironment, ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager)
        {
            _webHostEnvironment = webHostEnvironment;
            _dbContext = dbContext;
            _userManager = userManager;
        }

        public async Task<ChapterDto> CreateChapterAsync(ChapterCreateDto command)
        {
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

            chapter.SetAudio(command.AudioUrl, command.DurationInSeconds);

            await _dbContext.SaveChangesAsync();
        }

        public async Task<IEnumerable<ChapterDto>> GetChaptersByBookAsync(Guid bookId, string? userId)
        {
            bool canAccessAll = false;

            if (!string.IsNullOrEmpty(userId))
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user != null)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    if (roles.Contains("Administrator"))
                    {
                        canAccessAll = true;
                    }
                }

                if (!canAccessAll)
                {
                    canAccessAll = await _dbContext.UserBooks
                        .AnyAsync(ub => ub.UserId == userId && ub.BookId == bookId);
                }
            }

            var chapters = await _dbContext.Chapters
                .Where(c => c.BookId == bookId)
                .OrderBy(c => c.OrderNumber)
                .ToListAsync();

            return chapters.Select(c => {
                bool canListen = canAccessAll || c.OrderNumber <= 1;

                return new ChapterDto(
                    c.Id,
                    c.BookId,
                    c.OrderNumber,
                    c.Title,
                    c.ContentText,
                    canListen ? c.AudioUrl : null,
                    c.DurationInSeconds,
                    c.Status.ToString()
                );
            });
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

        public async Task<ChapterDto> CreateWithUploadAsync(ChapterUploadDto dto)
        {
            string folderPath = Path.Combine(_webHostEnvironment.WebRootPath, "audios");
            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

            string fileName = Path.GetFileName(dto.AudioFile.FileName);
            string uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
            string physicalPath = Path.Combine(folderPath, uniqueFileName);

            using (var stream = new FileStream(physicalPath, FileMode.Create))
            {
                await dto.AudioFile.CopyToAsync(stream);
            }

            int actualDuration = 0;
            try
            {
                using (var tfile = TagLib.File.Create(physicalPath))
                {
                    actualDuration = (int)tfile.Properties.Duration.TotalSeconds;
                }
            }
            catch (Exception ex)
            {
                actualDuration = 0;
                Console.WriteLine($"Không thể đọc metadata file: {ex.Message}");
            }

            var audioUrl = $"/audios/{uniqueFileName}";
            var chapter = Chapter.Create(dto.BookId, dto.OrderNumber, dto.Title, null, audioUrl);

            chapter.SetAudio(audioUrl, actualDuration);

            await _dbContext.Chapters.AddAsync(chapter);
            await _dbContext.SaveChangesAsync();

            return MapToDto(chapter);
        }

        private static ChapterDto MapToDto(Chapter c) =>
            new ChapterDto(c.Id, c.BookId, c.OrderNumber, c.Title, c.ContentText, c.AudioUrl, c.DurationInSeconds, c.Status.ToString());
    }
}