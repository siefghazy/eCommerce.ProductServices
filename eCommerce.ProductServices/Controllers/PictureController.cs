using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Product.Core.Interfaces;

namespace eCommerce.ProductServices.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PictureController : ControllerBase
    {
        private readonly IPictureService _pictureService;
        public PictureController(IPictureService pictureService)
        {
            _pictureService = pictureService;
        }
        [HttpGet("GetPictures")]
        public async Task<IActionResult> GetPictures()
        {
            var pictures = await _pictureService.ListPictures();
            return Ok(pictures);
        }
        [HttpDelete("DeletePicture/{id}")]
        public async Task<IActionResult> DeletePicture([FromRoute] Guid id)
        {
            await _pictureService.DeletePicture(id);
            return Ok(new { message = "Picture deleted successfully." });
        }
    }
}
