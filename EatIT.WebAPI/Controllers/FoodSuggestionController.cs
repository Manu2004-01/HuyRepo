using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Text.Json;
using EatIT.Core.Interface;
using EatIT.Core.Sharing;
using EatIT.Core.DTOs;
using EatIT.WebAPI.MyHelper;
using Microsoft.AspNetCore.Authorization;

namespace EatIT.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FoodSuggestionController : Controller
    {
		private readonly HttpClient _httpClient;
		private readonly string _apiKey;
		private readonly string _model;
		private readonly IUnitOfWork _unitOfWork;
		private readonly IConfiguration _configuration;
		private class DishOption { public int DishId { get; set; } public string DishName { get; set; } }

		public FoodSuggestionController(IHttpClientFactory httpClientFactory, IConfiguration config, IUnitOfWork unitOfWork)
        {
            _httpClient = httpClientFactory.CreateClient();
			_apiKey = config["Gemini:ApiKey"];
			_model = config["Gemini:Model"] ?? "gemini-2.5-flash";
			_unitOfWork = unitOfWork;
			_configuration = config;
        }

		[Authorize]
		[HttpPost]
		public async Task<IActionResult> GetSuggestion([FromBody] FoodSuggestionDTO request = null)
        {
			double ToRad(double x) => x * Math.PI / 180d;
			double DistKm(double aLat, double aLng, double bLat, double bLng)
			{
				var dLat = ToRad(bLat - aLat);
				var dLng = ToRad(bLng - aLng);
				var rLat1 = ToRad(aLat);
				var rLat2 = ToRad(bLat);
				var s = Math.Sin(dLat / 2d) * Math.Sin(dLat / 2d) + Math.Cos(rLat1) * Math.Cos(rLat2) * Math.Sin(dLng / 2d) * Math.Sin(dLng / 2d);
				var c = 2d * Math.Atan2(Math.Sqrt(s), Math.Sqrt(1d - s));
				return 6371d * c;
			}

			// Lấy bán kính từ request, mặc định là 5 km nếu không được cung cấp
			double radiusKm = request?.RadiusKm ?? 5.0;
			
			// Validate radius: phải lớn hơn 0 và không quá 100 km
			if (radiusKm <= 0 || radiusKm > 100)
			{
				return BadRequest(new { message = "Bán kính phải lớn hơn 0 và không quá 100 km" });
			}

			double? lat = null;
			double? lng = null;
			var currentUserId = Locations.GetCurrentUserId(User);
			if (currentUserId > 0)
			{
				var userLocation = await _unitOfWork.UserRepository.GetUserLocationAsync(currentUserId);
				if (userLocation != null)
				{
					lat = userLocation.UserLatitude;
					lng = userLocation.UserLongitude;
				}
			}
			var dishes = await _unitOfWork.DishRepository.GetAllAsync(new DishParams());
			List<DishOption> limited;

			if (lat.HasValue && lng.HasValue)
			{
				var restaurants = await _unitOfWork.RestaurantRepository.GetAllAsync(new RestaurantParams());
				if (restaurants.Any())
				{
					var nearbyRestaurants = restaurants
						.Where(r => DistKm(lat.Value, lng.Value, r.Latitude, r.Longitude) <= radiusKm)
						.OrderBy(r => DistKm(lat.Value, lng.Value, r.Latitude, r.Longitude))
						.ToList();

					var ordered = nearbyRestaurants;

					List<DishOption> BuildLimitedForRes(int resId) => dishes
						.Where(d => d.ResId == resId)
						.Take(50)
						.Select(d => new DishOption { DishId = d.DishId, DishName = d.DishName })
						.ToList();

					limited = new List<DishOption>();
					int restaurantCount = 0;
					foreach (var res in ordered)
					{
						var dishesFromRes = BuildLimitedForRes(res.ResId);
						limited.AddRange(dishesFromRes);
						restaurantCount++;
						if (restaurantCount >= 5 || limited.Count >= 50)
							break;
					}
				}
				else
				{
					limited = dishes.Take(50).Select(d => new DishOption { DishId = d.DishId, DishName = d.DishName }).ToList();
				}
			}
			else
			{
				limited = dishes.Take(50).Select(d => new DishOption { DishId = d.DishId, DishName = d.DishName }).ToList();
			}

			var dishMap = dishes.ToDictionary(d => d.DishId);
			var profile = currentUserId > 0 ? await _unitOfWork.UserRepository.GetProfileAsync(currentUserId) : null;
			var hasPremium = false;
			
			if (currentUserId > 0)
			{
				var activePremium = await _unitOfWork.PaymentRepository.GetActivePremiumByUserIdAsync(currentUserId);
				hasPremium = activePremium != null;
			}

			IEnumerable<string> SplitTerms(string s) => (s ?? string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim().ToLowerInvariant()).Where(x => x.Length > 0);
			bool Match(string text, IEnumerable<string> terms)
			{
				if (string.IsNullOrWhiteSpace(text)) return false;
				var l = text.ToLowerInvariant();
				foreach (var t in terms) if (l.Contains(t)) return true;
				return false;
			}

			if (profile != null)
			{
				if (profile.IsVegetarian)
				{
					limited = limited.Where(x => dishMap.TryGetValue(x.DishId, out var d) && d.IsVegan).ToList();
				}

				if (hasPremium)
				{
					var dislikeTerms = SplitTerms(profile.Dislike ?? string.Empty);
					var allergyTerms = SplitTerms(profile.Allergy ?? string.Empty);
					if (dislikeTerms.Any() || allergyTerms.Any())
					{
						limited = limited.Where(x =>
						{
							if (!dishMap.TryGetValue(x.DishId, out var d)) return false;
							var name = d.DishName ?? string.Empty;
							var desc = d.DishDescription ?? string.Empty;
							if (Match(name, dislikeTerms) || Match(desc, dislikeTerms)) return false;
							if (Match(name, allergyTerms) || Match(desc, allergyTerms)) return false;
							return true;
						}).ToList();
					}
					var prefTerms = SplitTerms(profile.Preference ?? string.Empty);
					if (prefTerms.Any())
					{
						limited = limited
							.OrderByDescending(x =>
							{
								if (!dishMap.TryGetValue(x.DishId, out var d)) return 0;
								var name = d.DishName ?? string.Empty;
								var desc = d.DishDescription ?? string.Empty;
								return Match(name, prefTerms) || Match(desc, prefTerms) ? 1 : 0;
							})
							.Take(50)
							.ToList();
					}
				}
			}

			if (limited.Count == 0)
			{
				return Ok(new { suggestions = new List<object>() });
			}

			var optionsList = string.Join("\n", limited.Select(x => $"{x.DishId}\t{x.DishName}"));
			var promptBuilder = new StringBuilder()
				.AppendLine("Hãy chọn từ 5 đến 10 món ăn trong danh sách sau, ưu tiên các món từ nhà hàng gần nhất.")
				.AppendLine("Trả về đúng JSON với dạng {\"ids\": [<DishId1>, <DishId2>, ...]} không kèm giải thích.")
				.AppendLine("Ưu tiên tuân thủ sở thích và hạn chế của người dùng nếu có.");
			if (profile != null)
			{
				if (profile.IsVegetarian) promptBuilder.AppendLine("Người dùng ăn chay.");
				
				if (hasPremium)
				{
					if (!string.IsNullOrWhiteSpace(profile.Preference)) promptBuilder.AppendLine($"Ưu tiên: {profile.Preference}");
					if (!string.IsNullOrWhiteSpace(profile.Dislike)) promptBuilder.AppendLine($"Tránh: {profile.Dislike}");
					if (!string.IsNullOrWhiteSpace(profile.Allergy)) promptBuilder.AppendLine($"Dị ứng: {profile.Allergy}");
					if (!string.IsNullOrWhiteSpace(profile.Diet)) promptBuilder.AppendLine($"Chế độ ăn: {profile.Diet}");
				}
			}
			var prompt = promptBuilder
				.AppendLine("Danh sách (id\tname):")
				.AppendLine(optionsList)
				.ToString();

			var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
							new { text = prompt }
                        }
                    }
                }
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

			var url = $"https://generativelanguage.googleapis.com/v1/models/{_model}:generateContent?key={_apiKey}";

			var response = await _httpClient.PostAsync(url, content);
			var responseText = await response.Content.ReadAsStringAsync();

			if (!response.IsSuccessStatusCode)
				return StatusCode((int)response.StatusCode, responseText);

			var jsonResult = JObject.Parse(responseText);
			var text = jsonResult["candidates"]?[0]?["content"]?["parts"]?[0]?["text"]?.ToString();
			List<int> dishIds = new List<int>();
			try
			{
				var parsed = JObject.Parse(text ?? "{}");
				var idsArray = parsed["ids"]?.ToObject<int[]>();
				if (idsArray != null && idsArray.Length > 0)
				{
					dishIds = idsArray.ToList();
				}
			}
			catch
			{
			}

			if (dishIds.Count == 0)
			{
				dishIds = limited.Take(Math.Min(5, limited.Count)).Select(x => x.DishId).ToList();
			}

			var selectedDishes = limited.Where(x => dishIds.Contains(x.DishId)).ToList();
			if (selectedDishes.Count == 0)
			{
				selectedDishes = limited.Take(Math.Min(5, limited.Count)).ToList();
			}

			var suggestions = new List<object>();
			foreach (var selected in selectedDishes)
			{
				var fullDish = dishes.FirstOrDefault(d => d.DishId == selected.DishId);
				if (fullDish != null)
				{
					var restaurant = await _unitOfWork.RestaurantRepository.GetByIdAsync(fullDish.ResId);
					string distanceDisplay = null;
					string dishImage = ImageUrlHelper.ResolveImageUrl(fullDish, _configuration, nameof(EatIT.Core.Entities.Dishes.DishImage));
					string resName = null;
					string resAddress = null;

					if (restaurant != null)
					{
						resName = restaurant.ResName;
						resAddress = restaurant.ResAddress;
						if (lat.HasValue && lng.HasValue)
						{
							var km = DistKm(lat.Value, lng.Value, restaurant.Latitude, restaurant.Longitude);
							distanceDisplay = Locations.FormatDistance(km);
						}
					}

					suggestions.Add(new
					{
						dishId = selected.DishId,
						dishName = selected.DishName,
						dishImage = dishImage,
						resName = resName,
						resAddress = resAddress,
						distanceDisplay = distanceDisplay
					});
				}
			}

			return Ok(new { suggestions = suggestions });
        }
    }
}
