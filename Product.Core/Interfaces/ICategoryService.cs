using Product.Core.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Product.Core.Interfaces
{
    public interface ICategoryService
    {
        public Task<List<CategoryDtoResponse>> ListCategories();
        public Task<CategoryDtoResponse> GetCategoryById(Guid id);
        public Task CreateCategory(CategoryDto category);
        public Task UpdateCategory(Guid id,CategoryDto category);
        public Task DeleteCategory(Guid id);
    }
}
