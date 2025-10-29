using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EatIT.Core.DTOs
{
    public class BaseDish
    {
        [Required(ErrorMessage = "Tên món ăn là bắt buộc")]
        [MaxLength(100, ErrorMessage = "Tên món ăn không được vượt quá 100 ký tự")]
        public string DishName { get; set; }

        [Required(ErrorMessage = "Tên món ăn là bắt buộc")]
        [MaxLength(500, ErrorMessage = "Mô tả món ăn không được vượt quá 500 ký tự")]
        public string? DishDescription { get; set; }

        [Required(ErrorMessage = "Giá món ăn là bắt buộc")]
        [Range(1000, 10000000, ErrorMessage = "Giá món ăn phải từ 1.000 đến 10.000.000 VND")]
        public decimal DishPrice { get; set; }
        
        public bool IsVegan { get; set; }
    }

    public class DishDTO : BaseDish
    {
        public int Id { get; set; }
        public string DishName { get; set; }
        public string DishImage { get; set; }
        public string DishDescription { get; set; }
        public decimal DishPrice { get; set; }
        public bool IsVegan { get; set; }
        //public DateTime CreateAt { get; set; }
        //public DateTime UpdateAt { get; set; }
    }

    public class DishBasicDTO
    {
        public int Id { get; set; }
        public string DishName { get; set; }
        public string DishImage { get; set; }
        public string DishDescription { get; set; }
        public decimal DishPrice { get; set; }
        public bool IsVegan { get; set; }
    }

    public class CreateDishDTO : BaseDish
    {
        public int restaurantid { get; set; }
        public IFormFile dimage { get; set; }
    }

    public class UpdateDishDTO : BaseDish
    {
        public int restaurantid { get; set; }
        public IFormFile dimage { get; set; }
    }
}
