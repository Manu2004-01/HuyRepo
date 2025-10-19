using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EatIT.Core.Entities
{
    public class Favorites : BasicEntity<int>
    {
        //public int FavorId { get; set; }
        public int? DishId { get; set; }
        public int UserId { get; set; }
        public int RestaurantId { get; set; }

        public Users User { get; set; }
        public Dishes Dish { get; set; }
        public Restaurants Restaurant { get; set; }
    }
}
