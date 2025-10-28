using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EatIT.Core.Entities
{
    public class Rating 
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; }
        public int RestaurantId { get; set; }
        public int Star { get; set; }
        public string? Comment { get; set; }
        public DateTime CreateAt { get; set; }

        public Users? User { get; set; }
        public Restaurants Restaurant { get; set; }
    }
}
