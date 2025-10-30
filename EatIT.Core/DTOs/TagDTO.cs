using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EatIT.Core.DTOs
{
    public class BaseTag
    {
        [Required(ErrorMessage = "Tên thẻ là bắt buộc")]
        [MaxLength(50, ErrorMessage = "TTên thẻ không được vượt quá 50 ký tự")]
        public string TagName { get; set; }
    }

    public class TagDTO : BaseTag
    {
        public int TagID { get; set; }
        public string TagName { get; set; }
        public string TagImg { get; set; }

        public class TagByIdRestaurantDTO
        {
            public int Id { get; set; }
            public string RestaurantImg { get; set; }
            public double? StarRating { get; set; }
            public string ResName { get; set; }
            public string ResAddress { get; set; }
            public long? ResPhoneNumber { get; set; }
            public string? OpeningHours { get; set; }
        }

        public class TagByIdDTO
        {
            public int TagID { get; set; }
            public string TagName { get; set; }
            public string TagImg { get; set; }
            public List<TagByIdRestaurantDTO> Restaurants { get; set; }
        }
    }

    public class CreateTagDTO : BaseTag
    {
        public IFormFile timage { get; set; }
    }

    public class UpdateTagDTO : BaseTag
    {
        public IFormFile timage { get; set; }
    }
}
