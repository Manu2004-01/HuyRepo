using EatIT.Core.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EatIT.Infrastructure.Data
{
    public class ApplicationDBContext : DbContext
    {
        public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options) : base(options)
        {
        }

        public virtual DbSet<UserRole> UserRoles { get; set; }
        public virtual DbSet<Users> Users { get; set; }
        public virtual DbSet<Dishes> Dishes { get; set; }
        public virtual DbSet<Rating> Ratings { get; set; }
        public virtual DbSet<Tags> Tags { get; set; }
        public virtual DbSet<Favorites> Favorites { get; set; }
        public virtual DbSet<Restaurants> Restaurants { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Configure table names to match Supabase (lowercase)
            modelBuilder.Entity<Dishes>().ToTable("dishes");
            modelBuilder.Entity<Restaurants>().ToTable("restaurants");
            modelBuilder.Entity<Users>().ToTable("users");
            modelBuilder.Entity<Favorites>().ToTable("favorites");
            modelBuilder.Entity<Rating>().ToTable("rating");
            modelBuilder.Entity<Tags>().ToTable("tags");
            modelBuilder.Entity<UserRole>().ToTable("user_roles");
            
            // Configure column names to use snake_case (PostgreSQL convention)
            ConfigureColumnNames(modelBuilder);
            
            // Configure decimal precision and scale
            modelBuilder.Entity<Dishes>()
                .Property(d => d.DishPrice)
                .HasPrecision(18, 2);
            
            // Configure relationships
            modelBuilder.Entity<Dishes>()
                .HasOne(d => d.Restaurant)
                .WithMany(r => r.Dishes)
                .HasForeignKey(d => d.ResId)
                .OnDelete(DeleteBehavior.Restrict);
            
            modelBuilder.Entity<Users>()
                .HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Restrict);
            
            modelBuilder.Entity<Restaurants>()
                .HasOne(r => r.Tag)
                .WithMany(t => t.Restaurants)
                .HasForeignKey(r => r.TagId)
                .OnDelete(DeleteBehavior.Restrict);
            
            modelBuilder.Entity<Favorites>()
                .HasOne(f => f.User)
                .WithMany(u => u.Favorites)
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            
            modelBuilder.Entity<Favorites>()
                .HasOne(f => f.Dish)
                .WithMany()
                .HasForeignKey(f => f.DishId)
                .OnDelete(DeleteBehavior.Restrict);
            
            modelBuilder.Entity<Favorites>()
                .HasOne(f => f.Restaurant)
                .WithMany(r => r.Favorites)
                .HasForeignKey(f => f.RestaurantId)
                .OnDelete(DeleteBehavior.Restrict);
            
            modelBuilder.Entity<Rating>()
                .HasOne(r => r.User)
                .WithMany(u => u.Ratings)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            
            modelBuilder.Entity<Rating>()
                .HasOne(r => r.Restaurant)
                .WithMany(r => r.Ratings)
                .HasForeignKey(r => r.RestaurantId)
                .OnDelete(DeleteBehavior.Restrict);
            
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }

        public override int SaveChanges()
        {
            ApplyTimeStamps();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            ApplyTimeStamps();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void ConfigureColumnNames(ModelBuilder modelBuilder)
        {
            // UserRole
            modelBuilder.Entity<UserRole>()
                .Property(u => u.RoleId).HasColumnName("role_id");
            modelBuilder.Entity<UserRole>()
                .Property(u => u.RoleName).HasColumnName("role_name");
            
            // Users
            modelBuilder.Entity<Users>()
                .Property(u => u.UserId).HasColumnName("user_id");
            modelBuilder.Entity<Users>()
                .Property(u => u.RoleId).HasColumnName("role_id");
            modelBuilder.Entity<Users>()
                .Property(u => u.UserImg).HasColumnName("user_img");
            modelBuilder.Entity<Users>()
                .Property(u => u.UserName).HasColumnName("user_name");
            modelBuilder.Entity<Users>()
                .Property(u => u.Email).HasColumnName("email");
            modelBuilder.Entity<Users>()
                .Property(u => u.Password).HasColumnName("password");
            modelBuilder.Entity<Users>()
                .Property(u => u.PhoneNumber).HasColumnName("phone_number");
            modelBuilder.Entity<Users>()
                .Property(u => u.UserAddress).HasColumnName("user_address");
            modelBuilder.Entity<Users>()
                .Property(u => u.Preference).HasColumnName("preference");
            modelBuilder.Entity<Users>()
                .Property(u => u.Dislike).HasColumnName("dislike");
            modelBuilder.Entity<Users>()
                .Property(u => u.Allergy).HasColumnName("allergy");
            modelBuilder.Entity<Users>()
                .Property(u => u.Diet).HasColumnName("diet");
            modelBuilder.Entity<Users>()
                .Property(u => u.ResetPasswordToken).HasColumnName("reset_password_token");
            modelBuilder.Entity<Users>()
                .Property(u => u.ResetPasswordTokenExpiry).HasColumnName("reset_password_token_expiry");
            modelBuilder.Entity<Users>()
                .Property(u => u.UserLatitude).HasColumnName("user_latitude");
            modelBuilder.Entity<Users>()
                .Property(u => u.UserLongitude).HasColumnName("user_longitude");
            modelBuilder.Entity<Users>()
                .Property(u => u.LastLocationUpdate).HasColumnName("last_location_update");
            modelBuilder.Entity<Users>()
                .Property(u => u.CreateAt).HasColumnName("create_at");
            modelBuilder.Entity<Users>()
                .Property(u => u.UpdateAt).HasColumnName("update_at");
            modelBuilder.Entity<Users>()
                .Property(u => u.IsVegetarian).HasColumnName("is_vegetarian");
            modelBuilder.Entity<Users>()
                .Property(u => u.IsActive).HasColumnName("is_active");
            
            // Tags
            modelBuilder.Entity<Tags>()
                .Property(t => t.TagId).HasColumnName("tag_id");
            modelBuilder.Entity<Tags>()
                .Property(t => t.TagName).HasColumnName("tag_name");
            modelBuilder.Entity<Tags>()
                .Property(t => t.TagImg).HasColumnName("tag_img");
            
            // Restaurants
            modelBuilder.Entity<Restaurants>()
                .Property(r => r.ResId).HasColumnName("res_id");
            modelBuilder.Entity<Restaurants>()
                .Property(r => r.TagId).HasColumnName("tag_id");
            modelBuilder.Entity<Restaurants>()
                .Property(r => r.StarRating).HasColumnName("star_rating");
            modelBuilder.Entity<Restaurants>()
                .Property(r => r.ResName).HasColumnName("res_name");
            modelBuilder.Entity<Restaurants>()
                .Property(r => r.RestaurantImg).HasColumnName("restaurant_img");
            modelBuilder.Entity<Restaurants>()
                .Property(r => r.ResAddress).HasColumnName("res_address");
            modelBuilder.Entity<Restaurants>()
                .Property(r => r.ResPhoneNumber).HasColumnName("res_phone");
            modelBuilder.Entity<Restaurants>()
                .Property(r => r.Latitude).HasColumnName("latitude");
            modelBuilder.Entity<Restaurants>()
                .Property(r => r.Longitude).HasColumnName("longitude");
            modelBuilder.Entity<Restaurants>()
                .Property(r => r.OpeningHours).HasColumnName("opening_hours");
            
            // Dishes
            modelBuilder.Entity<Dishes>()
                .Property(d => d.DishId).HasColumnName("dish_id");
            modelBuilder.Entity<Dishes>()
                .Property(d => d.ResId).HasColumnName("res_id");
            modelBuilder.Entity<Dishes>()
                .Property(d => d.DishName).HasColumnName("dish_name");
            modelBuilder.Entity<Dishes>()
                .Property(d => d.DishDescription).HasColumnName("dish_description");
            modelBuilder.Entity<Dishes>()
                .Property(d => d.DishPrice).HasColumnName("dish_price");
            modelBuilder.Entity<Dishes>()
                .Property(d => d.DishImage).HasColumnName("dish_image");
            modelBuilder.Entity<Dishes>()
                .Property(d => d.CreateAt).HasColumnName("create_at");
            modelBuilder.Entity<Dishes>()
                .Property(d => d.UpdateAt).HasColumnName("update_at");
            modelBuilder.Entity<Dishes>()
                .Property(d => d.IsVegan).HasColumnName("is_vegan");
            
            // Rating
            modelBuilder.Entity<Rating>()
                .Property(r => r.Id).HasColumnName("id");
            modelBuilder.Entity<Rating>()
                .Property(r => r.UserId).HasColumnName("user_id");
            modelBuilder.Entity<Rating>()
                .Property(r => r.RestaurantId).HasColumnName("restaurant_id");
            modelBuilder.Entity<Rating>()
                .Property(r => r.Star).HasColumnName("star");
            modelBuilder.Entity<Rating>()
                .Property(r => r.Comment).HasColumnName("comment");
            modelBuilder.Entity<Rating>()
                .Property(r => r.CreateAt).HasColumnName("create_at");
            
            // Favorites
            modelBuilder.Entity<Favorites>()
                .Property(f => f.FavorId).HasColumnName("favor_id");
            modelBuilder.Entity<Favorites>()
                .Property(f => f.DishId).HasColumnName("dish_id");
            modelBuilder.Entity<Favorites>()
                .Property(f => f.UserId).HasColumnName("user_id");
            modelBuilder.Entity<Favorites>()
                .Property(f => f.RestaurantId).HasColumnName("restaurant_id");
        }

        private void ApplyTimeStamps()
        {
            var now = DateTime.UtcNow;
            foreach (var entry in ChangeTracker.Entries().Where(e => e.State == EntityState.Added || e.State == EntityState.Modified)) 
            {
                var hasCreateAt = entry.Metadata.FindProperty("CreateAt") != null;
                var hasUpdateAt = entry.Metadata.FindProperty("UpdateAt") != null;
                if (entry.State == EntityState.Added)
                {
                    if (hasCreateAt && (entry.Property("CreateAt").CurrentValue == null || (DateTime)entry.Property("CreateAt").CurrentValue == default))
                        entry.Property("CreateAt").CurrentValue = now;

                    if (hasUpdateAt && (entry.Property("UpdateAt").CurrentValue == null || (DateTime)entry.Property("UpdateAt").CurrentValue == default))
                        entry.Property("UpdateAt").CurrentValue = now;
                }
                else if (entry.State == EntityState.Modified && hasUpdateAt)
                {
                    entry.Property("UpdateAt").CurrentValue = now;
                }
            }
        }
    }
}
