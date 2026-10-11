using Product.Core.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Product.Core.Interfaces
{
   public interface IBrandService
    {
        public Task<List<BrandDtoResponse>> ListBrands();
        public Task<BrandDtoResponse> GetBrandById(Guid id);
        public Task CreateBrand(BrandDto brand);
        public Task UpdateBrand(BrandDto brand);
        public Task DeleteBrand(Guid id);
    }
}
