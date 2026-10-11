using Product.Core.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Product.Core.Interfaces
{
    public interface IProductService
    {
        public Task<List<ProductDtoResponse>> ListProducts(int pageNumber, int pageSize );
        public Task<ProductDtoResponse> GetProductById(Guid id);
        public Task CreateProduct(ProductDto product);
        public Task UpdateProduct(Guid id, ProductDto product);
        public Task DeleteProduct(Guid id);
        public Task<List<ProductDtoResponse>> SearchProducts(string searchTerm);
    }
}
