using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Product.Core.Dtos;
using Product.Core.Interfaces;
using Product.Domain.Entities;
using Product.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Product.Core.Services
{
    public class BrandService : IBrandService
    {
        private readonly IBrandRepo _brandRepository;
        private readonly IDatabaseTransaction _transaction;
        public BrandService(IBrandRepo brandRepository, IDatabaseTransaction transaction)
        {
            _brandRepository = brandRepository;
            _transaction = transaction;
        }

        public async Task CreateBrand(BrandDto brand)
        {
            try
            {
                
                Brands Brand = new Brands
                {
                    Id = Guid.NewGuid(),
                    Name = brand.Name,
                    Description = brand.Description,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                if (brand.LogoUrl != null)
                {
                    var path = GeneratePath(brand.LogoUrl);
                    await _transaction.ExecuteAsync(async () =>
                    {
                   
                        Brand.Image = new Images
                        {
                            Id = Guid.NewGuid(),
                            Url = path,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };
                        await _brandRepository.CreateBrand(Brand);
                    });
                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await brand.LogoUrl.CopyToAsync(stream);
                    }      
                }
                else
                {
                    await _brandRepository.CreateBrand(Brand);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while creating the brand: {ex.Message}", ex);
            }
        }

        public Task DeleteBrand(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<BrandDtoResponse> GetBrandById(Guid id)
        {
            throw new NotImplementedException();
        }

        public async Task<List<BrandDtoResponse>> ListBrands()
        {
            var brands = await _brandRepository.ListBrands().Select(b => new BrandDtoResponse
            {
                Id = b.Id,
                Name = b.Name,
                Description = b.Description,
                LogoUrl = b.Image != null ? b.Image.Url : null,
                Products = b.Products.Where(x=>x.isDeleted==false).Select(p => new ProductDtoResponse
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    Quantity = p.Quantity,
                    CategoryId = p.CategoryId,
                    BrandId = p.BrandId,
                    Pictures = p.Images != null ? p.Images.Select(pic => new PicturesDto
                    {
                        Id = pic.Id,
                        Path = pic.Url
                    }).ToList() : null
                }).ToList()

            }).ToListAsync();
            return brands;
        }

        public Task UpdateBrand(BrandDto brand)
        {
            throw new NotImplementedException();
        }
        private string GeneratePath(IFormFile file)
        {
            string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "ProductImages");
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            string uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
            return Path.Combine(folderPath, uniqueFileName);
        }
    }
}
