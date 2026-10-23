using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Product.Core.Dtos;
using Product.Core.Interfaces;

namespace eCommerce.ProductServices.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BrandController : ControllerBase
    {
        private readonly IBrandService _brandService;
        public BrandController(IBrandService brandService)
        {
            _brandService = brandService;
        }
        [HttpGet("ListBrands")]
        public async Task<IActionResult> ListBrands()
        {
            var brands = await _brandService.ListBrands();
            return Ok(brands);
        }
        [HttpPost("CreateBrand")]
        public async Task<IActionResult> CreateBrand([FromForm] BrandDto brand)
        {
            await _brandService.CreateBrand(brand);
            return Ok(new { message = "Brand created successfully" });
        }
        [HttpPut("UpdateBrand/{id}")]
        public async Task<IActionResult> UpdateBrand(Guid id, [FromForm] BrandDto brand)
        {
            await _brandService.UpdateBrand(id, brand);
            return Ok(new { message = "Brand updated successfully" });
        }
        [HttpDelete("DeleteBrand/{id}")]
        public async Task<IActionResult> DeleteBrand(Guid id)
        {
            await _brandService.DeleteBrand(id);
            return Ok(new { message = "Brand deleted successfully" });
        }
    }
}
