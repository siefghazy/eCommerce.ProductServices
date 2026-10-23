using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Product.Core.Dtos;
using Product.Core.Interfaces;
using Product.Domain.Entities;
using Product.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Product.Core.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepo _CategoryRepo;
        private readonly IPicturesRepo _picturesRepo;
        private readonly IDatabaseTransaction _databaseTransaction;
        public CategoryService(ICategoryRepo categoryRepo, IDatabaseTransaction databaseTransaction,IPicturesRepo picturesRepo)
        {
            _CategoryRepo = categoryRepo;
            _databaseTransaction = databaseTransaction;
            _picturesRepo = picturesRepo;
        }
        public async Task CreateCategory(CategoryDto category)
        {
            Category Cat = new Category
            {
                Id = Guid.NewGuid(),
                Name = category.Name,
                Description = category.Description,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            if (category.Picture != null)
            {
                var path = GeneratePath(category.Picture);
                Cat.Image = new Images
                {
                    Id = Guid.NewGuid(),
                    Url = path,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                await _databaseTransaction.ExecuteAsync(async () =>
                {
                    await _CategoryRepo.CreateCategory(Cat);
                });
                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await category.Picture.CopyToAsync(stream);
                }
            }
            else
            {
                await _CategoryRepo.CreateCategory(Cat);
            }
            }

        public async Task DeleteCategory(Guid id)
        {
            try
            {
                var category = _CategoryRepo.ListCategories().Include(c => c.Image).Where(c => c.Image.IsDeleted == false).FirstOrDefault(c => c.Id == id && c.IsDeleted == false);
                if (category.Image != null)
                {
                    category.Image.IsDeleted = true;
                    await _databaseTransaction.ExecuteAsync(async () =>
                    {
                        await _CategoryRepo.DeleteCategory(category);
                    });
                    File.Delete(category.Image.Url);
                }
                else
                {
                    await _CategoryRepo.DeleteCategory(category);
                }

            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while deleting the category with ID {id}: {ex.Message}", ex);
            }
            }

        public async Task<CategoryDtoResponse> GetCategoryById(Guid id)
        {
            try
            {
                var category = await _CategoryRepo.GetCategoryById(id);
                CategoryDtoResponse cat = new CategoryDtoResponse
                {
                    Id = category.Id,
                    Name = category.Name,
                    Description = category.Description,
                    PictureUrl = category.Image != null && category.Image.IsDeleted == false ? category.Image.Url : null,
                    Products = category.Products.Select(p => new ProductDtoResponse
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Description = p.Description,
                        Price = p.Price,
                        Pictures = p.Images != null ? p.Images.Where(p => p.IsDeleted == false).Select(i => new PicturesDto
                        {
                            Id = i.Id,
                            Path = i.Url
                        }).ToList() : null
                    }).ToList()
                };
                return cat;
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while retrieving the category with ID {id}: {ex.Message}", ex);
            }
        }

        public async Task<List<CategoryDtoResponse>> ListCategories()
        {
            var categories = await _CategoryRepo.ListCategories().Select(c => new CategoryDtoResponse
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                PictureUrl = c.Image != null ? c.Image.Url : null,
                Products = c.Products.Select(p => new ProductDtoResponse
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    Pictures = p.Images != null ? p.Images.Where(p=>p.IsDeleted==false).Select(i=>new PicturesDto
                    {
                        Id = i.Id,
                        Path = i.Url
                    }).ToList() : null
                }).ToList()


            }).ToListAsync();
            return categories;
        }

        public async Task UpdateCategory(Guid id, CategoryDto category)
        {
            try
            {
                var categoryToUpdate = _CategoryRepo.ListCategories().Include(c => c.Image).Where(c => c.Image.IsDeleted == false).FirstOrDefault(c => c.Id == id && c.IsDeleted == false);
                categoryToUpdate.Name = category.Name;
                categoryToUpdate.Description = category.Description;
                categoryToUpdate.UpdatedAt = DateTime.UtcNow;
                if (category.Picture != null)
                {
                    if (categoryToUpdate.Image != null)
                    {
                        categoryToUpdate.Image.IsDeleted = true;
                        var path = GeneratePath(category.Picture);
                        var newImage = new Images()
                        {
                            Id = Guid.NewGuid(),
                            Url = path,
                            CreatedAt = DateTime.Now,
                            IsDeleted = false
                        };
                        categoryToUpdate.ImageId = newImage.Id;
                        await _databaseTransaction.ExecuteAsync(async () =>
                        {
                            await _picturesRepo.CreatePicture(newImage);
                            await _CategoryRepo.UpdateCategory(categoryToUpdate);
                        });



                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while updating the category with ID {category.Id}: {ex.Message}", ex);
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
