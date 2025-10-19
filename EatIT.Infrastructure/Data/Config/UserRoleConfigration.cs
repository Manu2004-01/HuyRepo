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
    public class UserRoleConfigration : IEntityTypeConfiguration<UserRole>
    {
        public void Configure(EntityTypeBuilder<UserRole> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .IsRequired()
                .ValueGeneratedOnAdd();

            builder.Property(x => x.RoleName)
                .IsRequired()
                .HasColumnType("nvarchar(50)");

            builder.HasData
                (
                    new UserRole { Id = 1, RoleName = "Admin"},
                    new UserRole { Id = 2, RoleName = "Người dùng" },
                    new UserRole { Id = 3, RoleName = "Nhà hàng" }
                );
        }
    }
}
