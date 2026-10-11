using Microsoft.Extensions.DependencyInjection;
using Product.Core.Interfaces;
using Product.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Product.Core
{
    public static class DependecyInjection
    {
        public static IServiceCollection AddCore(this IServiceCollection services)
        {
            services.AddScoped<IBrandService, BrandService>();
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<IPictureService, PictureService>();
            services.AddScoped<IProductService, ProductService>();
            return services;
        }
    }
}
