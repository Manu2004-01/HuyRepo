using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EatIT.Infrastructure.Data.DTOs
{
    public class BaseUser
    {
        [Required(ErrorMessage = "Tên người dùng là bắt buộc")]
        [MaxLength(100, ErrorMessage = "Tên người dùng không được vượt quá 100 ký tự")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        [RegularExpression(@"^(?=.*\d)(?=.*[@$!%*?&])[A-Z][A-Za-z\d@$!%*?&]{5,}$",
        ErrorMessage = "Mật khẩu phải bắt đầu bằng chữ in hoa, chứa ít nhất 1 số và 1 ký tự đặc biệt")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
        [RegularExpression(@"^\d{11}$", ErrorMessage = "Số điện thoại phải đúng 11 số")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Địa chỉ là bắt buộc")]
        public string UserAddress { get; set; }
        public string? Preference { get; set; } //Mon an ua thich
        public string? Dislike { get; set; } //Mon k thich or k an dc
        public string? Allergy { get; set; } //Mon di ung
        public string? Diet { get; set; } //Che do an
    }

    public class UserDTO : BaseUser
    {
        public int Id { get; set; }
        public string RoleName { get; set; }
        public string UserImage { get; set; }
        public DateTime CreateAt { get; set; }
        public DateTime UpdateAt { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreateUserDTO : BaseUser
    {
        public int userroleid { get; set; }
        public IFormFile? image {  get; set; }
    }

    //Update User (Admin only)
    public class UpdateUserDTO : BaseUser
    {
        public int userroleid { get; set; }
        public IFormFile? image { get; set; }
    }

    //Update User Profile (User can update their own profile)
    public class UpdateUserProfileDTO
    {
        [MaxLength(100, ErrorMessage = "Tên người dùng không được vượt quá 100 ký tự")]
        public string? UserName { get; set; }

        public string? UserAddress { get; set; }

        public IFormFile? image { get; set; }
    }

    public class UpdateUserLocationDTO
    {
        [Required(ErrorMessage = "Vĩ độ user là bắt buộc")]
        [Range(-90, 90, ErrorMessage = "Vĩ độ phải nằm trong khoảng -90 đến 90")]
        public double UserLatitude { get; set; }

        [Required(ErrorMessage = "Kinh độ user là bắt buộc")]
        [Range(-180, 180, ErrorMessage = "Kinh độ phải nằm trong khoảng -180 đến 180")]
        public double UserLongitude { get; set; }
    }

    public class UserLocationResponseDTO
    {
        public double UserLatitude { get; set; }
        public double UserLongitude { get; set; }
        public DateTime LastLocationUpdate { get; set; }
    }

    public class UpdateUserLocationWithIdDTO
    {
        [Required(ErrorMessage = "User ID là bắt buộc")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Vĩ độ user là bắt buộc")]
        [Range(-90, 90, ErrorMessage = "Vĩ độ phải nằm trong khoảng -90 đến 90")]
        public double UserLatitude { get; set; }

        [Required(ErrorMessage = "Kinh độ user là bắt buộc")]
        [Range(-180, 180, ErrorMessage = "Kinh độ phải nằm trong khoảng -180 đến 180")]
        public double UserLongitude { get; set; }
    }

    // User Profile Response DTO
    public class UserProfileDTO 
    {
        public string UserImage { get; set; }
        public string UserName { get; set; }
        public string PhoneNumber { get; set; }
        public string UserAddress { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string? Preference { get; set; } //Mon an ua thich
        public string? Dislike { get; set; } //Mon k thich or k an dc
        public string? Allergy { get; set; } //Mon di ung
        public string? Diet { get; set; } //Che do an
    }

    public class RegisterDTO 
    {
        [Required(ErrorMessage = "Tên người dùng là bắt buộc")]
        [MaxLength(100, ErrorMessage = "Tên người dùng không được vượt quá 100 ký tự")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        [RegularExpression(@"^(?=.*\d)(?=.*[@$!%*?&])[A-Z][A-Za-z\d@$!%*?&]{5,}$",
        ErrorMessage = "Mật khẩu phải bắt đầu bằng chữ in hoa, chứa ít nhất 1 số và 1 ký tự đặc biệt")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Xác nhận mật khẩu là bắt buộc")]
        public string ConfirmPassword { get; set; }
    }
}
