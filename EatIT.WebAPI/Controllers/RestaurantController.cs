using AutoMapper;
using EatIT.Core.DTOs;
using EatIT.Core.Entities;
using EatIT.Core.Interface;
using EatIT.Core.Services;
using EatIT.Infrastructure.Data.DTOs;
using EatIT.WebAPI.Errors;
using EatIT.WebAPI.MyHelper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace EatIT.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RestaurantController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public RestaurantController(IUnitOfWork UnitOfWork, IMapper mapper)
        {
            _unitOfWork = UnitOfWork;
            _mapper = mapper;
        }

        [HttpGet("restaurants")]
        public async Task<ActionResult> GetAllUser([FromQuery] string? search = null)
        {
            try
            {
                var res = await _unitOfWork.RestaurantRepository.GetAllAsync(new Core.Sharing.RestaurantParams
                {
                    Search = search
                }
                );

                var totalIteams = await _unitOfWork.RestaurantRepository.CountAsync();
                var result = _mapper.Map<List<RestaurantDTO>>(res);
                return Ok(new { totalIteams, result });
            }
            catch (Exception)
            {
                return StatusCode(500, new BaseCommentResponse(500, "Đã xảy ra lỗi máy chủ nội bộ khi đang tìm kiếm nhà hàng"));
            }
        }

        [HttpGet("restaurants/{id}")]
        [ResponseType(StatusCodes.Status200OK)]
        [ResponseType(typeof(BaseCommentResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult> GetRestaurantById(int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new BaseCommentResponse(400, "ID nhà hàng không hợp lệ"));

                var res = await _unitOfWork.RestaurantRepository.GetByIdAsync(id, x => x.Tag);
                if (res == null)
                    return NotFound(new BaseCommentResponse(404, "Không tìm thấy nhà hàng"));

                var result = _mapper.Map<RestaurantDTO>(res);
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, new BaseCommentResponse(500, "Đã xảy ra lỗi máy chủ nội bộ khi đang tìm kiếm nhà hàng"));
            }
        }

        [HttpPost("restaurants")]
        public async Task<ActionResult> AddNewRestaurant([FromForm] CreateRestaurantDTO createRestaurantDTO)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new BaseCommentResponse(400, "Dữ liệu đầu vào không hợp lệ"));
                if (createRestaurantDTO == null)
                    return BadRequest(new BaseCommentResponse(400, "Dữ liệu nhà hàng là bắt buộc"));
                
                var ok = await _unitOfWork.RestaurantRepository.AddAsync(createRestaurantDTO);
                if (!ok)
                    return BadRequest(new BaseCommentResponse(400, "Không thêm được nhà hàng. Tải ảnh lên không thành công hoặc tạo nhà hàng không thành công."));

                return Ok(ok);
            }
            catch (Exception)
            {
                return StatusCode(500, new BaseCommentResponse(500, "Đã xảy ra lỗi máy chủ nội bộ khi thêm nhà hàng"));
            }
        }

        [HttpPut("restsaurants/{id}")]
        public async Task<ActionResult> UpdateRestaurant(int id, [FromForm] UpdateRestaurantDTO updateRestaurantDTO)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new BaseCommentResponse(400, "ID nhà hàng không hợp lệ"));

                if (!ModelState.IsValid)
                    return BadRequest(new BaseCommentResponse(400, "Dữ liệu đầu vào không hợp lệ"));

                if (updateRestaurantDTO == null)
                    return BadRequest(new BaseCommentResponse(400, "Cần cập nhật dữ liệu"));

                var res = await _unitOfWork.RestaurantRepository.UpdateAsync(id, updateRestaurantDTO);
                return res ? Ok(updateRestaurantDTO) : NotFound(new BaseCommentResponse(404, "Không tìm thấy nhà hàng hoặc cập nhật không thành công"));
            }
            catch (Exception)
            {
                return StatusCode(500, new BaseCommentResponse(500, "Đã xảy ra lỗi máy chủ nội bộ khi cập nhật nhà hàng"));
            }
        }

        [HttpGet("restaurants-map")]
        public async Task<ActionResult> GetAllRestaurantsForMap([FromQuery] string? search = null)
        {
            try
            {
                var res = await _unitOfWork.RestaurantRepository.GetAllAsync(new Core.Sharing.RestaurantParams
                {
                    Search = search
                });

                // Return simplified data for map markers
                var mapData = res.Select(r => new
                {
                    id = r.ResId,
                    resName = r.ResName,
                    resAddress = r.ResAddress,
                    latitude = r.Latitude,
                    longitude = r.Longitude,
                    restaurantImg = r.RestaurantImg,
                    tagName = r.Tag?.TagName
                }).ToList();

                return Ok(new { totalItems = mapData.Count, restaurants = mapData });
            }
            catch (Exception)
            {
                return StatusCode(500, new BaseCommentResponse(500, "Đã xảy ra lỗi máy chủ nội bộ khi lấy danh sách nhà hàng cho bản đồ"));
            }
        }

        [HttpDelete("restaurants/{id}")]
        public async Task<ActionResult> DeleteRestaurant(int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new BaseCommentResponse(400, "ID nhà hàng không hợp lệ"));

                var res = await _unitOfWork.RestaurantRepository.DeleteAsync(id);
                return res ? Ok(new { message = "Nhà hàng đã bị xóa thành công", id }) : NotFound(new BaseCommentResponse(404, "Không tìm thấy nhà hàng"));
            }
            catch (Exception e)
            {
                return StatusCode(500, new BaseCommentResponse(500, "Đã xảy ra lỗi máy chủ nội bộ khi xóa nhà hàng"));
            }
        }

        //Get Nearby Restaurants using JWT Token
        [Authorize]
        [HttpGet("nearby")]
        [ResponseType(StatusCodes.Status200OK)]
        [ResponseType(typeof(BaseCommentResponse), StatusCodes.Status400BadRequest)]
        [ResponseType(typeof(BaseCommentResponse), StatusCodes.Status401Unauthorized)]
        [ResponseType(typeof(BaseCommentResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult> GetNearbyRestaurants([FromQuery] double radiusKm = 5.0)
        {
            try
            {
                // Lấy user ID từ JWT token
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new BaseCommentResponse(401, "Token không hợp lệ hoặc không chứa thông tin người dùng"));
                }

                var user = await _unitOfWork.UserRepository.GetByIdAsync(userId);

                if (user?.UserLatitude == null || user?.UserLongitude == null)
                {
                    return BadRequest(new BaseCommentResponse(400, "Bạn chưa cập nhật vị trí. Vui lòng cập nhật vị trí trước khi tìm nhà hàng gần đây."));
                }

                //var restaurants = await _unitOfWork.RestaurantRepository.GetAllAsync();
                var restaurants = await _unitOfWork.RestaurantRepository.GetAllAsync(new Core.Sharing.RestaurantParams());
                var distanceService = new DistanceCalculationService();

                var nearbyRestaurants = restaurants
                    .Where(r =>
                    {
                        var distance = distanceService.CalculateDistanceInKm(
                            user.UserLatitude.Value,
                            user.UserLongitude.Value,
                            r.Latitude,
                            r.Longitude
                        );
                        return distance <= radiusKm;
                    })
                    .Select(r =>
                    {
                        var distance = distanceService.CalculateDistanceInKm(
                            user.UserLatitude.Value,
                            user.UserLongitude.Value,
                            r.Latitude,
                            r.Longitude
                        );

                        var restaurantDto = _mapper.Map<RestaurantDTO>(r);
                        restaurantDto.DistanceFromUser = Math.Round(distance, 3);
                        restaurantDto.DistanceDisplay = Locations.FormatDistance(distance);

                        return restaurantDto;
                    })
                    .OrderBy(r => r.DistanceFromUser)
                    .ToList();

                return Ok(new
                {
                    totalItems = nearbyRestaurants.Count,
                    radiusKm = radiusKm,
                    userLocation = new { user.UserLatitude, user.UserLongitude },
                    restaurants = nearbyRestaurants
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseCommentResponse(500, "Đã xảy ra lỗi máy chủ nội bộ khi lấy nhà hàng gần đây"));
            }
        }

        //Get Restaurants with Distance using JWT Token
        [Authorize]
        [HttpGet("distance")]
        [ResponseType(StatusCodes.Status200OK)]
        [ResponseType(typeof(BaseCommentResponse), StatusCodes.Status400BadRequest)]
        [ResponseType(typeof(BaseCommentResponse), StatusCodes.Status401Unauthorized)]
        [ResponseType(typeof(BaseCommentResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult> GetRestaurantsWithDistance([FromQuery] string? search = null)
        {
            try
            {
                // Lấy user ID từ JWT token
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new BaseCommentResponse(401, "Token không hợp lệ hoặc không chứa thông tin người dùng"));
                }

                var user = await _unitOfWork.UserRepository.GetByIdAsync(userId);

                if (user?.UserLatitude == null || user?.UserLongitude == null)
                {
                    return BadRequest(new BaseCommentResponse(400, "Bạn chưa cập nhật vị trí. Vui lòng cập nhật vị trí trước khi xem khoảng cách đến nhà hàng."));
                }

                var restaurants = await _unitOfWork.RestaurantRepository.GetAllAsync(new Core.Sharing.RestaurantParams
                {
                    Search = search
                });

                var distanceService = new DistanceCalculationService();
                var result = restaurants.Select(r =>
                {
                    var distance = distanceService.CalculateDistanceInKm(
                        user.UserLatitude.Value,
                        user.UserLongitude.Value,
                        r.Latitude,
                        r.Longitude
                    );

                    var restaurantDto = _mapper.Map<RestaurantDTO>(r);
                    restaurantDto.DistanceFromUser = Math.Round(distance, 2);
                    restaurantDto.DistanceDisplay = Locations.FormatDistance(distance);

                    return restaurantDto;
                }).OrderBy(r => r.DistanceFromUser).ToList();

                return Ok(new 
                { 
                    totalItems = result.Count, 
                    userLocation = new { user.UserLatitude, user.UserLongitude },
                    restaurants = result 
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseCommentResponse(500, "Đã xảy ra lỗi máy chủ nội bộ khi lấy danh sách nhà hàng với khoảng cách"));
            }
        }
    }
}
