using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EatIT.Core.Entities
{
    public class Dishes : BasicEntity<int>
    {
        //public int Id { get; set; }
        public int ResId { get; set; }
        public string DishName { get; set; } = "";
        public string? DishDescription { get; set; }
        public decimal DishPrice { get; set; }
        public string? DishImage { get; set; }
        public DateTime CreateAt { get; set; }
        public DateTime UpdateAt { get; set; }
        public bool IsVegan { get; set; }

        public Restaurants Restaurant { get; set; }

        public ICollection<Favorites> Favorites { get; set; }
    }
}
