using Product.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Product.Domain.Interfaces
{
    public interface IProductRepo
    {
        public IQueryable<Products> ListProducts();
        public Task<Products> GetProductById(Guid id);
        public Task CreateProduct(Products product);
        public Task UpdateProduct(Products product);
        public Task DeleteProduct(Guid id);
    }
}
