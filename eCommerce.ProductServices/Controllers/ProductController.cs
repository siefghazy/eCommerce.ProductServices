using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Product.Core.Dtos;
using Product.Core.Interfaces;

namespace eCommerce.ProductServices.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }
        
        [HttpGet("GetProducts")]
        public async Task<IActionResult> GetProducts([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
           
            var products = await _productService.ListProducts(pageNumber, pageSize);
            return Ok(products);
        }
       
        [HttpPost("CreateProduct")]
        public async Task<IActionResult> CreateProduct([FromForm] ProductDto productDto)
        {
            try
            {
                await _productService.CreateProduct(productDto);
                return Ok(new { message = "Product created successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Error creating product: {ex.Message}" });
            }

        }
        [Authorize(Roles ="Admin")]
        [HttpPut("UpdateProduct/{id}")]
        public async Task<IActionResult> UpdateProduct([FromRoute] Guid id, [FromForm] ProductDto productDto)
        {
            await _productService.UpdateProduct(id, productDto);
            return Ok(new { message = "Product updated successfully." });
        }
        [Authorize(Roles ="Admin")]
        [HttpDelete("DeleteProduct/{id}")]
        public async Task<IActionResult> DeleteProduct([FromRoute] Guid id)
        {
            await _productService.DeleteProduct(id);
            return Ok(new { message = "Product deleted successfully." });
        }
        [HttpGet("GetProductById/{id}")]
        public async Task<IActionResult> GetProductById(Guid id)
        {
            var product = await _productService.GetProductById(id);
            return Ok(product);
        }
        [HttpGet("SearchProducts")]
        public async Task<IActionResult> SearchProducts([FromQuery] string searchTerm)
        {
            var products = await _productService.SearchProducts(searchTerm);
            return Ok(products);
        }
    }
}
