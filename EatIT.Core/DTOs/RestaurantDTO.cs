using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace EatIT.Core.DTOs
{
    public class BaseRestaurant
    {
        [JsonPropertyOrder(5)]
        public double? StarRating { get; set; }

        [Required(ErrorMessage = "Tên nhà hàng là bắt buộc")]
        [MaxLength(100, ErrorMessage = "Tên nhà hàng không được vượt quá 100 ký tự")]
        [JsonPropertyOrder(3)]
        public string ResName { get; set; }

        [Required(ErrorMessage = "Địa chỉ nhà hàng là bắt buộc")]
        [JsonPropertyOrder(4)]
        public string ResAddress { get; set; }

        [JsonPropertyOrder(6)]
        public long? ResPhoneNumber { get; set; }

        [Required(ErrorMessage = "Vĩ độ là bắt buộc")]
        [Range(-90, 90, ErrorMessage = "Vĩ độ phải nằm trong khoảng -90 đến 90")]
        [JsonPropertyOrder(8)]
        public double Latitude { get; set; }

        [Required(ErrorMessage = "Kinh độ là bắt buộc")]
        [Range(-180, 180, ErrorMessage = "Kinh độ phải nằm trong khoảng -180 đến 180")]
        [JsonPropertyOrder(9)]
        public double Longitude { get; set; }

        [JsonPropertyOrder(7)]
        public string? OpeningHours { get; set; }
    }

    public class RestaurantDTO : BaseRestaurant
    {
        [JsonPropertyOrder(0)]
        public int Id { get; set; }
        [JsonPropertyOrder(1)]
        public string TagName { get; set; }
        [JsonPropertyOrder(2)]
        public string? RestaurantImg { get; set; }

        [JsonPropertyOrder(10)]
        public double? DistanceFromUser { get; set; }
        [JsonPropertyOrder(11)]
        public string? DistanceDisplay { get; set; }
    }

    public class RestaurantDetailDTO : BaseRestaurant
    {
        [JsonPropertyOrder(0)]
        public int Id { get; set; }
        [JsonPropertyOrder(1)]
        public string TagName { get; set; }
        [JsonPropertyOrder(2)]
        public string? RestaurantImg { get; set; }
        [JsonPropertyOrder(12)]
        public List<DishBasicDTO> Dishes { get; set; }
    }

    public class CreateRestaurantDTO : BaseRestaurant
    {
        public int tagid { get; set; }
        public IFormFile? rimage { get; set; }
    }

    public class UpdateRestaurantDTO : BaseRestaurant
    {
        public int tagid { get; set; }
        public IFormFile? rimage { get; set; }
    }
}
