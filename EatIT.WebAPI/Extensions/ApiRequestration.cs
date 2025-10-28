using EatIT.Core.Interface;
using EatIT.Core.Services;
using EatIT.WebAPI.Errors;
using EatIT.WebAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EatIT.WebAPI.Extensions
{
    public static class ApiRequestration
    {
        public static IServiceCollection AddAPIRequestration(this IServiceCollection services)
        {
            //AutoMapper
            services.AddAutoMapper((serviceProvider, cfg) =>
            {
                // Configure AutoMapper to use the service provider
                var configuration = serviceProvider.GetRequiredService<IConfiguration>();
                cfg.ConstructServicesUsing(serviceProvider.GetService);
            },
                typeof(MappingUser).Assembly);

            //FileProvider
            services.AddSingleton<IFileProvider>
                (
                    new PhysicalFileProvider(Path.Combine
                    (
                        Directory.GetCurrentDirectory(), "wwwroot")
                    )
                );

            services.Configure<ApiBehaviorOptions>(opt =>
            {
                opt.InvalidModelStateResponseFactory = context =>
                {
                    var errorReponse = new ApiValidationErrorResponse
                    {
                        Errors = context.ModelState
                            .Where(x => x.Value.Errors.Count() > 0)
                            .SelectMany(x => x.Value.Errors)
                            .Select(x => x.ErrorMessage).ToArray()
                    };
                    return new BadRequestObjectResult(errorReponse);
                };
            });

            //Enable CORS
            // CORS: (Cross-Origin Resource Sharing) là một cơ chế bảo mật của trình duyệt, cho phép một trang web truy cập các tài nguyên (như API, dữ liệu, phông chữ) từ một nguồn gốc khác (miền, giao thức, hoặc cổng khác) một cách có kiểm soát, thay vì bị chặn bởi chính sách cùng nguồn gốc (Same-Origin Policy)
            services.AddCors (opt => 
            {
                opt.AddPolicy("CorsPolicy", pol =>
                {
                    pol.AllowAnyHeader()
                       .AllowAnyMethod()
                       .WithOrigins("https://huyrepo.onrender.com", "https://eatit-two.vercel.app", "https://localhost:7091", "http://localhost:5192", "https://localhost:5192", "http://localhost:7091");
                });
            });

            //Token 
            services.AddScoped<ITokenService, TokenService>();

            return services;
        }
    }
}
