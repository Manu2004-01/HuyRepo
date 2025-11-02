using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EatIT.Core.DTOs
{
    public class ChangePasswordDTO
    {
        [Required(ErrorMessage = "Mật khẩu cũ là bắt buộc")]
        public string OldPassword { get; set; } = "";

        [Required(ErrorMessage = "Mật khẩu mới là bắt buộc")]
        [RegularExpression(@"^(?=.*\d)(?=.*[@$!%*?&])[A-Z][A-Za-z\d@$!%*?&]{5,}$",
        ErrorMessage = "Mật khẩu phải bắt đầu bằng chữ in hoa, chứa ít nhất 1 số và 1 ký tự đặc biệt")]
        public string NewPassword { get; set; } = "";

        [Required(ErrorMessage = "Xác nhận mật khẩu là bắt buộc")]
        [Compare("NewPassword", ErrorMessage = "Mật khẩu xác nhận không khớp")]
        public string ConfirmPassword { get; set; } = "";
    }
}

