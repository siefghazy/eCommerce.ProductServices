using eCommerce.ProductServices.Middlewares;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Product.Core;
using Product.Core.Dtos;
using Product.Core.Services;
using Product.Infrastructure;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddCore();
builder.Services.AddInfrastructure();
builder.Services.AddControllers();
string ConnectionString = "";
var ENV = Environment.GetEnvironmentVariable("ENV");
if (ENV == null)
{
    ConnectionString = "server=WIN-JKQLDIQ02OA\\SQLEXPRESS;database=ProductService;trusted_connection=True;TrustServerCertificate=True;";
}
else
{
    ConnectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? "Server=sql-server,1433;Database=Product_Services;User Id=sa;Password=Your_Strong_Password123!;TrustServerCertificate=True;";
}
builder.Services.AddDbContext<ProductDbContext>(options =>
{
    options.UseSqlServer(ConnectionString);
});
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    builder.Configuration["Jwt:Key"]!
                )
            ),
            RoleClaimType = "role"
        };
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = async context =>
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json";

                await context.Response.WriteAsJsonAsync(new
                {
                    message = context.Exception.Message
                });
            }
        };

    });
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration =
        builder.Configuration.GetConnectionString("Redis");
});

// Persist DataProtection keys to the mounted volume so they survive container restarts.
// The path matches the volume mount in docker-compose.yaml.
if (ENV != null)
{
    builder.Services.AddDataProtection()
        .PersistKeysToFileSystem(new DirectoryInfo("/home/app/.aspnet/DataProtection-Keys"));
}

builder.Services.AddValidatorsFromAssemblyContaining<ProductDto>();
builder.Services.AddFluentValidationAutoValidation();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Auto-apply EF migrations on startup (creates the DB if it doesn't exist).
// Safe to run on every start — EF skips already-applied migrations.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ProductDbContext>();
    await db.Database.MigrateAsync();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// In Docker, TLS is terminated upstream — skip HTTPS redirection inside the container.
if (ENV == null)
{
    app.UseHttpsRedirection();
}

app.UseAuthorization();

app.MapControllers();
app.UseGlobalErrorHandler();

app.Run();
