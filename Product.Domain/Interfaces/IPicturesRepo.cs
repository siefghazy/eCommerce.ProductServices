using Product.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Product.Domain.Interfaces
{
    public interface IPicturesRepo
    {
        public IQueryable<Images> ListPictures();
        public Task<Images> GetPictureById(Guid id);
        public Task CreatePicture(Images picture);
        public Task UpdatePicture(Images picture);
        public Task DeletePicture(Guid id);
    }
}
