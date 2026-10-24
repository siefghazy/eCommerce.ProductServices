using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Product.Core.Dtos;
using Product.Core.Interfaces;
using Product.Domain.Entities;
using Product.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Product.Core.Services
{
    public class BrandService : IBrandService
    {
        private readonly IPicturesRepo _picturesRepository;
        private readonly IBrandRepo _brandRepository;
        private readonly IDatabaseTransaction _transaction;
        public BrandService(IBrandRepo brandRepository, IDatabaseTransaction transaction, IPicturesRepo picturesRepository)
        {
            _brandRepository = brandRepository;
            _transaction = transaction;
            _picturesRepository = picturesRepository;
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

        public async Task DeleteBrand(Guid id)
        {
            try
            {
                var brandToDelete = _brandRepository.ListBrands().Include(b => b.Image).Where(i => i.IsDeleted == false).FirstOrDefault(b => b.Id == id && b.IsDeleted == false);
                if (brandToDelete.Image != null)
                {
                    brandToDelete.Image.IsDeleted = true;
                    await _transaction.ExecuteAsync(async () =>
                    {
                        await _brandRepository.DeleteBrand(brandToDelete);
                    });
                    File.Delete(brandToDelete.Image.Url);
                }
                else
                {
                    await _brandRepository.DeleteBrand(brandToDelete);
                }

            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while deleting the brand: {ex.Message}", ex);
            }
        }

        public Task<BrandDtoResponse> GetBrandById(Guid id)
        {
            throw new NotImplementedException();
        }

        public async Task<List<BrandDtoResponse>> ListBrands()
        {
            var brands = await _brandRepository.ListBrands().Where(b=>b.IsDeleted==false).Select(b => new BrandDtoResponse
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
                    Pictures = p.Images != null ? p.Images.Where(p=>p.IsDeleted==false).Select(pic => new PicturesDto
                    {
                        Id = pic.Id,
                        Path = pic.Url
                    }).ToList() : null
                }).ToList()

            }).ToListAsync();
            return brands;
        }

        public async Task UpdateBrand(Guid id,BrandDto brand)
        {
            try
            {
                if (brand.LogoUrl != null)
                {
                    var brandToUpdate = await _brandRepository.ListBrands().Include(b => b.Image).Where(b => b.Image.IsDeleted == false).FirstOrDefaultAsync(b => b.Id == id);
                    if (brandToUpdate.Image != null)
                    {
                        brandToUpdate.Name = brand.Name;
                        brandToUpdate.Description = brand.Description;
                        brandToUpdate.UpdatedAt = DateTime.UtcNow;
                        brandToUpdate.Image.IsDeleted = true;
                        var PathToDelete = brandToUpdate.Image.Url;
                        var path = GeneratePath(brand.LogoUrl);
                        var newImage = new Images
                        {
                            Id = Guid.NewGuid(),
                            Url = path,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };
                        brandToUpdate.ImageId = newImage.Id;
                        await _transaction.ExecuteAsync(async () =>
                        {
                            await _picturesRepository.CreatePicture(newImage);
                            await _brandRepository.UpdateBrand(brandToUpdate);
                        });
                        File.Delete(PathToDelete);
                        using (var stream = new FileStream(path, FileMode.Create))
                        {
                            await brand.LogoUrl.CopyToAsync(stream);
                        }
                    }
                    else
                    {
                        var path = GeneratePath(brand.LogoUrl);
                        var newImage = new Images
                        {
                            Id = Guid.NewGuid(),
                            Url = path,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };
                        var brandToUpdateWithoutImage = await _brandRepository.ListBrands().FirstOrDefaultAsync(b => b.Id == id && b.IsDeleted == false);
                        brandToUpdateWithoutImage.Name = brand.Name;
                        brandToUpdateWithoutImage.Description = brand.Description;
                        brandToUpdateWithoutImage.UpdatedAt = DateTime.UtcNow;
                        brandToUpdateWithoutImage.ImageId = newImage.Id;
                        await _transaction.ExecuteAsync(async () =>
                        {
                            await _picturesRepository.CreatePicture(newImage);
                            await _brandRepository.UpdateBrand(brandToUpdateWithoutImage);
                        });
                        using (var stream = new FileStream(path, FileMode.Create))
                        {
                            await brand.LogoUrl.CopyToAsync(stream);
                        }
                    }
                }
                else
                {
                    var brandToUpdateWithoutImage = await _brandRepository.ListBrands().FirstOrDefaultAsync(b => b.Id == id && b.IsDeleted == false);
                    brandToUpdateWithoutImage.Name = brand.Name;
                    brandToUpdateWithoutImage.Description = brand.Description;
                    brandToUpdateWithoutImage.UpdatedAt = DateTime.UtcNow;

                    await _brandRepository.UpdateBrand(brandToUpdateWithoutImage);
                }

            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while updating the brand: {ex.Message}", ex);
            }
           
            
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
