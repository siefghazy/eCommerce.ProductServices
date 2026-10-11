using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Product.Domain.Interfaces;
using Product.Infrastructure.Repositories;


namespace Product.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            var config=services.BuildServiceProvider().GetRequiredService<IConfiguration>();
            services.AddDbContext<ProductDbContext>(options =>
            {
                options.UseSqlServer(config.GetConnectionString("DefaultConnection"));
            });
            services.AddScoped<IProductRepo, ProductRepo>();
            services.AddScoped<IDatabaseTransaction, DatabaseTransaction>();
            services.AddScoped<IPicturesRepo, ImagesRepo>();
            return services;
        }
    }
}
