using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace eCommerce.ProductServices.Middlewares
{
    public class GlobalErrorHandler
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalErrorHandler> _logger;

        public GlobalErrorHandler(RequestDelegate next, ILogger<GlobalErrorHandler> logger)
        {
            _next = next;
            _logger = logger;
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
                    _logger.LogError(ex, "Unhandled exception on {Method} {Path}",
                        httpContext.Request.Method, httpContext.Request.Path);
                    httpContext.Response.StatusCode = 500;
                    await httpContext.Response.WriteAsync($"An unexpected error occurred: {ex.Message}");
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
