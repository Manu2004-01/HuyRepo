using AutoMapper;
using EatIT.Core.DTOs;
using EatIT.Core.Interface;
using EatIT.Infrastructure.Data.DTOs;
using EatIT.WebAPI.Errors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using StackExchange.Redis;

namespace EatIT.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RatingController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public RatingController(IUnitOfWork UnitOfWork, IMapper mapper)
        {
            _unitOfWork = UnitOfWork;
            _mapper = mapper;
        }

        [HttpGet("ratings")]
        public async Task<ActionResult> GetAllRating([FromQuery] string? sort = null)
        {
            try
            {
                var res = await _unitOfWork.RatingRepository.GetAllAsync(new Core.Sharing.RatingParams
                {
                    Sorting = sort
                });

                var totalIteams = await _unitOfWork.RatingRepository.CountAsync();
                var result = _mapper.Map<List<RatingDTO>>(res);
                return Ok(new { totalIteams, result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseCommentResponse(500, "Đã xảy ra lỗi máy chủ nội bộ khi đang tìm kiếm bài đánh giá"));
            }
        }

        [HttpGet("ratings/{id}")]
        [ResponseType(StatusCodes.Status200OK)]
        [ResponseType(typeof(BaseCommentResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult> GetRatingById(int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new BaseCommentResponse(400, "ID bài đánh giá không hợp lệ"));

                var res = await _unitOfWork.RatingRepository.GetByIdAsync(id, x => x.User, x => x.Restaurant);
                if (res == null)
                    return NotFound(new BaseCommentResponse(404, "Không tìm thấy bài đánh giá"));

                var result = _mapper.Map<RatingDTO>(res);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseCommentResponse(500, "Đã xảy ra lỗi máy chủ nội bộ khi đang tìm kiếm bài đánh giá"));
            }
        }

        [Authorize]
        [HttpPost("restaurants/{restaurantId:int}/ratings")]
        public async Task<ActionResult> AddRestaurantRating(int restaurantId, [FromBody] BaseRating body)
        {
            try
            {
                if (restaurantId <= 0)
                    return BadRequest(new BaseCommentResponse(400, "ID nhà hàng không hợp lệ"));

                if (!ModelState.IsValid)
                    return BadRequest(new BaseCommentResponse(400, "Dữ liệu đầu vào không hợp lệ"));

                var userIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                    return Unauthorized(new BaseCommentResponse(401, "Token không hợp lệ hoặc không chứa thông tin người dùng"));

                var dto = new CreateRatingDTO
                {
                    userid = userId,
                    restaurantid = restaurantId,
                    Star = body.Star,
                    Comment = body.Comment
                };

                var ok = await _unitOfWork.RatingRepository.AddAsync(dto);
                return ok ? Ok(new { message = "Đã thêm đánh giá" }) : BadRequest(new BaseCommentResponse(400, "Không thêm được bài đánh giá"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseCommentResponse(500, "Đã xảy ra lỗi máy chủ nội bộ khi thêm bài đánh giá"));
            }
        }

        [Authorize]
        [HttpPut("ratings/{id}")]
        public async Task<ActionResult> UpdateRating(int id, [FromForm] UpdateRatingDTO updateRatingDTO)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new BaseCommentResponse(400, "ID bài đánh giá không hợp lệ"));

                if (!ModelState.IsValid)
                    return BadRequest(new BaseCommentResponse(400, "Dữ liệu đầu vào không hợp lệ"));

                if (updateRatingDTO == null)
                    return BadRequest(new BaseCommentResponse(400, "Cần cập nhật dữ liệu"));

                var res = await _unitOfWork.RatingRepository.UpdateAsync(id, updateRatingDTO);
                return res ? Ok(updateRatingDTO) : NotFound(new BaseCommentResponse(404, "Không tìm thấy bài đánh giá hoặc cập nhật không thành công"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseCommentResponse(500, "Đã xảy ra lỗi máy chủ nội bộ khi cập nhật bài đánh giá"));
            }
        }

        [Authorize]
        [HttpDelete("ratings/{id}")]
        public async Task<ActionResult> DeleteRating(int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new BaseCommentResponse(400, "ID bài đánh giá không hợp lệ"));

                var res = await _unitOfWork.RatingRepository.DeleteAsync(id);
                return res ? Ok(new { message = "Bài đánh giá đã bị xóa thành công", id }) : NotFound(new BaseCommentResponse(404, "Không tìm thấy bài đánh giá"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseCommentResponse(500, "Đã xảy ra lỗi máy chủ nội bộ khi xóa bài đánh giá"));
            }
        }

        [HttpGet("restaurants/{restaurantId:int}/ratings")]
        public async Task<ActionResult> GetRatingsForRestaurant(int restaurantId, [FromQuery] string? sort = null)
        {
            try
            {
                if (restaurantId <= 0)
                    return BadRequest(new BaseCommentResponse(400, "ID nhà hàng không hợp lệ"));

                var res = await _unitOfWork.RatingRepository.GetByRestaurantAsync(restaurantId, new Core.Sharing.RatingParams
                {
                    Sorting = sort
                });

                var result = _mapper.Map<List<RatingDTO>>(res);
                return Ok(new { totalIteams = result.Count, result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseCommentResponse(500, "Đã xảy ra lỗi máy chủ nội bộ khi đang tìm kiếm bài đánh giá"));
            }
        }
    }
}
