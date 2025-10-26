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
    public class DishesConfiguration : IEntityTypeConfiguration<Dishes>
    {
        public void Configure(EntityTypeBuilder<Dishes> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .IsRequired()
                .ValueGeneratedOnAdd();

            builder.Property(x => x.DishName)
                .IsRequired()
                .HasColumnType("nvarchar(250)");
            builder.Property(x => x.DishDescription)
                .IsRequired()
                .HasColumnType("ntext");
            builder.Property(x => x.DishPrice)
                .IsRequired()
                .HasColumnType("decimal(10,2)");
            builder.Property(x => x.DishImage)
                .IsRequired()
                .HasColumnType("text");

            builder.HasOne(x => x.Restaurant)
                .WithMany(r => r.Dishes)
                .HasForeignKey(x => x.ResId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
