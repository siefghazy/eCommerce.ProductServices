using Microsoft.EntityFrameworkCore;
using Product.Core.Dtos;
using Product.Core.Interfaces;
using Product.Domain.Entities;
using Product.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Product.Core.Services
{
    public class PictureService : IPictureService
    {
        
       private readonly IPicturesRepo _picturesRepo;
        public PictureService(IPicturesRepo picturesRepo)
        {
            _picturesRepo = picturesRepo;
        }   
        public async Task CreatePicture(PicturesDto picture)
        {


            Images image = new Images
            {
                Id = Guid.NewGuid(),
                Url = picture.Path,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false,
            };
            await _picturesRepo.CreatePicture(image);
        }

        public async Task DeletePicture(Guid id)
        {
            var picture = await _picturesRepo.GetPictureById(id);
            _picturesRepo.DeletePicture(picture.Id);
        }

        public async Task<PicturesDto> GetPictureById(Guid id)
        {
            var picture = await _picturesRepo.GetPictureById(id);
            return new PicturesDto
            {
                Id = picture.Id,
                Path = picture.Url
            };
        }

        public async Task<IEnumerable<PicturesDto>> ListPictures()
        {
            var pictures = await _picturesRepo.ListPictures().Where(p => p.IsDeleted ==false).Select(p => new PicturesDto
            {
                Id = p.Id,
                Path = p.Url
            }).ToListAsync();
            return pictures;

        }
        
    }
}
