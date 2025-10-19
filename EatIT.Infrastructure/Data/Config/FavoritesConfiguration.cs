using EatIT.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EatIT.Infrastructure.Data.Config
{
    public class FavoritesConfiguration : IEntityTypeConfiguration<Favorites>
    {
        public void Configure(EntityTypeBuilder<Favorites> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .IsRequired()
                .ValueGeneratedOnAdd();

            builder.HasOne(x => x.User)
                .WithMany(u => u.Favorites)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Dish)
                .WithMany(d => d.Favorites)
                .HasForeignKey(x => x.DishId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.Restaurant)
                .WithMany(r => r.Favorites)
                .HasForeignKey(x => x.RestaurantId)
                .OnDelete(DeleteBehavior.NoAction);

        }
    }
}
