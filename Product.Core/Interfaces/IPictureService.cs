using Product.Core.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Product.Core.Interfaces
{
    public interface IPictureService
    {
        public Task<IEnumerable<PicturesDto>> ListPictures();
        public Task<PicturesDto> GetPictureById(Guid id);
        public Task CreatePicture(PicturesDto picture);
        
        public Task DeletePicture(Guid id);

    }
}
