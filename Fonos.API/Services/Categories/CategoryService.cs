using Fonos.API.DTOs.Categories;
using Fonos.API.Models;
using Fonos.API.Persistence;
using Microsoft.EntityFrameworkCore;

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

            return new CategoryDto(category.Id, category.Name);
        }

        public async Task<CategoryDto> GetCategoryAsync(Guid id)
        {
            var category = await _dbContext.Categories.FindAsync(id)
                           ?? throw new KeyNotFoundException("Invalid Category Id");

            return new CategoryDto(category.Id, category.Name);
        }

        public async Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync()
        {
            return await _dbContext.Categories
                .Select(c => new CategoryDto(c.Id, c.Name))
                .ToListAsync();
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