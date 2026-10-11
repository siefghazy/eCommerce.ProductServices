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
    public class ProductRepo : IProductRepo
    {
        private readonly ProductDbContext _dbContext;
        public ProductRepo(ProductDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task CreateProduct(Products product)
        {
            await _dbContext.Products.AddAsync(product);
        }

        public async Task DeleteProduct(Guid id)
        {
            var product = await _dbContext.Products.FirstOrDefaultAsync(p => p.Id == id && p.isDeleted == false);
            product.isDeleted = true;
        }

        public async Task<Products> GetProductById(Guid id)
        {
            var product = await _dbContext.Products.FirstOrDefaultAsync(p => p.Id == id&& p.isDeleted == false);
           return product;
        }


        public IQueryable<Products> ListProducts()
        {
            return _dbContext.Products;
        }

        public async Task UpdateProduct(Products product)
        {
           
            _dbContext.Products.Update(product);  
        }
        
    }
}
