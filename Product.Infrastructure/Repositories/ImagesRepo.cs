using Microsoft.EntityFrameworkCore;
using Product.Domain.Entities;
using Product.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Product.Infrastructure.Repositories
{
    public class ImagesRepo : IPicturesRepo
    {
        private readonly ProductDbContext _context;
        public ImagesRepo(ProductDbContext context)
        {
            _context = context;
        }   
        public async Task CreatePicture(Images picture)
        {
            await _context.Images.AddAsync(picture);
            
        }

        public async Task DeletePicture(Guid id)
        {
            var picture = ListPictures().FirstOrDefault(p => p.Id == id && p.IsDeleted ==false);
            picture.IsDeleted = true;
           
        }

        public async Task<Images> GetPictureById(Guid id)
        {
            var picture = await ListPictures().FirstOrDefaultAsync(p => p.Id == id&& p.IsDeleted == false);
            return picture;
        }
        

        public IQueryable<Images> ListPictures()
        {
            var pictures =  _context.Images;
            return pictures;
        }

        public async Task  UpdatePicture(Images picture)
        {
            _context.Images.Update(picture);
            
        }
    }
}
