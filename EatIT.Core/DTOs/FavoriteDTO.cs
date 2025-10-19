using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EatIT.Core.DTOs
{
    public class FavoriteDTO 
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string DishName { get; set; }
        public string RestaurantName { get; set; }
    }

    public class CreateFavoriteDTO 
    {
        public int? dishid { get; set; }
        public int userid { get; set; }
        public int restaurantid { get; set; }
    }

    public class UpdateFavoriteDTO 
    {
        public int? dishid { get; set; }
        public int userid { get; set; }
        public int restaurantid { get; set; }
    }
}