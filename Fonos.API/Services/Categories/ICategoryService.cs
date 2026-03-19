using Fonos.API.Common;
using Fonos.API.DTOs.Categories;

namespace Fonos.API.Services.Categories
{
    public interface ICategoryService
    {
        Task<PagedResponse<CategoryDto>> GetAllCategoriesAsync(QueryFilter query, CancellationToken cancellationToken = default);
        Task<CategoryDto> GetCategoryAsync(Guid id);
        Task<CategoryDto> CreateCategoryAsync(CategoryCreateDto command);
        Task UpdateCategoryAsync(Guid id, CategoryUpdateDto command);
        Task DeleteCategoryAsync(Guid id);
    }
}