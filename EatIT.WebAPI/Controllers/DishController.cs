using AutoMapper;
using EatIT.Core.DTOs;
using EatIT.Core.Interface;
using EatIT.Infrastructure.Data.DTOs;
using EatIT.WebAPI.Errors;
using EatIT.WebAPI.MyHelper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;

namespace EatIT.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DishController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public DishController(IUnitOfWork UnitOfWork, IMapper mapper)
        {
            _unitOfWork = UnitOfWork;
            _mapper = mapper;
        }

        [HttpGet("dishes")]
        public async Task<ActionResult> GetAllDish([FromQuery] string? sort = null, [FromQuery] decimal? price = null, [FromQuery] string? search = null)
        {
            try
            {
                var res = await _unitOfWork.DishRepository.GetAllAsync(new Core.Sharing.DishParams
                {
                    Sorting = sort,
                    DishPrice = price,
                    Search = search
                });

                var totalIteams = res.Count();
                var result = _mapper.Map<List<DishDTO>>(res);
                return Ok(new { totalIteams, result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseCommentResponse(500, "Đã xảy ra lỗi máy chủ nội bộ khi đang tìm kiếm món ăn"));
            }
        }

        [HttpGet("dishes/{id}")]
        [ResponseType(StatusCodes.Status200OK)]
        [ResponseType(typeof(BaseCommentResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult> GetDishById(int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new BaseCommentResponse(400, "ID món ăn không hợp lệ"));
                var res = await _unitOfWork.DishRepository.GetByIdAsync(id, x => x.Restaurant);
                if (res == null)
                    return NotFound(new BaseCommentResponse(404, "Không tìm thấy món ăn"));
                var result = _mapper.Map<DishDTO>(res);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseCommentResponse(500, "Đã xảy ra lỗi máy chủ nội bộ khi đang tìm kiếm món ăn"));
            }
        }

        [HttpPost("dishes")]
        public async Task<ActionResult> AddNewDish([FromForm] CreateDishDTO createDishDTO)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new BaseCommentResponse(400, "Dữ liệu đầu vào không hợp lệ"));
                if (createDishDTO == null)
                    return BadRequest(new BaseCommentResponse(400, "Dữ liệu món ăn là bắt buộc"));
                var ok = await _unitOfWork.DishRepository.AddAsync(createDishDTO);
                if (!ok)
                    return BadRequest(new BaseCommentResponse(400, "Không thêm được món ăn. Tải ảnh lên không thành công hoặc tạo món ăn không thành công."));
                return Ok(ok);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseCommentResponse(500, "Đã xảy ra lỗi máy chủ nội bộ khi thêm món ăn"));
            }
        }

        [HttpPut("dishes/{id}")]
        public async Task<ActionResult> UpdateDish(int id, [FromForm] UpdateDishDTO updateDishDTO)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new BaseCommentResponse(400, "ID món ăn không hợp lệ"));
                if (!ModelState.IsValid)
                    return BadRequest(new BaseCommentResponse(400, "Dữ liệu đầu vào không hợp lệ"));

                if (updateDishDTO == null)
                    return BadRequest(new BaseCommentResponse(400, "Cần cập nhật dữ liệu"));

                var res = await _unitOfWork.DishRepository.UpdateAsync(id, updateDishDTO);
                return res ? Ok(updateDishDTO) : NotFound(new BaseCommentResponse(404, "Không tìm thấy món ăn hoặc cập nhật không thành công"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseCommentResponse(500, "Đã xảy ra lỗi máy chủ nội bộ khi cập nhật món ăn"));
            }
        }

        [HttpDelete("dishes/{id}")]
        public async Task<ActionResult> DeleteDish(int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new BaseCommentResponse(400, "ID món ăn không hợp lệ"));

                var res = await _unitOfWork.DishRepository.DeleteAsync(id);
                return res ? Ok(new { message = "món ăn đã bị xóa thành công", id }) : NotFound(new BaseCommentResponse(404, "Không tìm thấy món ăn"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseCommentResponse(500, "Đã xảy ra lỗi máy chủ nội bộ khi xóa món ăn"));
            }
        }
    }
}
