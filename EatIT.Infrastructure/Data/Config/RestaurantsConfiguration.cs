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
    public class RestaurantsConfiguration : IEntityTypeConfiguration<Restaurants>
    {
        public void Configure(EntityTypeBuilder<Restaurants> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.ResName)
                .IsRequired()
                .HasColumnType("nvarchar(150)");

            builder.Property(x => x.ResAddress)
                .IsRequired()
                .HasColumnType("nvarchar(150)");

            builder.Property(x => x.Latitude)
                .IsRequired()
                .HasColumnType("float");

            builder.Property(x => x.Longitude)
                .IsRequired()
                .HasColumnType("float");

            builder.Property(x => x.OpeningHours)
                .IsRequired()
                .HasColumnType("nvarchar(150)");

            builder.HasOne(x => x.Tag)
                .WithMany(t => t.Restaurants)
                .HasForeignKey(x => x.TagId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasMany(x => x.Ratings)
                .WithOne(r => r.Restaurant)
                .HasForeignKey(x => x.RestaurantId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
