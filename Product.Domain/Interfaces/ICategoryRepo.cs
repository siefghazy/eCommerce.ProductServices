using Product.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Product.Domain.Interfaces
{
    public interface ICategoryRepo
    {
        public IQueryable<Category> ListCategories();
        public Task<Category> GetCategoryById(Guid id);
        public Task<Category> CreateCategory(Category category);
        public Task UpdateCategory(Category category);
        public Task DeleteCategory(Guid id);
    }
}
