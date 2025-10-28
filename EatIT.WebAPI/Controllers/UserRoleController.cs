using AutoMapper;
using EatIT.Core.Entities;
using EatIT.Core.Interface;
using EatIT.Infrastructure.Data.DTOs;
using EatIT.WebAPI.Errors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace EatIT.WebAPI.Controllers
{
    //[Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UserRoleController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public UserRoleController(IUnitOfWork UnitOfWork, IMapper mapper)
        {
            _unitOfWork = UnitOfWork;
            _mapper = mapper;
        }
        
        //[Authorize(Roles = "Admin")]
        [HttpGet("roles")]
        public async Task<ActionResult> Get() 
        {
            try
            {
                var all_user_role = await _unitOfWork.UserRoleRepository.GetAllAsync();

                if (all_user_role != null)
                {
                    var res = _mapper.Map<IReadOnlyList<UserRole>, IReadOnlyList<ListUserRoleDTO>>(all_user_role);
                    return Ok(res);
                }
                    
                return NotFound(new BaseCommentResponse(404, "Không tìm thấy vai trò người dùng"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseCommentResponse(500, "Đã xảy ra lỗi máy chủ nội bộ khi tìm nạp vai trò người dùng"));
            }
        }

        [HttpGet("roles/{id}")]
        public async Task<ActionResult> GetById(int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new BaseCommentResponse(400, "ID vai trò không hợp lệ"));

                var role = await _unitOfWork.UserRoleRepository.GetAsync(id);
                if (role != null) 
                    return Ok(_mapper.Map<UserRole, ListUserRoleDTO>(role));
                    
                return NotFound(new BaseCommentResponse(404, "Không tìm thấy vai trò người dùng"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseCommentResponse(500, "Đã xảy ra lỗi máy chủ nội bộ khi tìm nạp vai trò người dùng"));
            }
        }

        [HttpPost("roles")]
        public async Task<ActionResult> AddUserRole(UserRoleDTO userRoleDTO)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new BaseCommentResponse(400, "Dữ liệu đầu vào không hợp lệ"));

                if (userRoleDTO == null)
                    return BadRequest(new BaseCommentResponse(400, "Dữ liệu vai trò người dùng là bắt buộc"));

                var res = _mapper.Map<UserRole>(userRoleDTO);
                await _unitOfWork.UserRoleRepository.AddAsync(res);
                return Ok(res);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseCommentResponse(500, "Đã xảy ra lỗi máy chủ nội bộ khi thêm vai trò người dùng"));
            }
        }

        [HttpPut("roles/{id}")]
        public async Task<ActionResult> UpdateUserRole(int id, UserRoleDTO userRoleDTO)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new BaseCommentResponse(400, "ID vai trò không hợp lệ"));

                if (!ModelState.IsValid)
                    return BadRequest(new BaseCommentResponse(400, "Dữ liệu đầu vào không hợp lệ"));

                if (userRoleDTO == null)
                    return BadRequest(new BaseCommentResponse(400, "Cần cập nhật dữ liệu"));

                var existing_role = await _unitOfWork.UserRoleRepository.GetAsync(id);
                if (existing_role != null)
                {
                    _mapper.Map(userRoleDTO, existing_role);
                    await _unitOfWork.UserRoleRepository.UpdateAsync(id, existing_role);
                    return Ok(existing_role);
                }
                return NotFound(new BaseCommentResponse(404, "Không tìm thấy vai trò người dùng"));
            }
            catch (Exception ex) 
            { 
                return StatusCode(500, new BaseCommentResponse(500, "Đã xảy ra lỗi máy chủ nội bộ khi cập nhật vai trò người dùng"));
            }
        }

        [HttpDelete("roles/{id}")]
        public async Task<ActionResult> DeleteUserRole(int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new BaseCommentResponse(400, "ID vai trò không hợp lệ"));

                var existing_role = await _unitOfWork.UserRoleRepository.GetAsync(id);
                if(existing_role != null)
                {
                    await _unitOfWork.UserRoleRepository.DeleteAsync(id);
                    return Ok(new { message = $"Vai trò người dùng '{existing_role.RoleName}' đã xóa thành công", id });
                }
                return NotFound(new BaseCommentResponse(404, "Không tìm thấy vai trò người dùng"));
            }
            catch (Exception ex) 
            {
                return StatusCode(500, new BaseCommentResponse(500, "Đã xảy ra lỗi máy chủ nội bộ khi xóa vai trò người dùng"));
            }
        }
    }
}
