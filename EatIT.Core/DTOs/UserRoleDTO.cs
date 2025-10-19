using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EatIT.Infrastructure.Data.DTOs
{
    public class UserRoleDTO
    {
        [Required]
        public string RoleName { get; set; }
    }

    public class ListUserRoleDTO
    {
        public int Id { get; set; }
        public string RoleName { get; set; }
    }
}
