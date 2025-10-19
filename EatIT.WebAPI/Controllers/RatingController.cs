using AutoMapper;
using EatIT.Core.DTOs;
using EatIT.Core.Interface;
using EatIT.Infrastructure.Data.DTOs;
using EatIT.WebAPI.Errors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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

        [HttpGet("get-all-ratings")]
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

        [HttpGet("get-rating-by-id/{id}")]
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

        [HttpPost("add-new-rating")]
        public async Task<ActionResult> AddNewRating([FromForm] CreateRatingDTO createRatingDTO)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new BaseCommentResponse(400, "Dữ liệu đầu vào không hợp lệ"));

                if (createRatingDTO == null)
                    return BadRequest(new BaseCommentResponse(400, "Dữ liệu bài đánh giá là bắt buộc"));

                var ok = await _unitOfWork.RatingRepository.AddAsync(createRatingDTO);
                if (!ok)
                    return BadRequest(new BaseCommentResponse(400, "Không thêm được bài đánh giá"));

                return Ok(ok);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseCommentResponse(500, "Đã xảy ra lỗi máy chủ nội bộ khi thêm bài đánh giá"));
            }
        }

        [HttpPut("update-existing-rating/{id}")]
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

        [HttpDelete("delete-existing-rating/{id}")]
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
    }
}
