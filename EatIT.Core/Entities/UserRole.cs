using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EatIT.Core.Entities
{
    public class UserRole : BasicEntity<int>
    {
        //public int RoleId { get; set; }
        public string RoleName { get; set; } = "";
        public virtual ICollection<Users> Users { get; set; } = new HashSet<Users>();
    }
}
