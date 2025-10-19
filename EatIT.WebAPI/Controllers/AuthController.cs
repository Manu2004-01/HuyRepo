using EatIT.Core.DTOs;
using EatIT.Core.Interface;
using EatIT.Infrastructure.Data;
using EatIT.WebAPI.Errors;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Http;
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

        public AuthController(ApplicationDBContext db, ITokenService tokenService, IHostEnvironment hostEnvironment, IEmailSender emailSender)
        {
            _db = db;
            _tokenService = tokenService;
            _hostEnvironment = hostEnvironment;
            _emailSender = emailSender;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO dto)
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
                    user = new { user.Id, user.UserName, user.Email, user.RoleId, RoleName = user.Role?.RoleName }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseCommentResponse(500, "Đã xảy ra lỗi máy chủ nội bộ trong quá trình đăng nhập"));
            }
        }

        [HttpGet("google-login")]
        public IActionResult GoogleLogin()
        {
            var properties = new AuthenticationProperties
            {
                RedirectUri = Url.Action("GoogleResponse")
            };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        [HttpGet("google-response")]
        public async Task<IActionResult> GoogleResponse()
        {
            try
            {
                var result = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);
                
                if (!result.Succeeded)
                    return BadRequest(new BaseCommentResponse(400, "Đăng nhập Google thất bại"));

                var claims = result.Principal.Claims.ToList();
                var googleId = claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
                var email = claims.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value;
                var name = claims.FirstOrDefault(x => x.Type == ClaimTypes.Name)?.Value;
                var picture = claims.FirstOrDefault(x => x.Type == "picture")?.Value;

                if (string.IsNullOrEmpty(googleId) || string.IsNullOrEmpty(email))
                    return BadRequest(new BaseCommentResponse(400, "Không thể lấy thông tin từ Google"));

                // Kiểm tra xem user đã tồn tại chưa
                var existingUser = await _db.Users.Include(u => u.Role)
                    .FirstOrDefaultAsync(u => u.GoogleId == googleId || u.Email == email);

                if (existingUser == null)
                {
                    // Tạo user mới với role mặc định là User (RoleId = 2)
                    existingUser = new EatIT.Core.Entities.Users
                    {
                        GoogleId = googleId,
                        Email = email,
                        UserName = name ?? email.Split('@')[0],
                        UserImg = picture,
                        RoleId = 2, // User role
                        Password = "", // Không cần password cho Google login
                        PhoneNumber = "",
                        UserAddress = "",
                        CreateAt = DateTime.Now,
                        UpdateAt = DateTime.Now,
                        IsActive = true
                    };

                    _db.Users.Add(existingUser);
                    await _db.SaveChangesAsync();
                }
                else
                {
                    // Cập nhật GoogleId nếu chưa có
                    if (string.IsNullOrEmpty(existingUser.GoogleId))
                    {
                        existingUser.GoogleId = googleId;
                        existingUser.UpdateAt = DateTime.Now;
                        await _db.SaveChangesAsync();
                    }
                }

                var token = _tokenService.CreateToken(existingUser, existingUser.Role?.RoleName ?? string.Empty);

                return Ok(new
                {
                    token,
                    user = new { 
                        existingUser.Id, 
                        existingUser.UserName, 
                        existingUser.Email, 
                        existingUser.RoleId, 
                        RoleName = existingUser.Role?.RoleName,
                        existingUser.UserImg
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseCommentResponse(500, $"Đã xảy ra lỗi máy chủ nội bộ trong quá trình đăng nhập Google: {ex.Message}"));
            }
        }

        [HttpPost("google-login-token")]
        public async Task<IActionResult> GoogleLoginWithToken([FromBody] GoogleLoginDTO dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new BaseCommentResponse(400, "Dữ liệu đầu vào không hợp lệ"));

                if (dto == null || string.IsNullOrEmpty(dto.GoogleId) || string.IsNullOrEmpty(dto.Email))
                    return BadRequest(new BaseCommentResponse(400, "Google ID và Email là bắt buộc"));

                // Kiểm tra xem user đã tồn tại chưa
                var existingUser = await _db.Users.Include(u => u.Role)
                    .FirstOrDefaultAsync(u => u.GoogleId == dto.GoogleId || u.Email == dto.Email);

                bool isNewUser = false;

                if (existingUser == null)
                {
                    // Tạo user mới với role mặc định là User (RoleId = 2)
                    existingUser = new EatIT.Core.Entities.Users
                    {
                        GoogleId = dto.GoogleId,
                        Email = dto.Email,
                        UserName = dto.Name ?? dto.Email.Split('@')[0],
                        UserImg = dto.Picture,
                        RoleId = 2, // User role
                        Password = "", // Không cần password cho Google login
                        PhoneNumber = "",
                        UserAddress = "",
                        CreateAt = DateTime.Now,
                        UpdateAt = DateTime.Now,
                        IsActive = true
                    };

                    _db.Users.Add(existingUser);
                    await _db.SaveChangesAsync();
                    isNewUser = true;
                }
                else
                {
                    // Cập nhật GoogleId nếu chưa có
                    if (string.IsNullOrEmpty(existingUser.GoogleId))
                    {
                        existingUser.GoogleId = dto.GoogleId;
                        existingUser.UpdateAt = DateTime.Now;
                        await _db.SaveChangesAsync();
                    }
                }

                var token = _tokenService.CreateToken(existingUser, existingUser.Role?.RoleName ?? string.Empty);

                return Ok(new
                {
                    token,
                    user = new { 
                        existingUser.Id, 
                        existingUser.UserName, 
                        existingUser.Email, 
                        existingUser.RoleId, 
                        RoleName = existingUser.Role?.RoleName,
                        existingUser.UserImg
                    },
                    isNewUser = isNewUser,
                    message = isNewUser ? "Đăng ký tài khoản thành công!" : "Đăng nhập thành công!"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseCommentResponse(500, $"Đã xảy ra lỗi máy chủ nội bộ trong quá trình đăng nhập Google: {ex.Message}"));
            }
        }

        [HttpPost("google-register")]
        public async Task<IActionResult> GoogleRegister([FromBody] GoogleLoginDTO dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new BaseCommentResponse(400, "Dữ liệu đầu vào không hợp lệ"));

                if (dto == null || string.IsNullOrEmpty(dto.GoogleId) || string.IsNullOrEmpty(dto.Email))
                    return BadRequest(new BaseCommentResponse(400, "Google ID và Email là bắt buộc"));

                // Kiểm tra xem email đã được sử dụng chưa
                var existingUser = await _db.Users
                    .FirstOrDefaultAsync(u => u.Email == dto.Email);

                if (existingUser != null)
                {
                    return BadRequest(new BaseCommentResponse(400, "Email này đã được sử dụng. Vui lòng đăng nhập thay vì đăng ký."));
                }

                // Tạo user mới
                var newUser = new EatIT.Core.Entities.Users
                {
                    GoogleId = dto.GoogleId,
                    Email = dto.Email,
                    UserName = dto.Name ?? dto.Email.Split('@')[0],
                    UserImg = dto.Picture,
                    RoleId = 2, // User role
                    Password = "", // Không cần password cho Google login
                    PhoneNumber = "",
                    UserAddress = "",
                    CreateAt = DateTime.Now,
                    UpdateAt = DateTime.Now,
                    IsActive = true
                };

                _db.Users.Add(newUser);
                await _db.SaveChangesAsync();

                // Lấy user với role information
                var userWithRole = await _db.Users.Include(u => u.Role)
                    .FirstOrDefaultAsync(u => u.Id == newUser.Id);

                var token = _tokenService.CreateToken(userWithRole, userWithRole.Role?.RoleName ?? string.Empty);

                return Ok(new
                {
                    token,
                    user = new { 
                        userWithRole.Id, 
                        userWithRole.UserName, 
                        userWithRole.Email, 
                        userWithRole.RoleId, 
                        RoleName = userWithRole.Role?.RoleName,
                        userWithRole.UserImg
                    },
                    message = "Đăng ký tài khoản thành công!"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseCommentResponse(500, $"Đã xảy ra lỗi máy chủ nội bộ trong quá trình đăng ký Google: {ex.Message}"));
            }
        }

        //Forget Password
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDTO dto)
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
                var resetToken = GenerateResetToken();
                user.ResetPasswordToken = resetToken;
                user.ResetPasswordTokenExpiry = DateTime.UtcNow.AddHours(1); // Token hết hạn sau 1 giờ
                user.UpdateAt = DateTime.Now;

                await _db.SaveChangesAsync();

                var apiUrl = HttpContext.RequestServices
    .GetRequiredService<IConfiguration>()["API_url"] ?? "https://localhost:7091/";
                var resetLink = $"{apiUrl}reset-password?token={Uri.EscapeDataString(resetToken)}&email={Uri.EscapeDataString(user.Email ?? string.Empty)}";

            //    if (!string.IsNullOrEmpty(user.Email) && user.Email.EndsWith("@gmail.com", StringComparison.OrdinalIgnoreCase))
            //    {
            //        try
            //        {
            //            var subject = "EatIT - Reset mật khẩu";
            //            var body = $@"
            //<p>Xin chào,</p>
            //<p>Bạn vừa yêu cầu đặt lại mật khẩu. Vui lòng bấm vào liên kết bên dưới hoặc dùng token để đặt lại mật khẩu.</p>
            //<p><a href=""{resetLink}"">{resetLink}</a></p>
            //<p><strong>Token:</strong> {resetToken}</p>
            //<p>Token sẽ hết hạn lúc: {user.ResetPasswordTokenExpiry:yyyy-MM-dd HH:mm:ss} UTC</p>
            //<p>Nếu bạn không yêu cầu, vui lòng bỏ qua email này.</p>";

            //            await _emailSender.SendAsync(user.Email, subject, body);
            //        }
            //        catch (Exception emailEx)
            //        {
            //            // Log lỗi email nhưng vẫn trả về response thành công
            //            Console.WriteLine($"Failed to send email: {emailEx.Message}");
            //        }
            //    }

                //token test
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
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDTO dto)
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
        public async Task<IActionResult> VerifyResetToken([FromBody] VerifyResetTokenDTO dto)
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

        // Helper method để tạo reset token
        private string GenerateResetToken()
        {
            using (var rng = RandomNumberGenerator.Create())
            {
                var bytes = new byte[32];
                rng.GetBytes(bytes);
                return Convert.ToBase64String(bytes);
            }
        }
    }
}
