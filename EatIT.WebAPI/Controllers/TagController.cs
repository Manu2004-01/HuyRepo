using AutoMapper;
using Azure;
using EatIT.Core.DTOs;
using EatIT.Core.Entities;
using EatIT.Core.Interface;
using EatIT.Infrastructure.Data.DTOs;
using EatIT.WebAPI.Errors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace EatIT.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TagController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public TagController(IUnitOfWork UnitOfWork, IMapper mapper)
        {
            _unitOfWork = UnitOfWork;
            _mapper = mapper;
        }

        [HttpGet("get-all-tags")]
        public async Task<ActionResult> GetAllTag()
        {
            try
            {
                var tag = await _unitOfWork.TagRepository.GetAllAsync();

                var item = _mapper.Map<List<TagDTO>>(tag);
                return Ok(item);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseCommentResponse(500, "Đã xảy ra lỗi máy chủ nội bộ khi đang tìm kiếm nhãn nhà hàng"));
            }
        }

        [HttpPost("add-new-tag")]
        public async Task<ActionResult> AddNewTag([FromForm] CreateTagDTO createTagDTO)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new BaseCommentResponse(400, "Dữ liệu đầu vào không hợp lệ"));

                if (createTagDTO == null)
                    return BadRequest(new BaseCommentResponse(400, "Dữ liệu người dùng là bắt buộc"));

                var item = await _unitOfWork.TagRepository.AddAsync(createTagDTO);

                if (!item)
                    return BadRequest(new BaseCommentResponse(400, "Không thêm được nhãn nhà hàng. Tải ảnh lên không thành công hoặc tạo nhãn nhà hàng không thành công."));

                return Ok(item);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseCommentResponse(500, "Đã xảy ra lỗi máy chủ nội bộ khi thêm nhãn nhà hàng"));
            }
        }

        [HttpDelete("delete-existing-tag/{id}")]
        public async Task<ActionResult> DeleteTag(int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new BaseCommentResponse(400, "ID nhãn nhà hàng không hợp lệ"));

                var res = await _unitOfWork.TagRepository.DeleteAsync(id);
                return res ? Ok(new { message = "nhãn nhà hàng đã bị xóa thành công", id }) : NotFound(new BaseCommentResponse(404, "Không tìm thấy nhãn nhà hàng"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseCommentResponse(500, "Đã xảy ra lỗi máy chủ nội bộ khi xóa nhãn nhà hàng"));
            }
        }
    }
}
