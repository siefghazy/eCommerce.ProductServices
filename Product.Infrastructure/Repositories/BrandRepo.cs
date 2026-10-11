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
    public class BrandRepo : IBrandRepo
    {
        private readonly ProductDbContext _context;
        public BrandRepo(ProductDbContext context)
        {
            _context = context;
        }
        public async Task CreateBrand(Brands brand)
        {
            await _context.Brands.AddAsync(brand);
           
        }

        public async Task DeleteBrand(Guid id)
        {
            var brand = await _context.Brands.FirstOrDefaultAsync(b => b.Id == id);
            brand.IsDeleted = true;
        }   
        

         public async Task<Brands> GetBrandById(Guid id)
        {
            return await _context.Brands.FirstOrDefaultAsync(b => b.Id == id);
        }

        public IQueryable<Brands> ListBrands()
        {
            return _context.Brands;
        }

        public async Task UpdateBrand(Brands brand)
        {
            _context.Brands.Update(brand);
            
        }
        }
    }

