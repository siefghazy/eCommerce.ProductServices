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
    public class CategoryRepo : ICategoryRepo
    {
        private readonly ProductDbContext _context;
        public CategoryRepo(ProductDbContext context)
        {
            _context = context;
        }
        public async Task CreateCategory(Category category)
        {
           await _context.Categories.AddAsync(category);
        }

        public async Task DeleteCategory(Guid id)
        {
            var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);
            category.IsDeleted = true;
        }

        public async Task<Category> GetCategoryById(Guid id)
        {
           var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);
           return category;
        }

        public IQueryable<Category> ListCategories()
        {
           var categories= _context.Categories;
           return categories;
        }

        public async Task UpdateCategory(Category category)
        {
            _context.Categories.Update(category);
        }
    }
}
