using AutoMapper;
using EatIT.Core.DTOs;
using EatIT.Core.Interface;
using EatIT.WebAPI.Errors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace EatIT.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FavoriteController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public FavoriteController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [Authorize]
        [HttpGet("favorites")]
        public async Task<ActionResult> GetAllFavorites()
        {
            try
            {
                // Lấy userId từ JWT token
                var userIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                    return Unauthorized(new BaseCommentResponse(401, "Token không hợp lệ hoặc không chứa thông tin người dùng"));

                // Chỉ lấy favorites của user đang đăng nhập
                var favorites = await _unitOfWork.FavoriteRepository.GetFavoritesByUserIdAsync(userId);
                var result = _mapper.Map<List<FavoriteDTO>>(favorites);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseCommentResponse(500, "Đã xảy ra lỗi máy chủ nội bộ khi lấy danh sách yêu thích"));
            }
        }

        [Authorize]
        [HttpGet("favorites/{userId}")]
        public async Task<ActionResult> GetFavoritesByUser(int userId)
        {
            try
            {
                if (userId <= 0)
                    return BadRequest(new BaseCommentResponse(400, "ID người dùng không hợp lệ"));

                // Lấy userId từ JWT token
                var userIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int currentUserId))
                    return Unauthorized(new BaseCommentResponse(401, "Token không hợp lệ hoặc không chứa thông tin người dùng"));

                // Chỉ cho phép user xem favorites của chính họ
                if (userId != currentUserId)
                    return StatusCode(403, new BaseCommentResponse(403, "Bạn không có quyền xem favorites của người dùng khác"));

                var favorites = await _unitOfWork.FavoriteRepository.GetFavoritesByUserIdAsync(userId);
                var result = _mapper.Map<List<FavoriteDTO>>(favorites);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseCommentResponse(500, "Đã xảy ra lỗi máy chủ nội bộ khi lấy danh sách yêu thích của người dùng"));
            }
        }

        [Authorize]
        [HttpGet("favorites/{id}")]
        public async Task<ActionResult> GetFavoriteById(int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new BaseCommentResponse(400, "ID yêu thích không hợp lệ"));

                // Lấy userId từ JWT token
                var userIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int currentUserId))
                    return Unauthorized(new BaseCommentResponse(401, "Token không hợp lệ hoặc không chứa thông tin người dùng"));

                var favorite = await _unitOfWork.FavoriteRepository.GetByIdAsync(id, x => x.User, x => x.Dish, x => x.Restaurant);
                if (favorite == null)
                    return NotFound(new BaseCommentResponse(404, "Không tìm thấy mục yêu thích"));

                // Chỉ cho phép user xem favorite của chính họ
                if (favorite.UserId != currentUserId)
                    return StatusCode(403, new BaseCommentResponse(403, "Bạn không có quyền xem favorite này"));

                var result = _mapper.Map<FavoriteDTO>(favorite);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseCommentResponse(500, "Đã xảy ra lỗi máy chủ nội bộ khi tìm kiếm mục yêu thích"));
            }
        }

        [Authorize]
        [HttpPost("favorites")]
        public async Task<ActionResult> AddNewFavorite([FromForm] CreateFavoriteRequestDTO requestDTO)
        {
            try
            {
                if (requestDTO == null)
                    return BadRequest(new BaseCommentResponse(400, "Dữ liệu yêu thích là bắt buộc"));

                if (!ModelState.IsValid)
                    return BadRequest(new BaseCommentResponse(400, "Dữ liệu đầu vào không hợp lệ"));

                // Lấy userId từ JWT token
                var userIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                    return Unauthorized(new BaseCommentResponse(401, "Token không hợp lệ hoặc không chứa thông tin người dùng"));

                // Map từ request DTO sang internal DTO với userId từ token
                var createFavoriteDTO = new CreateFavoriteDTO
                {
                    dishid = requestDTO.dishid,
                    restaurantid = requestDTO.restaurantid,
                    userid = userId // Tự động lấy từ JWT token
                };

                var ok = await _unitOfWork.FavoriteRepository.AddAsync(createFavoriteDTO);
                if (!ok)
                    return BadRequest(new BaseCommentResponse(400, "Không thêm được mục yêu thích. Món ăn, nhà hàng hoặc người dùng không tồn tại, hoặc bạn đã yêu thích món này rồi"));

                return Ok(ok);
            }
            catch (DbUpdateException dbEx)
            {
                Console.WriteLine($"Database error in AddNewFavorite: {dbEx.Message}");
                if (dbEx.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {dbEx.InnerException.Message}");
                    // Kiểm tra duplicate key error
                    if (dbEx.InnerException.Message.Contains("duplicate") || dbEx.InnerException.Message.Contains("UNIQUE"))
                    {
                        return BadRequest(new BaseCommentResponse(400, "Bạn đã yêu thích món này rồi"));
                    }
                }
                return StatusCode(500, new BaseCommentResponse(500, $"Lỗi cơ sở dữ liệu: {dbEx.InnerException?.Message ?? dbEx.Message}"));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in AddNewFavorite: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                    Console.WriteLine($"StackTrace: {ex.StackTrace}");
                }
                return StatusCode(500, new BaseCommentResponse(500, $"Đã xảy ra lỗi máy chủ nội bộ: {ex.Message}"));
            }
        }

        [Authorize]
        [HttpPut("favorites/{id}")]
        public async Task<ActionResult> UpdateFavorite(int id, [FromForm] UpdateFavoriteRequestDTO requestDTO)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new BaseCommentResponse(400, "ID yêu thích không hợp lệ"));

                if (requestDTO == null)
                    return BadRequest(new BaseCommentResponse(400, "Cần cập nhật dữ liệu"));

                if (!ModelState.IsValid)
                    return BadRequest(new BaseCommentResponse(400, "Dữ liệu đầu vào không hợp lệ"));

                // Lấy userId từ JWT token
                var userIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                    return Unauthorized(new BaseCommentResponse(401, "Token không hợp lệ hoặc không chứa thông tin người dùng"));

                // Kiểm tra favorite có thuộc về user này không
                var favorite = await _unitOfWork.FavoriteRepository.GetByIdAsync(id);
                if (favorite == null)
                    return NotFound(new BaseCommentResponse(404, "Không tìm thấy mục yêu thích"));

                if (favorite.UserId != userId)
                    return StatusCode(403, new BaseCommentResponse(403, "Bạn không có quyền cập nhật favorite này"));

                // Map từ request DTO sang internal DTO với userId từ token
                var updateFavoriteDTO = new UpdateFavoriteDTO
                {
                    dishid = requestDTO.dishid,
                    restaurantid = requestDTO.restaurantid,
                    userid = userId // Tự động lấy từ JWT token
                };

                var res = await _unitOfWork.FavoriteRepository.UpdateAsync(id, updateFavoriteDTO);
                return res ? Ok(new { message = "Cập nhật mục yêu thích thành công" }) : NotFound(new BaseCommentResponse(404, "Không tìm thấy mục yêu thích hoặc cập nhật không thành công"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseCommentResponse(500, "Đã xảy ra lỗi máy chủ nội bộ khi cập nhật mục yêu thích"));
            }
        }

        [Authorize]
        [HttpDelete("favorites/{id}")]
        public async Task<ActionResult> DeleteFavorite(int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new BaseCommentResponse(400, "ID yêu thích không hợp lệ"));

                // Lấy userId từ JWT token
                var userIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                    return Unauthorized(new BaseCommentResponse(401, "Token không hợp lệ hoặc không chứa thông tin người dùng"));

                // Kiểm tra favorite có thuộc về user này không
                var favorite = await _unitOfWork.FavoriteRepository.GetByIdAsync(id);
                if (favorite == null)
                    return NotFound(new BaseCommentResponse(404, "Không tìm thấy mục yêu thích"));

                if (favorite.UserId != userId)
                    return StatusCode(403, new BaseCommentResponse(403, "Bạn không có quyền xóa favorite này"));

                var res = await _unitOfWork.FavoriteRepository.DeleteAsync(id);
                return res ? Ok(new { message = "Mục yêu thích đã bị xóa thành công", id }) : NotFound(new BaseCommentResponse(404, "Không tìm thấy mục yêu thích"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseCommentResponse(500, "Đã xảy ra lỗi máy chủ nội bộ khi xóa mục yêu thích"));
            }
        }
    }
}