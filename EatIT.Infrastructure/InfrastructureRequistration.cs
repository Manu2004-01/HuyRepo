using EatIT.Core.Interface;
using EatIT.Infrastructure.Data;
using EatIT.Infrastructure.Repository;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Builder;
using EatIT.Core.Entities;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace EatIT.Infrastructure
{
    public static class InfrastructureRequistration
    {
        public static IServiceCollection InfraStructureConfigration (this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IUserRoleRepository, UserRoleRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IRestaurantRepository, RestaurantRepository>();
            services.AddScoped<IDishRepository, DishRepository>();
            services.AddScoped<IFavoriteRepository, FavoriteRepository>();
            services.AddScoped<ITagRepository, TagRepository>();
            services.AddScoped<IRatingRepository, RatingRepository>();
            services.AddDbContext<ApplicationDBContext>(option =>
            {
                option.UseNpgsql(configuration.GetConnectionString("DefaultConnection"), npgsqlOptions =>
                {
                    npgsqlOptions.CommandTimeout(120); // Tăng timeout lên 120 giây
                });
                option.LogTo(Console.WriteLine, Microsoft.Extensions.Logging.LogLevel.Information);
            }
            );
            //services.AddIdentity<Users, UserRole>().AddEntityFrameworkStores<ApplicationDBContext>().AddDefaultTokenProviders();
            services.AddMemoryCache();
            //services.AddAuthentication(opt =>
            //{
            //    opt.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            //});
            return services;
        }

        public static async Task InfrastructureConfigMiddleware(IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
            
            // Check if migrations have been applied
            var canConnect = await context.Database.CanConnectAsync();
            if (canConnect)
            {
                // Database exists and is accessible
                Console.WriteLine("Database connection successful. Migrations handled separately.");
            }
            else
            {
                // Database doesn't exist, ensure it's created
                await context.Database.EnsureCreatedAsync();
            }
        }
    }
}
