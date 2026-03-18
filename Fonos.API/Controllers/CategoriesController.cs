using Fonos.API.DTOs.Categories;
using Fonos.API.Services.Categories;
using Microsoft.AspNetCore.Mvc;

namespace Fonos.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public sealed class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CategoryDto>>> GetAll()
        {
            return Ok(await _categoryService.GetAllCategoriesAsync());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CategoryDto>> GetById(Guid id)
        {
            return Ok(await _categoryService.GetCategoryAsync(id));
        }

        [HttpPost]
        public async Task<ActionResult<CategoryDto>> Create([FromBody] CategoryCreateDto command)
        {
            var result = await _categoryService.CreateCategoryAsync(command);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] CategoryUpdateDto command)
        {
            await _categoryService.UpdateCategoryAsync(id, command);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _categoryService.DeleteCategoryAsync(id);
            return NoContent();
        }
    }
}