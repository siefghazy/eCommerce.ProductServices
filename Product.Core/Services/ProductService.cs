using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Product.Core.Dtos;
using Product.Core.Interfaces;
using Product.Domain.Entities;
using Product.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Product.Core.Services
{
    public class ProductService : IProductService
    {
        private readonly IDistributedCache _Cache;
        private readonly IProductRepo _productRepo;
        private readonly IPictureService _pictureService;
        private readonly IPicturesRepo _picturesRepo;   
        private readonly IDatabaseTransaction _databaseTransaction;
        public ProductService(IProductRepo productRepo, IPictureService pictureService, IDatabaseTransaction databaseTransaction, IDistributedCache cache, IPicturesRepo picturesRepo)
        {
            _productRepo = productRepo;
            _pictureService = pictureService;
            _databaseTransaction = databaseTransaction;
            _Cache = cache;
            _picturesRepo = picturesRepo;
        }
        public async Task CreateProduct(ProductDto product)
        {
            try
            {
                List<Images> images = new List<Images>();
                if (product.Pictures != null)
                {
                    foreach (var picture in product.Pictures)
                    {
                        var path = GeneratePath(picture);
                        images.Add(new Images
                        {
                            Id = Guid.NewGuid(),
                            Url = path,
                            CreatedAt = DateTime.UtcNow,
                            IsDeleted = false
                        });
                    }
                    await _databaseTransaction.ExecuteAsync(async () =>
                    {
                        Products newProduct = new Products
                        {
                            Id = Guid.NewGuid(),
                            Name = product.Name,
                            Description = product.Description,
                            Price = product.Price,
                            CategoryId = product.CategoryId,
                            BrandId = product.brandId,
                            Images = images,
                            Quantity = product.Quantity,
                            CreatedAt = DateTime.UtcNow
                        };
                        await _productRepo.CreateProduct(newProduct);
                    });
                    for (int i = 0; i < images.Count; i++)
                    {
                        await using var stream = new FileStream(images[i].Url, FileMode.Create);
                        var productPicture = product.Pictures.ElementAt(i);
                        await productPicture.CopyToAsync(stream);
                    }
                }
                else
                {
                    Products newProduct = new Products
                    {
                        Id = Guid.NewGuid(),
                        Name = product.Name,
                        Description = product.Description,
                        Price = product.Price,
                        CategoryId = product.CategoryId,
                        BrandId = product.brandId,
                        Quantity = product.Quantity,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _productRepo.CreateProduct(newProduct);
                }

            }
            catch (Exception ex)
            {

                throw new ArgumentException("An error occurred while creating the product.", ex);
            }
        }

        public async Task DeleteProduct(Guid id)
        {
            try
            {
                var product = await _productRepo.ListProducts().Where(p => p.Id == id && p.isDeleted == false).Select(p => new Products
                {
                    Id = p.Id,
                    Description = p.Description,
                    Price = p.Price,
                    CategoryId = p.CategoryId,
                    BrandId = p.BrandId,
                    Quantity = p.Quantity,
                    Name = p.Name,
                    Images = p.Images.Where(i => i.IsDeleted == false).Select(i => new Images
                    {
                        Id = i.Id,
                        Url = i.Url,
                        IsDeleted = i.IsDeleted
                    }).ToList()
                }).FirstOrDefaultAsync();

                if (product.Images != null )
                {
                    foreach (var image in product.Images)
                    {
                        image.IsDeleted = true;
                    }
                    await _databaseTransaction.ExecuteAsync(async () =>
                    {
                        await _productRepo.DeleteProduct(product);
                    });
                    foreach (var image in product.Images)
                    {
                        File.Delete(image.Url);
                    }
                    var chacedProduct = await _Cache.GetStringAsync($"product:{product.Id}");
                    if (chacedProduct != null)
                    {
                        await _Cache.RemoveAsync($"product:{product.Id}");
                    }
                }
                else
                {
                        await _productRepo.DeleteProduct(product);
                        var chacedProduct = await _Cache.GetStringAsync($"product:{product.Id}");
                        if (chacedProduct != null)
                        {
                            await _Cache.RemoveAsync($"product:{product.Id}");
                        }
                }
            }
            catch (Exception ex)
            {
                throw new ArgumentException("An error occurred while deleting the product.", ex.Message);
            }
        }

        public async Task<ProductDtoResponse> GetProductById(Guid id)
        {
            try
            {
                var cacheKey = $"product:{id}";
                var cachedProduct = await _Cache.GetStringAsync(cacheKey);
                if(cachedProduct != null)
                {
                    await _Cache.RefreshAsync(cacheKey);
                    return System.Text.Json.JsonSerializer.Deserialize<ProductDtoResponse>(cachedProduct);

                }
                var product = await _productRepo.ListProducts().Where(p => p.Id == id && p.isDeleted == false).Select(p => new ProductDtoResponse
                {
                    Id = p.Id,
                    BrandId = p.BrandId,
                    CategoryId = p.CategoryId,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    Quantity = p.Quantity,
                    Pictures = p.Images.Where(i => i.IsDeleted == false).Select(i => new PicturesDto { Id = i.Id, Path = i.Url }).ToList()
                }).FirstOrDefaultAsync();

                var productJson = System.Text.Json.JsonSerializer.Serialize(product);
                await _Cache.SetStringAsync(cacheKey, productJson, new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
                });
                return product;
            }
            catch (Exception ex)
            {
                throw new ArgumentException("An error occurred while retrieving the product.", ex);
            }
        }

        public async Task<List<ProductDtoResponse>> ListProducts(int pageNumber, int pageSize)
        {
            var products =await _productRepo.ListProducts().Where(p => p.isDeleted == false).Select(p => new ProductDtoResponse
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                Quantity = p.Quantity,
                CategoryId = p.CategoryId,
                BrandId =  p.BrandId,
                Pictures = p.Images.Where(i => i.IsDeleted == false).Select(i => new PicturesDto { Id = i.Id, Path = i.Url }).ToList()
            }).Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
            return products;
        }

        public async Task UpdateProduct(Guid id, ProductDto product)
        {
            try
            {
                var productToUpdate = await _productRepo.ListProducts().Include(p => p.Images).FirstOrDefaultAsync(p => p.Id == id && p.isDeleted == false);

                productToUpdate.Name = product.Name;
                productToUpdate.Description = product.Description;
                productToUpdate.Price = product.Price;
                productToUpdate.Quantity = product.Quantity;
                productToUpdate.CategoryId = product.CategoryId;
                productToUpdate.BrandId = product.brandId;
                if (product.Pictures != null)
                {
                    List<Images> newImages = new List<Images>();
                    if (productToUpdate.Images == null)
                    {
                        foreach (var picture in product.Pictures)
                        {
                            var path = GeneratePath(picture);
                            newImages.Add(new Images
                            {
                                Id = Guid.NewGuid(),
                                Product=productToUpdate,
                                ProductId=productToUpdate.Id,
                                Url = path,
                                CreatedAt = DateTime.UtcNow,
                                IsDeleted = false
                            });

                        }
                        await _databaseTransaction.ExecuteAsync(async () =>
                        {
                            await _productRepo.UpdateProduct(productToUpdate);
                            await _picturesRepo.AddImageRange(newImages);
                        });
                    }
                    else
                    {
                        foreach (var picture in productToUpdate.Images.Where(x=>x.IsDeleted==false))
                        {
                            picture.IsDeleted = true;
                        }
                        foreach (var picture in product.Pictures)
                        {
                            var path = GeneratePath(picture);
                            newImages.Add(new Images
                            {
                                Id = Guid.NewGuid(),
                                ProductId=productToUpdate.Id,
                                Product=productToUpdate,
                                Url = path,
                                CreatedAt = DateTime.UtcNow,
                                IsDeleted = false
                            });
                        }
                        await _databaseTransaction.ExecuteAsync(async () =>
                        {
                            await _productRepo.UpdateProduct(productToUpdate);
                            await _picturesRepo.AddImageRange(newImages);
                        });
                        foreach (var img in productToUpdate.Images.Where(i => i.IsDeleted == true))
                        {
                            File.Delete(img.Url);
                        }
                        foreach (var img in newImages)
                        {
                            await using var stream = new FileStream(img.Url, FileMode.Create);
                            var productPicture = product.Pictures.FirstOrDefault(p => p.FileName == Path.GetFileName(img.Url));
                            if (productPicture != null)
                            {
                                await productPicture.CopyToAsync(stream);
                            }
                        }
                    }
                }
                else
                {
                    await _databaseTransaction.ExecuteAsync(async () =>
                    {
                        await _productRepo.UpdateProduct(productToUpdate);
                    });
                }
            }

            catch (Exception ex)
            {
                throw new ArgumentException("An error occurred while updating the product.", ex);
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

        public async Task<List<ProductDtoResponse>> SearchProducts(string searchTerm)
        {
            try
            {
                var cacheKey = $"products:search:{searchTerm}";
                var cachedProducts = await _Cache.GetStringAsync(cacheKey);
                if (cachedProducts != null)
                {
                    return System.Text.Json.JsonSerializer.Deserialize<List<ProductDtoResponse>>(cachedProducts);
                }

                var products = await _productRepo.ListProducts().Where(p => p.isDeleted == false && (p.Name.Contains(searchTerm) || p.Description.Contains(searchTerm))).Select(p => new ProductDtoResponse
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    Quantity = p.Quantity,
                    CategoryId = p.CategoryId,
                    BrandId = p.BrandId,
                    Pictures = p.Images.Where(i => i.IsDeleted == false).Select(i => new PicturesDto { Id = i.Id, Path = i.Url }).ToList()
                }).ToListAsync();
                var productsJson = System.Text.Json.JsonSerializer.Serialize(products);
                await _Cache.SetStringAsync(cacheKey, productsJson, new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
                });
                return products;
            }
            catch (Exception ex)
            {
                throw new ArgumentException("An error occurred while searching for products.", ex.Message);
            }
        }
    }
}
