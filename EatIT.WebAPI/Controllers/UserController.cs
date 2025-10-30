using AutoMapper;
using EatIT.Core.Entities;
using EatIT.Core.Interface;
using EatIT.Infrastructure.Data.DTOs;
using EatIT.WebAPI.Errors;
using EatIT.WebAPI.MyHelper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace EatIT.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public UserController(IUnitOfWork UnitOfWork, IMapper mapper)
        {
            _unitOfWork = UnitOfWork;
            _mapper = mapper;
        }

        //[Authorize(Roles = "Người dùng")]
        [HttpGet("users")]
        public async Task<ActionResult> GetAllUser([FromQuery] int pagenumber, [FromQuery] int pazesize, [FromQuery] string? sort = null, [FromQuery] int? roleid = null, [FromQuery] string? search = null)
        {
            try
            {
                if (pagenumber <= 0 || pazesize <= 0)
                    return BadRequest(new BaseCommentResponse(400, "Số trang và kích thước trang phải lớn hơn 0"));

                var res = await _unitOfWork.UserRepository.GetAllAsync(new Core.Sharing.UserParams 
                { 
                    Sorting = sort, 
                    Roleid = roleid,
                    Pagenumber = pagenumber,
                    Pagesize = pazesize,
                    Search = search
                });

                var totalIteams = await _unitOfWork.UserRepository.CountAsync();

                var result = _mapper.Map<List<UserDTO>>(res);
                return Ok(new Pagination<UserDTO>(pazesize, pagenumber, totalIteams, result));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseCommentResponse(500, "Đã xảy ra lỗi máy chủ nội bộ khi đang tìm kiếm người dùng"));
            }
        }

        [HttpGet("users/{id}")]
        [ResponseType(StatusCodes.Status200OK)]
        [ResponseType(typeof(BaseCommentResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult> GetUserById(int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new BaseCommentResponse(400, "ID người dùng không hợp lệ"));

                var res = await _unitOfWork.UserRepository.GetByIdAsync(id, x => x.Role);
                if(res == null)  
                    return NotFound(new BaseCommentResponse(404, "Không tìm thấy người dùng"));
                    
                var result = _mapper.Map<UserDTO>(res);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseCommentResponse(500, "Đã xảy ra lỗi máy chủ nội bộ khi đang tìm kiếm người dùng"));
            }
        }
        
        [HttpPost("users")]
        public async Task<ActionResult> AddNewUser([FromForm]CreateUserDTO createUserDTO)
        {
            try
            {
                if (!ModelState.IsValid) 
                    return BadRequest(new BaseCommentResponse(400, "Dữ liệu đầu vào không hợp lệ"));

                if (createUserDTO == null)
                    return BadRequest(new BaseCommentResponse(400, "Dữ liệu người dùng là bắt buộc"));

                var ok = await _unitOfWork.UserRepository.AddAsync(createUserDTO);
                if (!ok) 
                    return BadRequest(new BaseCommentResponse(400, "Không thêm được người dùng. Tải ảnh lên không thành công hoặc tạo người dùng không thành công."));

                return Ok(ok);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseCommentResponse(500, "Đã xảy ra lỗi máy chủ nội bộ khi thêm người dùng"));
            }
        }
        
        //Update User (Admin only)
        //[Authorize(Roles = "Admin")]
        [HttpPut("users/{id}")]
        public async Task<ActionResult> UpdateUser(int id, [FromForm] UpdateUserDTO updateUserDTO)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new BaseCommentResponse(400, "ID người dùng không hợp lệ"));

                if (!ModelState.IsValid)
                    return BadRequest(new BaseCommentResponse(400, "Dữ liệu đầu vào không hợp lệ"));

                if (updateUserDTO == null)
                    return BadRequest(new BaseCommentResponse(400, "Cần cập nhật dữ liệu"));

                var res = await _unitOfWork.UserRepository.UpdateAsync(id, updateUserDTO);
                return res ? Ok(updateUserDTO) : NotFound(new BaseCommentResponse(404, "Không tìm thấy người dùng hoặc cập nhật không thành công"));
            }
            catch (Exception e) 
            {
                return StatusCode(500, new BaseCommentResponse(500, "Đã xảy ra lỗi máy chủ nội bộ khi cập nhật người dùng"));
            }
        }

        //Update User Profile (User can update their own profile using JWT token)
        [Authorize]
        [HttpPut("profile")]
        [ResponseType(StatusCodes.Status200OK)]
        [ResponseType(typeof(BaseCommentResponse), StatusCodes.Status400BadRequest)]
        [ResponseType(typeof(BaseCommentResponse), StatusCodes.Status401Unauthorized)]
        [ResponseType(typeof(BaseCommentResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult> UpdateUserProfile([FromForm] UpdateUserProfileDTO updateUserProfileDTO)
        {
            try
            {
                var userIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new BaseCommentResponse(401, "Token không hợp lệ hoặc không chứa thông tin người dùng"));
                }

                if (!ModelState.IsValid)
                    return BadRequest(new BaseCommentResponse(400, "Dữ liệu đầu vào không hợp lệ"));

                if (updateUserProfileDTO == null)
                    return BadRequest(new BaseCommentResponse(400, "Cần cập nhật dữ liệu"));

                var res = await _unitOfWork.UserRepository.UpdateProfileAsync(userId, updateUserProfileDTO);
                return res ? Ok(new { message = "Cập nhật thông tin cá nhân thành công", data = updateUserProfileDTO }) : NotFound(new BaseCommentResponse(404, "Không tìm thấy người dùng hoặc cập nhật không thành công"));
            }
            catch (Exception e) 
            {
                return StatusCode(500, new BaseCommentResponse(500, "Đã xảy ra lỗi máy chủ nội bộ khi cập nhật thông tin cá nhân"));
            }
        }
        
        

        //Delete User
        [HttpDelete("users/{id}")]
        public async Task<ActionResult> DeleteUser(int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new BaseCommentResponse(400, "ID người dùng không hợp lệ"));

                var res = await _unitOfWork.UserRepository.DeleteAsync(id);
                return res ? Ok(new { message = "Người dùng đã bị xóa thành công", id }) : NotFound(new BaseCommentResponse(404, "Không tìm thấy người dùng"));
            }
            catch (Exception e) 
            {
                return StatusCode(500, new BaseCommentResponse(500, "Đã xảy ra lỗi máy chủ nội bộ khi xóa người dùng"));
            }
        }

        //Get User Profile from JWT Token
        [Authorize]
        [HttpGet("profile")]
        [ResponseType(StatusCodes.Status200OK)]
        [ResponseType(typeof(BaseCommentResponse), StatusCodes.Status401Unauthorized)]
        [ResponseType(typeof(BaseCommentResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult> GetUserProfile()
        {
            try
            {
                var userIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new BaseCommentResponse(401, "Token không hợp lệ hoặc không chứa thông tin người dùng"));
                }

                // Lấy thông tin user từ database
                var user = await _unitOfWork.UserRepository.GetProfileAsync(userId);
                if (user == null)
                {
                    return NotFound(new BaseCommentResponse(404, "Không tìm thấy thông tin người dùng"));
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseCommentResponse(500, "Đã xảy ra lỗi máy chủ nội bộ khi lấy thông tin người dùng"));
            }
        }

        //Update User Location using JWT Token
        [Authorize]
        [HttpPut("location")]
        [ResponseType(StatusCodes.Status200OK)]
        [ResponseType(typeof(BaseCommentResponse), StatusCodes.Status400BadRequest)]
        [ResponseType(typeof(BaseCommentResponse), StatusCodes.Status401Unauthorized)]
        [ResponseType(typeof(BaseCommentResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult> UpdateUserLocation([FromBody] UpdateUserLocationDTO locationDto)
        {
            try
            {
                // Lấy user ID từ JWT token
                var userId = Locations.GetCurrentUserId(User);
                if (userId <= 0)
                {
                    return Unauthorized(new BaseCommentResponse(401, "Token không hợp lệ hoặc không chứa thông tin người dùng"));
                }

                if (!ModelState.IsValid)
                    return BadRequest(new BaseCommentResponse(400, "Dữ liệu đầu vào không hợp lệ"));

                if (locationDto == null)
                    return BadRequest(new BaseCommentResponse(400, "Dữ liệu vị trí là bắt buộc"));

                var res = await _unitOfWork.UserRepository.UpdateUserLocationAsync(userId, locationDto);
                return res ? Ok(new { message = "Cập nhật vị trí thành công", location = locationDto }) : NotFound(new BaseCommentResponse(404, "Không tìm thấy người dùng hoặc cập nhật không thành công"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseCommentResponse(500, "Đã xảy ra lỗi máy chủ nội bộ khi cập nhật vị trí người dùng"));
            }
        }

        //Get User Location using JWT Token
        [Authorize]
        [HttpGet("location")]
        [ResponseType(StatusCodes.Status200OK)]
        [ResponseType(typeof(BaseCommentResponse), StatusCodes.Status401Unauthorized)]
        [ResponseType(typeof(BaseCommentResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult> GetUserLocation()
        {
            try
            {
                // Lấy user ID từ JWT token
                var userId = Locations.GetCurrentUserId(User);
                if (userId <= 0)
                {
                    return Unauthorized(new BaseCommentResponse(401, "Token không hợp lệ hoặc không chứa thông tin người dùng"));
                }

                var location = await _unitOfWork.UserRepository.GetUserLocationAsync(userId);
                if (location == null)
                {
                    return NotFound(new BaseCommentResponse(404, "Không tìm thấy vị trí người dùng"));
                }

                return Ok(location);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseCommentResponse(500, "Đã xảy ra lỗi máy chủ nội bộ khi lấy vị trí người dùng"));
            }
        }
    }
}
