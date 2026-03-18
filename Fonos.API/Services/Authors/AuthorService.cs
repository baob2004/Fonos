using Fonos.API.DTOs.Authors;
using Fonos.API.Models;
using Fonos.API.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Fonos.API.Services.Authors
{
    public class AuthorService : IAuthorService
    {
        private readonly ApplicationDbContext _dbContext;

        public AuthorService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<AuthorDto> CreateAuthorAsync(AuthorCreateDto command)
        {
            var author = Author.Create(command.Name, command.AvatarUrl);

            await _dbContext.Authors.AddAsync(author);
            await _dbContext.SaveChangesAsync();

            return new AuthorDto(author.Id, author.Name, author.AvatarUrl);
        }

        public async Task<AuthorDto> GetAuthorAsync(Guid id)
        {
            var author = await _dbContext.Authors.FindAsync(id)
                         ?? throw new KeyNotFoundException("Invalid Author Id");

            return new AuthorDto(author.Id, author.Name, author.AvatarUrl);
        }

        public async Task<IEnumerable<AuthorDto>> GetAllAuthorsAsync()
        {
            return await _dbContext.Authors
                .Select(a => new AuthorDto(a.Id, a.Name, a.AvatarUrl))
                .ToListAsync();
        }

        public async Task UpdateAuthorAsync(Guid id, AuthorUpdateDto command)
        {
            var author = await _dbContext.Authors.FindAsync(id)
                         ?? throw new KeyNotFoundException("Invalid Author Id");

            author.Update(command.Name, command.AvatarUrl);

            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteAuthorAsync(Guid id)
        {
            var author = await _dbContext.Authors.FindAsync(id)
                         ?? throw new KeyNotFoundException("Invalid Author Id");

            _dbContext.Authors.Remove(author);
            await _dbContext.SaveChangesAsync();
        }
    }
}