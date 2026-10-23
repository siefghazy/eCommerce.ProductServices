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
            if (brand.Image != null)
            {
                await _context.Brands.AddAsync(brand);
            }
            else
            {
                await _context.Brands.AddAsync(brand);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteBrand(Brands brand )
        {
            
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
            if (brand.Image != null)
            {
                _context.Brands.Update(brand);
            }
            else
            {
                _context.Brands.Update(brand);
                await _context.SaveChangesAsync();
            }
        }
        }
    }

