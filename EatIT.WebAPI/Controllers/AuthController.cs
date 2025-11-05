using EatIT.Core.DTOs;
using EatIT.Core.Interface;
using EatIT.Infrastructure.Data;
using EatIT.Infrastructure.Data.DTOs;
using EatIT.WebAPI.Errors;
using EatIT.WebAPI.MyHelper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
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
        private readonly IConfiguration _configuration;

        public AuthController(ApplicationDBContext db, ITokenService tokenService, IHostEnvironment hostEnvironment, IEmailSender emailSender, IUnitOfWork unitOfWork, IConfiguration configuration)
        {
            _db = db;
            _tokenService = tokenService;
            _hostEnvironment = hostEnvironment;
            _emailSender = emailSender;
            _unitOfWork = unitOfWork;
            _configuration = configuration;
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

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            try
            {
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value 
                    ?? User.FindFirst("sub")?.Value;

                if (string.IsNullOrEmpty(userIdClaim))
                {
                    return Unauthorized(new BaseCommentResponse(401, "Token không hợp lệ"));
                }

                return Ok(new BaseCommentResponse(200, "Đăng xuất thành công"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseCommentResponse(500, "Đã xảy ra lỗi máy chủ nội bộ trong quá trình đăng xuất"));
            }
        }

        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromQuery] ChangePasswordDTO dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new BaseCommentResponse(400, "Dữ liệu đầu vào không hợp lệ"));

                if (dto == null || string.IsNullOrEmpty(dto.OldPassword) || 
                    string.IsNullOrEmpty(dto.NewPassword) || string.IsNullOrEmpty(dto.ConfirmPassword))
                    return BadRequest(new BaseCommentResponse(400, "Tất cả các trường là bắt buộc"));

                if (dto.NewPassword != dto.ConfirmPassword)
                    return BadRequest(new BaseCommentResponse(400, "Mật khẩu xác nhận không khớp"));

                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value 
                    ?? User.FindFirst("sub")?.Value;

                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                    return Unauthorized(new BaseCommentResponse(401, "Token không hợp lệ"));

                var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == userId);

                if (user == null)
                    return NotFound(new BaseCommentResponse(404, "Không tìm thấy người dùng"));

                if (user.Password != dto.OldPassword)
                    return Unauthorized(new BaseCommentResponse(401, "Mật khẩu cũ không đúng"));

                if (user.Password == dto.NewPassword)
                    return BadRequest(new BaseCommentResponse(400, "Mật khẩu mới phải khác mật khẩu cũ"));

                user.Password = dto.NewPassword;
                user.UpdateAt = DateTime.UtcNow;

                await _db.SaveChangesAsync();

                return Ok(new BaseCommentResponse(200, "Đổi mật khẩu thành công"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseCommentResponse(500, "Đã xảy ra lỗi máy chủ nội bộ trong quá trình đổi mật khẩu"));
            }
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromQuery] ForgotPasswordDTO dto, [FromQuery] bool includeToken = false)
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

                var apiUrl = _configuration["API_url"] ?? "https://localhost:7091/";
                var resetLink = $"{apiUrl}reset-password?token={Uri.EscapeDataString(resetToken)}&email={Uri.EscapeDataString(user.Email ?? string.Empty)}";

                // Gửi email reset password
                try
                {
                    var emailSubject = "Đặt lại mật khẩu - EatIT";
                    var emailBody = $@"
                        <html>
                        <body style='font-family: Arial, sans-serif;'>
                            <h2>Yêu cầu đặt lại mật khẩu</h2>
                            <p>Xin chào,</p>
                            <p>Bạn đã yêu cầu đặt lại mật khẩu cho tài khoản EatIT của mình.</p>
                            <p>Vui lòng click vào link sau để đặt lại mật khẩu (link có hiệu lực trong 1 giờ):</p>
                            <p><a href='{resetLink}' style='background-color: #4CAF50; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px; display: inline-block;'>Đặt lại mật khẩu</a></p>
                            <p>Hoặc copy link sau vào trình duyệt:</p>
                            <p style='word-break: break-all;'>{resetLink}</p>
                            <p>Nếu bạn không yêu cầu đặt lại mật khẩu, vui lòng bỏ qua email này.</p>
                            <p>Trân trọng,<br/>Đội ngũ EatIT</p>
                        </body>
                        </html>";

                    await _emailSender.SendAsync(user.Email ?? string.Empty, emailSubject, emailBody);
                }
                catch (Exception emailEx)
                {
                    // Log lỗi nhưng vẫn trả về thành công để không tiết lộ thông tin
                    // Trong production, có thể log vào file hoặc logging service
                    // Console.WriteLine($"Lỗi gửi email: {emailEx.Message}");
                }

                // Hiển thị token nếu là development hoặc nếu includeToken=true (cho testing)
                if (_hostEnvironment.IsDevelopment() || includeToken)
                {
                    return Ok(new
                    {
                        message = "Nếu email tồn tại, bạn sẽ nhận được hướng dẫn reset mật khẩu",
                        developmentInfo = new
                        {
                            email = user.Email,
                            resetToken = resetToken,
                            resetLink = resetLink,
                            expiry = user.ResetPasswordTokenExpiry,
                            note = _hostEnvironment.IsDevelopment() 
                                ? "Thông tin này chỉ hiển thị trong môi trường development" 
                                : "Thông tin này được hiển thị vì includeToken=true (chỉ dùng cho testing)"
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
