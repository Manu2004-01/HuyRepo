using AutoMapper;
using EatIT.Core.Entities;
using EatIT.Core.Interface;
using EatIT.Infrastructure.Data.DTOs;
using EatIT.WebAPI.Errors;
using EatIT.WebAPI.MyHelper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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
        [HttpGet("get-all-users")]
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
        
        [Authorize(Roles = "Admin")]
        [HttpGet("get-user-by-id/{id}")]
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
        
        [HttpPost("add-new-user")]
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
        [HttpPut("update-existing-user/{id}")]
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

        //Update User Profile (User can update their own profile)
        //[Authorize]
        [HttpPut("update-profile/{id}")]
        public async Task<ActionResult> UpdateUserProfile(int id, [FromForm] UpdateUserProfileDTO updateUserProfileDTO)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new BaseCommentResponse(400, "ID người dùng không hợp lệ"));

                if (!ModelState.IsValid)
                    return BadRequest(new BaseCommentResponse(400, "Dữ liệu đầu vào không hợp lệ"));

                if (updateUserProfileDTO == null)
                    return BadRequest(new BaseCommentResponse(400, "Cần cập nhật dữ liệu"));

                var res = await _unitOfWork.UserRepository.UpdateProfileAsync(id, updateUserProfileDTO);
                return res ? Ok(new { message = "Cập nhật thông tin cá nhân thành công", data = updateUserProfileDTO }) : NotFound(new BaseCommentResponse(404, "Không tìm thấy người dùng hoặc cập nhật không thành công"));
            }
            catch (Exception e) 
            {
                return StatusCode(500, new BaseCommentResponse(500, "Đã xảy ra lỗi máy chủ nội bộ khi cập nhật thông tin cá nhân"));
            }
        }
        
        //Delete User
        [HttpDelete("delete-existing-user/{id}")]
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
    }
}
