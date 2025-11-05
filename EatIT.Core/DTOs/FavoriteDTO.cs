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

    // DTO dành cho client request (không có userid - sẽ lấy từ JWT token)
    public class CreateFavoriteRequestDTO 
    {
        [Required(ErrorMessage = "Món ăn là bắt buộc")]
        public int dishid { get; set; }
        
        [Required(ErrorMessage = "Nhà hàng là bắt buộc")]
        public int restaurantid { get; set; }
    }

    // DTO dành cho client request (không có userid - sẽ lấy từ JWT token)
    public class UpdateFavoriteRequestDTO 
    {
        public int? dishid { get; set; }
        public int? restaurantid { get; set; }
    }

    // DTO nội bộ (sử dụng trong repository)
    public class CreateFavoriteDTO 
    {
        public int? dishid { get; set; }
        public int userid { get; set; } // Được set từ JWT token trong controller
        public int? restaurantid { get; set; }
    }

    // DTO nội bộ (sử dụng trong repository)
    public class UpdateFavoriteDTO 
    {
        public int? dishid { get; set; }
        public int userid { get; set; } // Được set từ JWT token trong controller
        public int? restaurantid { get; set; }
    }
}