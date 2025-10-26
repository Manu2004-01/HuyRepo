using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EatIT.Core.DTOs
{
    public class BaseRestaurant
    {
        [Required(ErrorMessage = "Tên nhà hàng là bắt buộc")]
        [MaxLength(100, ErrorMessage = "Tên nhà hàng không được vượt quá 100 ký tự")]
        public string ResName { get; set; }

        [Required(ErrorMessage = "Địa chỉ nhà hàng là bắt buộc")]
        public string ResAddress { get; set; }

        [Required(ErrorMessage = "Vĩ độ là bắt buộc")]
        [Range(-90, 90, ErrorMessage = "Vĩ độ phải nằm trong khoảng -90 đến 90")]
        public float Latitude { get; set; }

        [Required(ErrorMessage = "Kinh độ là bắt buộc")]
        [Range(-180, 180, ErrorMessage = "Kinh độ phải nằm trong khoảng -180 đến 180")]
        public float Longitude { get; set; }

        [Required(ErrorMessage = "Thời gian mở cửa của nhà hàng là bắt buộc")]
        public string OpeningHours { get; set; }
    }

    public class RestaurantDTO : BaseRestaurant
    {
        public int Id { get; set; }
        public string TagName { get; set; }
        public string? RestaurantImg { get; set; }

        public double? DistanceFromUser { get; set; }
        public string? DistanceDisplay { get; set; }
    }

    public class CreateRestaurantDTO : BaseRestaurant
    {
        public int tagid { get; set; }
        public IFormFile rimage { get; set; }
    }

    public class UpdateRestaurantDTO : BaseRestaurant
    {
        public int tagid { get; set; }
        public IFormFile rimage { get; set; }
    }
}
