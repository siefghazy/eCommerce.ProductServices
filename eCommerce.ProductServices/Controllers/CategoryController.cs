using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Product.Core.Dtos;
using Product.Core.Interfaces;

namespace eCommerce.ProductServices.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;
        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }
        [HttpGet("ListCategories")]
        public async Task<IActionResult> ListCategories()
        {
            var categories = await _categoryService.ListCategories();
            return Ok(categories);
        }
        [HttpPost("CreateCategory")]
        public async Task<IActionResult> CreateCategory([FromForm] CategoryDto category)
        {
            await _categoryService.CreateCategory(category);
            return Ok(new { message = "Category created successfully" });
        }
        [HttpPut("UpdateCategory/{id}")]
        public async Task<IActionResult> UpdateCategory([FromRoute] Guid id, [FromForm] CategoryDto category)
        {
            await _categoryService.UpdateCategory(id, category);
            return Ok(new { message = "Category updated successfully" });
        }
        [HttpDelete("DeleteCategory/{id}")]
        public async Task<IActionResult> DeleteCategory([FromRoute] Guid id)
        {
            await _categoryService.DeleteCategory(id);
            return Ok(new { message = "Category deleted successfully" });
        }
    }
}
