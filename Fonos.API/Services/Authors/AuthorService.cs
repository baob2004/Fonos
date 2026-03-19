using Fonos.API.Common;
using Fonos.API.DTOs.Authors;
using Fonos.API.DTOs.Categories;
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

        public async Task<PagedResponse<AuthorDto>> GetAllAuthorsAsync(QueryFilter filter, CancellationToken cancellationToken)
        {
            var pageNumber = Math.Max(1, filter.PageNumber);
            var pageSize = Math.Clamp(filter.PageSize, 1, 50);

            var query = _dbContext.Authors.AsNoTracking().AsQueryable();

            // 1. Apply search filter (reduces the dataset)
            query = query.ApplySearch(filter.Search);

            // 2. Count total records AFTER filtering, BEFORE pagination
            var totalRecords = await query.CountAsync(cancellationToken);

            // 3. Apply sorting (default to Title if not specified)
            query = query.ApplySort(
                string.IsNullOrWhiteSpace(filter.SortBy) ? "Name" : filter.SortBy);

            // 4. Apply pagination and project to DTOs
            var authors = await query
                .ApplyPagination(pageNumber, pageSize)
                .Select(m => new AuthorDto(m.Id, m.Name,m.AvatarUrl))
                .ToListAsync(cancellationToken);

            return new PagedResponse<AuthorDto>
            {
                Data = authors,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalRecords = totalRecords,
                TotalPages = (int)Math.Ceiling(totalRecords / (double)pageSize)
            };
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