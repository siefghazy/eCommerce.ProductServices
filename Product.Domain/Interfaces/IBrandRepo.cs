using Product.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Product.Domain.Interfaces
{
    public interface IBrandRepo
    {
        public IQueryable<Brands> ListBrands();
        public Task<Brands> GetBrandById(Guid id);
        public Task CreateBrand(Brands brand);
        public Task UpdateBrand(Brands brand);
        public Task DeleteBrand(Guid id);
    }
}
