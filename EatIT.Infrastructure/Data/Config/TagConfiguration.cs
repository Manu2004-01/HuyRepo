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
    public class TagConfiguration : IEntityTypeConfiguration<Tags>
    {
        public void Configure(EntityTypeBuilder<Tags> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                   .IsRequired()
                   .ValueGeneratedOnAdd();

            builder.Property(x => x.TagName)
                   .IsRequired()
                   .HasColumnType("nvarchar(150)");

            builder.HasData
                (
                    new Tags { Id = 1, TagName = "Cơm tấm" , TagImg = null},
                    new Tags { Id = 2, TagName = "Món nước", TagImg = null },
                    new Tags { Id = 3, TagName = "Trà sữa", TagImg = null },
                    new Tags { Id = 4, TagName = "Thức ăn nhanh", TagImg = null },
                    new Tags { Id = 5, TagName = "Bánh mì", TagImg = null },
                    new Tags { Id = 6, TagName = "Chè & Tráng miệng", TagImg = null },
                    new Tags { Id = 7, TagName = "Cà phê & Nước uống", TagImg = null },
                    new Tags { Id = 8, TagName = "Hải sản", TagImg = null },
                    new Tags { Id = 9, TagName = "Nướng & BBQ", TagImg = null },
                    new Tags { Id = 10, TagName = "Lẩu", TagImg = null },
                    new Tags { Id = 11, TagName = "Quán nhậu/Bia hơi", TagImg = null },
                    new Tags { Id = 12, TagName = "Ăn vặt/Vỉa hè", TagImg = null },
                    new Tags { Id = 13, TagName = "Cơm văn phòng", TagImg = null },
                    new Tags { Id = 14, TagName = "Chay", TagImg = null },
                    new Tags { Id = 15, TagName = "Ẩm thực miền Bắc", TagImg = null },
                    new Tags { Id = 16, TagName = "Ẩm thực miền Trung", TagImg = null },
                    new Tags { Id = 17, TagName = "Ẩm thực miền Nam", TagImg = null },
                    new Tags { Id = 18, TagName = "Ẩm thực Tây Âu", TagImg = null },
                    new Tags { Id = 19, TagName = "Ẩm thực Nhật/Hàn/Trung", TagImg = null }
                );
        }
    }
}
