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

        [HttpGet("get-all-restaurants")]
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

        [HttpGet("get-restaurant-by-id/{id}")]
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

        [HttpPost("add-new-restaurant")]
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

        [HttpPut("update-existing-restsaurant/{id}")]
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

        [HttpGet("get-restaurant-for-map/{id}")]
        [ResponseType(StatusCodes.Status200OK)]
        [ResponseType(typeof(BaseCommentResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult> GetRestaurantForMap(int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new BaseCommentResponse(400, "ID nhà hàng không hợp lệ"));

                var res = await _unitOfWork.RestaurantRepository.GetByIdAsync(id, x => x.Tag);
                if (res == null)
                    return NotFound(new BaseCommentResponse(404, "Không tìm thấy nhà hàng"));

                // Return data specifically for Google Maps integration
                var mapData = new
                {
                    resName = res.ResName,
                    resAddress = res.ResAddress,
                    latitude = res.Latitude,
                    longitude = res.Longitude,
                    restaurantImg = res.RestaurantImg,
                    openingHours = res.OpeningHours,
                    tagName = res.Tag?.TagName
                };

                return Ok(mapData);
            }
            catch (Exception)
            {
                return StatusCode(500, new BaseCommentResponse(500, "Đã xảy ra lỗi máy chủ nội bộ khi lấy dữ liệu nhà hàng cho bản đồ"));
            }
        }

        [HttpGet("get-all-restaurants-for-map")]
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
                    id = r.Id,
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

        [HttpDelete("delete-existing-restaurant/{id}")]
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

        [HttpPost("update-user-location-by-id/{userId}")]
        public async Task<ActionResult> UpdateUserLocationById(int userId, [FromBody] UpdateUserLocationDTO locationDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    var errorDetails = string.Join(", ", errors);
                    Console.WriteLine($"Debug: ModelState errors = {errorDetails}");
                    return BadRequest(new BaseCommentResponse(400, $"Dữ liệu tọa độ không hợp lệ: {errorDetails}"));
                }

                Console.WriteLine($"Debug: userId = {userId}");
                Console.WriteLine($"Debug: locationDto = {locationDto?.UserLatitude}, {locationDto?.UserLongitude}");

                if (userId <= 0)
                {
                    return BadRequest(new BaseCommentResponse(400, "User ID không hợp lệ"));
                }

                // Kiểm tra user có tồn tại không
                var userExists = await _unitOfWork.UserRepository.GetByIdAsync(userId);
                if (userExists == null)
                {
                    Console.WriteLine($"Debug: User with ID {userId} not found in database");
                    return NotFound(new BaseCommentResponse(404, $"User với ID {userId} không tồn tại"));
                }

                Console.WriteLine($"Debug: User found - {userExists.UserName}");

                // Sử dụng hàm riêng để cập nhật location
                var success = await _unitOfWork.UserRepository.UpdateUserLocationAsync(userId, locationDto);

                Console.WriteLine($"Debug: UpdateUserLocationAsync result = {success}");

                if (!success)
                {
                    Console.WriteLine("Debug: UpdateUserLocationAsync returned false");
                    return NotFound(new BaseCommentResponse(404, "Cập nhật location thất bại"));
                }

                var locationInfo = await _unitOfWork.UserRepository.GetUserLocationAsync(userId);
                Console.WriteLine($"Debug: Location updated successfully");

                return Ok(locationInfo);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Debug: Exception = {ex.Message}");
                Console.WriteLine($"Debug: StackTrace = {ex.StackTrace}");
                return StatusCode(500, new BaseCommentResponse(500, $"Lỗi khi cập nhật vị trí user: {ex.Message}"));
            }
        }

        [HttpGet("get-nearby-restaurants-by-user-id/{userId}")]
        public async Task<ActionResult> GetNearbyRestaurantsByUserId(int userId, [FromQuery] double radiusKm = 5.0)
        {
            try
            {
                Console.WriteLine($"Debug: Getting nearby restaurants for userId = {userId}, radius = {radiusKm}km");
                
                if (userId <= 0)
                {
                    return BadRequest(new BaseCommentResponse(400, "User ID không hợp lệ"));
                }

                var user = await _unitOfWork.UserRepository.GetByIdAsync(userId);

                if (user?.UserLatitude == null || user?.UserLongitude == null)
                {
                    Console.WriteLine($"Debug: User {userId} has no location data");
                    return BadRequest(new BaseCommentResponse(400, "User chưa cập nhật vị trí"));
                }

                Console.WriteLine($"Debug: User location: {user.UserLatitude}, {user.UserLongitude}");

                var restaurants = await _unitOfWork.RestaurantRepository.GetAllAsync();
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
                        restaurantDto.DistanceFromUser = Math.Round(distance, 3); // 3 chữ số thập phân
                        restaurantDto.DistanceDisplay = Locations.FormatDistance(distance);

                        return restaurantDto;
                    })
                    .OrderBy(r => r.DistanceFromUser)
                    .ToList();

                Console.WriteLine($"Debug: Found {nearbyRestaurants.Count} nearby restaurants");

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
                Console.WriteLine($"Debug: Exception in GetNearbyRestaurantsByUserId = {ex.Message}");
                return StatusCode(500, new BaseCommentResponse(500, "Lỗi khi lấy nhà hàng gần đây"));
            }
        }

        [HttpGet("get-user-location-by-id/{userId}")]
        public async Task<ActionResult> GetUserLocationById(int userId)
        {
            try
            {
                Console.WriteLine($"Debug: Getting location for userId = {userId}");
                
                if (userId <= 0)
                {
                    return BadRequest(new BaseCommentResponse(400, "User ID không hợp lệ"));
                }

                var locationInfo = await _unitOfWork.UserRepository.GetUserLocationAsync(userId);

                if (locationInfo == null)
                {
                    Console.WriteLine($"Debug: No location found for userId = {userId}");
                    return NotFound(new BaseCommentResponse(404, "User chưa cập nhật vị trí"));
                }

                Console.WriteLine($"Debug: Location found for userId = {userId}: {locationInfo.UserLatitude}, {locationInfo.UserLongitude}");
                return Ok(locationInfo);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Debug: Exception in GetUserLocationById = {ex.Message}");
                return StatusCode(500, new BaseCommentResponse(500, $"Lỗi khi lấy vị trí user: {ex.Message}"));
            }
        }

        [HttpGet("get-restaurants-with-distance-by-user-id/{userId}")]
        public async Task<ActionResult> GetRestaurantsWithDistanceByUserId(int userId, [FromQuery] string? search = null)
        {
            try
            {
                Console.WriteLine($"Debug: Getting restaurants with distance for userId = {userId}");
                
                if (userId <= 0)
                {
                    return BadRequest(new BaseCommentResponse(400, "User ID không hợp lệ"));
                }

                var user = await _unitOfWork.UserRepository.GetByIdAsync(userId);

                if (user?.UserLatitude == null || user?.UserLongitude == null)
                {
                    Console.WriteLine($"Debug: User {userId} has no location data");
                    return BadRequest(new BaseCommentResponse(400, "User chưa cập nhật vị trí"));
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

                Console.WriteLine($"Debug: Found {result.Count} restaurants with distance");

                return Ok(new { totalItems = result.Count, restaurants = result });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Debug: Exception in GetRestaurantsWithDistanceByUserId = {ex.Message}");
                return StatusCode(500, new BaseCommentResponse(500, "Lỗi khi lấy danh sách nhà hàng với khoảng cách"));
            }
        }
    }
}
