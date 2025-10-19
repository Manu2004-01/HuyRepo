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
