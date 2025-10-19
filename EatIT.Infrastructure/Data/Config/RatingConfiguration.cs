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
    public class RatingConfiguration : IEntityTypeConfiguration<Rating>
    {
        public void Configure(EntityTypeBuilder<Rating> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .IsRequired()
                .ValueGeneratedOnAdd();

            builder.Property(x => x.RestaurantId)
                .IsRequired()
                .HasColumnType("int");

            builder.Property(x => x.Star)
                .IsRequired()
                .HasColumnType("int");

            builder.Property(x => x.Comment)
                .IsRequired()
                .HasColumnType("text");

            builder.Property(x => x.CreateAt)
                .IsRequired()
                .HasColumnType("datetime");

            builder.HasOne(x => x.User)
                .WithMany(u => u.Ratings)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Restaurant)
                .WithMany(u => u.Ratings)
                .HasForeignKey(x => x.RestaurantId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
