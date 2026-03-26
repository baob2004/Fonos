using Fonos.API.Common;
using Fonos.API.DTOs.Categories;
using Fonos.API.Models;
using Fonos.API.Persistence;
using Microsoft.EntityFrameworkCore;
using static System.Net.WebRequestMethods;

namespace Fonos.API.Services.Categories
{
    public class CategoryService : ICategoryService
    {
        private readonly ApplicationDbContext _dbContext;

        public CategoryService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<CategoryDto> CreateCategoryAsync(CategoryCreateDto command)
        {
            // Sử dụng hàm Create từ Domain Model của Bảo
            var category = Category.Create(command.Name);

            await _dbContext.Categories.AddAsync(category);
            await _dbContext.SaveChangesAsync();

            return new CategoryDto(category.Id, category.Name, category.Books.Count());
        }

        public async Task<CategoryDto> GetCategoryAsync(Guid id)
        {
            var category = await _dbContext.Categories.FindAsync(id)
                           ?? throw new KeyNotFoundException("Invalid Category Id");

            return new CategoryDto(category.Id, category.Name, category.Books.Count());
        }

        public async Task<PagedResponse<CategoryDto>> GetAllCategoriesAsync(QueryFilter filter, CancellationToken cancellationToken= default)
        {
            var pageNumber = Math.Max(1, filter.PageNumber);
            var pageSize = Math.Clamp(filter.PageSize, 1, 50);

            var query = _dbContext.Categories.Include(c=>c.Books).AsNoTracking().AsQueryable();

            // 1. Apply search filter (reduces the dataset)
            query = query.ApplySearch(filter.Search);

            // 2. Count total records AFTER filtering, BEFORE pagination
            var totalRecords = await query.CountAsync(cancellationToken);

            // 3. Apply sorting (default to Title if not specified)
            query = query.ApplySort(
                string.IsNullOrWhiteSpace(filter.SortBy) ? "Name" : filter.SortBy);

            // 4. Apply pagination and project to DTOs
            var categories = await query
                .ApplyPagination(pageNumber, pageSize)
                .Select(m => new CategoryDto(m.Id, m.Name, m.Books.Count()))
                .ToListAsync(cancellationToken);

            return new PagedResponse<CategoryDto>
            {
                Data = categories,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalRecords = totalRecords,
                TotalPages = (int)Math.Ceiling(totalRecords / (double)pageSize)
            };
        }

        public async Task UpdateCategoryAsync(Guid id, CategoryUpdateDto command)
        {
            var category = await _dbContext.Categories.FindAsync(id)
                           ?? throw new KeyNotFoundException("Invalid Category Id");

            category.Update(command.Name);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteCategoryAsync(Guid id)
        {
            var category = await _dbContext.Categories.FindAsync(id)
                           ?? throw new KeyNotFoundException("Invalid Category Id");

            _dbContext.Categories.Remove(category);
            await _dbContext.SaveChangesAsync();
        }
    }
}