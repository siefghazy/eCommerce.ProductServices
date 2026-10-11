using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace eCommerce.ProductServices.Middlewares
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    public class GlobalErrorHandler
    {
        private readonly RequestDelegate _next;
       

        public GlobalErrorHandler(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext)
        {

            try
            {
                await _next(httpContext);
            }
            catch (Exception ex)
            {
             
                if (ex is ArgumentException)
                {
                    
                    httpContext.Response.StatusCode = 400;
                     await httpContext.Response.WriteAsync(ex.Message);
                }
                else if (ex is KeyNotFoundException)
                {
                    
                    httpContext.Response.StatusCode = 404;
                     await httpContext.Response.WriteAsync(ex.Message);
                }
                else if (ex is UnauthorizedAccessException)
                {
                   
                    httpContext.Response.StatusCode = 401;
                     await httpContext.Response.WriteAsync(ex.Message);
                }
                else if (ex is NullReferenceException)
                {
                   
                    httpContext.Response.StatusCode = 400;
                     await httpContext.Response.WriteAsync(ex.Message);
                }
                else
                {
                    
                    httpContext.Response.StatusCode = 500;
                     await httpContext.Response.WriteAsync("An unexpected error occurred.");
                }
            }
        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class GlobalErrorHandlerExtensions
    {
        public static IApplicationBuilder UseGlobalErrorHandler(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<GlobalErrorHandler>();
        }
    }
}
