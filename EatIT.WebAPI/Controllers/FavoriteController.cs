using AutoMapper;
using EatIT.Core.DTOs;
using EatIT.Core.Interface;
using EatIT.WebAPI.Errors;
using Microsoft.AspNetCore.Mvc;

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

        [HttpGet("get-all-favorites")]
        public async Task<ActionResult> GetAllFavorites()
        {
            try
            {
                var favorites = await _unitOfWork.FavoriteRepository.GetAllAsync();
                var result = _mapper.Map<List<FavoriteDTO>>(favorites);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseCommentResponse(500, "Đã xảy ra lỗi máy chủ nội bộ khi lấy danh sách yêu thích"));
            }
        }

        [HttpGet("get-favorites-by-user/{userId}")]
        public async Task<ActionResult> GetFavoritesByUser(int userId)
        {
            try
            {
                if (userId <= 0)
                    return BadRequest(new BaseCommentResponse(400, "ID người dùng không hợp lệ"));

                var favorites = await _unitOfWork.FavoriteRepository.GetFavoritesByUserIdAsync(userId);
                var result = _mapper.Map<List<FavoriteDTO>>(favorites);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseCommentResponse(500, "Đã xảy ra lỗi máy chủ nội bộ khi lấy danh sách yêu thích của người dùng"));
            }
        }

        [HttpGet("get-favorite-by-id/{id}")]
        public async Task<ActionResult> GetFavoriteById(int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new BaseCommentResponse(400, "ID yêu thích không hợp lệ"));

                var favorite = await _unitOfWork.FavoriteRepository.GetByIdAsync(id, x => x.User, x => x.Dish, x => x.Restaurant);
                if (favorite == null)
                    return NotFound(new BaseCommentResponse(404, "Không tìm thấy mục yêu thích"));

                var result = _mapper.Map<FavoriteDTO>(favorite);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseCommentResponse(500, "Đã xảy ra lỗi máy chủ nội bộ khi tìm kiếm mục yêu thích"));
            }
        }

        [HttpPost("add-new-favorite")]
        public async Task<ActionResult> AddNewFavorite([FromForm] CreateFavoriteDTO createFavoriteDTO)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new BaseCommentResponse(400, "Dữ liệu đầu vào không hợp lệ"));

                if (createFavoriteDTO == null)
                    return BadRequest(new BaseCommentResponse(400, "Dữ liệu yêu thích là bắt buộc"));
                
                // Kiểm tra nếu dishid null hoặc 0
                if (!createFavoriteDTO.dishid.HasValue || createFavoriteDTO.dishid.Value == 0)
                {
                    return BadRequest(new BaseCommentResponse(400, "Phải chọn một món ăn"));
                }

                var ok = await _unitOfWork.FavoriteRepository.AddAsync(createFavoriteDTO);
                if (!ok)
                    return BadRequest(new BaseCommentResponse(400, "Không thêm được mục yêu thích. Món ăn, combo, nhà hàng hoặc người dùng không tồn tại"));

                return Ok(ok);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in AddNewFavorite: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                return StatusCode(500, new BaseCommentResponse(500, "Đã xảy ra lỗi máy chủ nội bộ khi thêm mục yêu thích"));
            }
        }

        [HttpPut("update-existing-favorite/{id}")]
        public async Task<ActionResult> UpdateFavorite(int id, [FromForm] UpdateFavoriteDTO updateFavoriteDTO)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new BaseCommentResponse(400, "ID yêu thích không hợp lệ"));

                if (!ModelState.IsValid)
                    return BadRequest(new BaseCommentResponse(400, "Dữ liệu đầu vào không hợp lệ"));

                if (updateFavoriteDTO == null)
                    return BadRequest(new BaseCommentResponse(400, "Cần cập nhật dữ liệu"));

                var res = await _unitOfWork.FavoriteRepository.UpdateAsync(id, updateFavoriteDTO);
                return res ? Ok(updateFavoriteDTO) : NotFound(new BaseCommentResponse(404, "Không tìm thấy mục yêu thích hoặc cập nhật không thành công"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseCommentResponse(500, "Đã xảy ra lỗi máy chủ nội bộ khi cập nhật mục yêu thích"));
            }
        }

        [HttpDelete("delete-existing-favorite/{id}")]
        public async Task<ActionResult> DeleteFavorite(int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new BaseCommentResponse(400, "ID yêu thích không hợp lệ"));

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