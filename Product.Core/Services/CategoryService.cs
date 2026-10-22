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
        private readonly IDatabaseTransaction _databaseTransaction;
        public CategoryService(ICategoryRepo categoryRepo, IDatabaseTransaction databaseTransaction)
        {
            _CategoryRepo = categoryRepo;
            _databaseTransaction = databaseTransaction;
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

        public Task DeleteCategory(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<CategoryDtoResponse> GetCategoryById(Guid id)
        {
            throw new NotImplementedException();
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

        public Task UpdateCategory(CategoryDto category)
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
