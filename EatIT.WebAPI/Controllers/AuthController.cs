using EatIT.Core.DTOs;
using EatIT.Core.Interface;
using EatIT.Infrastructure.Data;
using EatIT.Infrastructure.Data.DTOs;
using EatIT.WebAPI.Errors;
using EatIT.WebAPI.MyHelper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography;

namespace EatIT.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDBContext _db;
        private readonly ITokenService _tokenService;
        private readonly IHostEnvironment _hostEnvironment;
        private readonly IEmailSender _emailSender;
        private readonly IUnitOfWork _unitOfWork;

        public AuthController(ApplicationDBContext db, ITokenService tokenService, IHostEnvironment hostEnvironment, IEmailSender emailSender, IUnitOfWork unitOfWork)
        {
            _db = db;
            _tokenService = tokenService;
            _hostEnvironment = hostEnvironment;
            _emailSender = emailSender;
            _unitOfWork = unitOfWork;
        }

        [HttpPost("register")]
        [ResponseType(StatusCodes.Status200OK)]
        [ResponseType(typeof(BaseCommentResponse), StatusCodes.Status400BadRequest)]
        [ResponseType(typeof(BaseCommentResponse), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Register([FromQuery] RegisterDTO dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new BaseCommentResponse(400, "Dữ liệu đầu vào không hợp lệ"));

                if (dto == null)
                    return BadRequest(new BaseCommentResponse(400, "Dữ liệu đăng ký là bắt buộc"));

                if(dto.Password != dto.ConfirmPassword)
                {
                    return BadRequest("Mật khẩu xác nhận không khớp. Vui lòng nhập lại mật khẩu xác minh");
                }

                // Kiểm tra email đã tồn tại chưa
                var existingUser = await _db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);

                if (existingUser != null)
                {
                    return Conflict(new BaseCommentResponse(409, "Email này đã được sử dụng"));
                }

                // Kiểm tra username đã tồn tại chưa
                var existingUsername = await _db.Users.FirstOrDefaultAsync(u => u.UserName == dto.UserName);
                if (existingUsername != null)
                {
                    return Conflict(new BaseCommentResponse(409, "Tên người dùng này đã được sử dụng"));
                }

                var registerUser = new CreateUserDTO
                {
                    UserName = dto.UserName,
                    Email = dto.Email,
                    Password = dto.Password,
                    PhoneNumber = "",
                    UserAddress = "",
                    userroleid = 2,
                    image = null
                };

                var res = await _unitOfWork.UserRepository.AddAsync(registerUser);

                if (!res)
                    return BadRequest(new BaseCommentResponse(400, "Không thêm được người dùng. Tải ảnh lên không thành công hoặc tạo người dùng không thành công."));
                return Ok(res);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseCommentResponse(500, "Đã xảy ra lỗi máy chủ nội bộ khi thêm người dùng"));
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromQuery] LoginDTO dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new BaseCommentResponse(400, "Dữ liệu đầu vào không hợp lệ"));

                if (dto == null || string.IsNullOrEmpty(dto.Email) || string.IsNullOrEmpty(dto.Password))
                    return BadRequest(new BaseCommentResponse(400, "Email và mật khẩu là bắt buộc"));

                var user = await _db.Users.Include(u => u.Role)
                    .FirstOrDefaultAsync(u => u.Email == dto.Email && u.Password == dto.Password);

                if (user == null)
                    return Unauthorized(new BaseCommentResponse(401, "Thông tin đăng nhập không hợp lệ"));

                var token = _tokenService.CreateToken(user, user.Role?.RoleName ?? string.Empty);

                return Ok(new
                {
                    token,
                    user = new { user.UserId, user.UserName, user.Email, user.RoleId, RoleName = user.Role?.RoleName }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseCommentResponse(500, "Đã xảy ra lỗi máy chủ nội bộ trong quá trình đăng nhập"));
            }
        }

        //Forget Password
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromQuery] ForgotPasswordDTO dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new BaseCommentResponse(400, "Dữ liệu đầu vào không hợp lệ"));

                if (dto == null || string.IsNullOrEmpty(dto.Email))
                    return BadRequest(new BaseCommentResponse(400, "Email là bắt buộc"));

                var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);

                if (user == null)
                {
                    // Trả về thành công để không tiết lộ email có tồn tại hay không
                    return Ok(new BaseCommentResponse(200, "Nếu email tồn tại, bạn sẽ nhận được hướng dẫn reset mật khẩu"));
                }

                // Tạo reset token
                var resetToken = new ResetToken().GenerateResetToken();
                user.ResetPasswordToken = resetToken;
                user.ResetPasswordTokenExpiry = DateTime.UtcNow.AddHours(1); // Token hết hạn sau 1 giờ
                user.UpdateAt = DateTime.Now;

                await _db.SaveChangesAsync();

                var apiUrl = HttpContext.RequestServices
    .GetRequiredService<IConfiguration>()["API_url"] ?? "https://localhost:7091/";
                var resetLink = $"{apiUrl}reset-password?token={Uri.EscapeDataString(resetToken)}&email={Uri.EscapeDataString(user.Email ?? string.Empty)}";

                if (_hostEnvironment.IsDevelopment())
                {
                    return Ok(new
                    {
                        message = "Nếu email tồn tại, bạn sẽ nhận được hướng dẫn reset mật khẩu",
                        developmentInfo = new
                        {
                            email = user.Email,
                            resetToken = resetToken,
                            expiry = user.ResetPasswordTokenExpiry,
                            note = "Thông tin này chỉ hiển thị trong môi trường development"
                        }
                    });
                }

                return Ok(new BaseCommentResponse(200, "Nếu email tồn tại, bạn sẽ nhận được hướng dẫn reset mật khẩu"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseCommentResponse(500, "Đã xảy ra lỗi máy chủ nội bộ trong quá trình xử lý yêu cầu reset mật khẩu"));
            }
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromQuery] ResetPasswordDTO dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new BaseCommentResponse(400, "Dữ liệu đầu vào không hợp lệ"));

                if (dto == null || string.IsNullOrEmpty(dto.Token) || string.IsNullOrEmpty(dto.Email) ||
                    string.IsNullOrEmpty(dto.NewPassword) || string.IsNullOrEmpty(dto.ConfirmPassword))
                    return BadRequest(new BaseCommentResponse(400, "Tất cả các trường là bắt buộc"));

                if (dto.NewPassword != dto.ConfirmPassword)
                    return BadRequest(new BaseCommentResponse(400, "Mật khẩu xác nhận không khớp"));

                var user = await _db.Users.FirstOrDefaultAsync(u =>
                    u.Email == dto.Email &&
                    u.ResetPasswordToken == dto.Token &&
                    u.ResetPasswordTokenExpiry > DateTime.UtcNow);

                if (user == null)
                    return BadRequest(new BaseCommentResponse(400, "Token không hợp lệ hoặc đã hết hạn"));

                // Cập nhật mật khẩu mới
                user.Password = dto.NewPassword;
                user.ResetPasswordToken = null;
                user.ResetPasswordTokenExpiry = null;
                user.UpdateAt = DateTime.Now;

                await _db.SaveChangesAsync();

                return Ok(new BaseCommentResponse(200, "Đặt lại mật khẩu thành công"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseCommentResponse(500, "Đã xảy ra lỗi máy chủ nội bộ trong quá trình đặt lại mật khẩu"));
            }
        }

        [HttpPost("verify-reset-token")]
        public async Task<IActionResult> VerifyResetToken([FromQuery] VerifyResetTokenDTO dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new BaseCommentResponse(400, "Dữ liệu đầu vào không hợp lệ"));

                if (dto == null || string.IsNullOrEmpty(dto.Token) || string.IsNullOrEmpty(dto.Email))
                    return BadRequest(new BaseCommentResponse(400, "Token và Email là bắt buộc"));

                var user = await _db.Users.FirstOrDefaultAsync(u =>
                    u.Email == dto.Email &&
                    u.ResetPasswordToken == dto.Token &&
                    u.ResetPasswordTokenExpiry > DateTime.UtcNow);

                if (user == null)
                    return BadRequest(new BaseCommentResponse(400, "Token không hợp lệ hoặc đã hết hạn"));

                return Ok(new BaseCommentResponse(200, "Token hợp lệ"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseCommentResponse(500, "Đã xảy ra lỗi máy chủ nội bộ trong quá trình xác minh token"));
            }
        }
    }
}
