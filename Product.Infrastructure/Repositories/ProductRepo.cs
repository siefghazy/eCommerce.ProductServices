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
            if (product.Images != null)
            {
                await _dbContext.Products.AddAsync(product);
            }
            else
            {
                await _dbContext.Products.AddAsync(product);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task DeleteProduct(Products product)
        {
            if (product.Images != null)
            {
                product.isDeleted = true;
                await UpdateProduct(product);
            }
            else
            {
                product.isDeleted = true;
                await UpdateProduct(product);
                await _dbContext.SaveChangesAsync();
            }

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
            if (product.Images != null)
            {
                _dbContext.Products.Update(product);
              
            }
            else
            {
                _dbContext.Products.Update(product);
                await _dbContext.SaveChangesAsync();
            } 
        }
        
    }
}
