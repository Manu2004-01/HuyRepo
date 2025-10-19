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
    public class UserConfigration : IEntityTypeConfiguration<Users>
    {
        public void Configure(EntityTypeBuilder<Users> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .IsRequired()
                .ValueGeneratedOnAdd();

            builder.Property(x => x.UserName)
                .IsRequired()
                .HasColumnType("nvarchar(100)");

            builder.Property(x => x.RoleId)
                .IsRequired()
                .HasColumnType("int");

            builder.Property(x => x.Email)
                .IsRequired()
                .HasColumnType("varchar(250)");

            builder.Property(x => x.Password)
                .IsRequired()
                .HasColumnType("nvarchar(250)");

            builder.Property(x => x.PhoneNumber)
                .IsRequired()
                .HasColumnType("nvarchar(20)");

            builder.Property(x => x.UserAddress)
                .IsRequired()
                .HasColumnType("nvarchar(250)");

            builder.Property(x => x.GoogleId)
                .IsRequired(false)
                .HasColumnType("varchar(100)");

            builder.Property(x => x.ResetPasswordToken)
                .IsRequired(false)
                .HasColumnType("varchar(500)");

            builder.Property(x => x.ResetPasswordTokenExpiry)
                .IsRequired(false)
                .HasColumnType("datetime");

            builder.Property(x => x.UserLatitude)
                .IsRequired(false)
                .HasColumnType("float");

            builder.Property(x => x.UserLongitude)
                .IsRequired(false)
                .HasColumnType("float");

            builder.Property(x => x.LastLocationUpdate)
                .IsRequired(false)
                .HasColumnType("datetime");

            builder.Property(x => x.CreateAt)
                .IsRequired()
                .HasColumnType("datetime");

            builder.Property(x => x.UpdateAt)
                .IsRequired()
                .HasColumnType("datetime");

            builder.Property(x => x.IsActive)
                .IsRequired()
                .HasColumnType("bit");

            builder.HasOne(u => u.Role)
                   .WithMany(r => r.Users)
                   .HasForeignKey(u => u.RoleId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasData
                (
                    new Users
                    {
                        Id = 1,
                        UserImg = "http",
                        UserName = "Admin Huy Che",
                        RoleId = 1,
                        Email = "admin@eatit.com",
                        Password = "123456",
                        PhoneNumber = "0382727683",
                        UserAddress = "Thành phố Hồ Chí Minh",
                        CreateAt = new DateTime(2025, 08, 25, 0, 0, 0),
                        UpdateAt = new DateTime(2025, 08, 25, 0, 0, 0),
                        IsActive = true
                    },

                    new Users
                    {
                        Id = 2,
                        UserImg = "http",
                        UserName = "Huy Che",
                        RoleId = 2,
                        Email = "user@eatit.com",
                        Password = "654321",
                        PhoneNumber = "0912345678",
                        UserAddress = "Hà Nội",
                        CreateAt = new DateTime(2025, 08, 25, 0, 0, 0),
                        UpdateAt = new DateTime(2025, 08, 25, 0, 0, 0),
                        IsActive = true
                    },

                    new Users
                    {
                        Id = 3,
                        UserImg = "http",
                        UserName = "Nhà hàng đồ ăn",
                        RoleId = 3,
                        Email = "restaurant@eatit.com",
                        Password = "111111",
                        PhoneNumber = "0912345678",
                        UserAddress = "Đà Nẵng",
                        CreateAt = new DateTime(2025, 08, 25, 0, 0, 0),
                        UpdateAt = new DateTime(2025, 08, 25, 0, 0, 0),
                        IsActive = true
                    }
                );
        }
    }
}
