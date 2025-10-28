using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EatIT.Core.Entities
{
    public class Restaurants 
    {
        [Key]
        public int ResId { get; set; }
        public int TagId { get; set; }
        public double StarRating { get; set; } = 0;
        public string ResName { get; set; } = "";
        public string? RestaurantImg { get; set; }
        public string ResAddress { get; set; }
        public long? ResPhoneNumber { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string OpeningHours { get; set; }
        public Tags Tag { get; set; }

        public ICollection<Rating> Ratings { get; set; } = new HashSet<Rating>();
        public ICollection<Dishes> Dishes { get; set; } = new HashSet<Dishes>();
        public ICollection<Favorites> Favorites { get; set; } = new HashSet<Favorites>();
    }
}
