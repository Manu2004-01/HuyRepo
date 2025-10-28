using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EatIT.Core.Entities
{
    public class Tags 
    {
        [Key]
        public int TagId { get; set; }
        public string TagName { get; set; } = "";
        public string? TagImg { get; set; }

        public ICollection<Restaurants> Restaurants { get; set; } = new HashSet<Restaurants>();
    }
}
