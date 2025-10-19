using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EatIT.Core.DTOs
{
    public class BaseRating
    {
        [Required]
        public int Star { get; set; }

        [MaxLength(100, ErrorMessage = "Comment không được vượt quá 100 ký tự")]
        public string? Comment { get; set; }
    }

    public class RatingDTO : BaseRating 
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string RestaurantName { get; set; }
        public DateTime CreateAt { get; set; }
    }

    public class CreateRatingDTO : BaseRating
    {
        public int userid { get; set; }
        public int restaurantid { get; set; }
    }

    public class UpdateRatingDTO : BaseRating
    {
        public int userid { get; set; }
        public int restaurantid { get; set; }
    }
}
