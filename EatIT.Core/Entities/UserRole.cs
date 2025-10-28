using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EatIT.Core.Entities
{
    public class UserRole 
    {
        [Key]
        public int RoleId { get; set; }
        public string RoleName { get; set; } = "";
        public virtual ICollection<Users> Users { get; set; } = new HashSet<Users>();
    }
}
