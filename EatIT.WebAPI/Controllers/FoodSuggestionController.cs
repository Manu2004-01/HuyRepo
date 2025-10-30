using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Text.Json;
using EatIT.Core.Interface;
using EatIT.Core.Sharing;
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
		private class DishOption { public int DishId { get; set; } public string DishName { get; set; } }

		public FoodSuggestionController(IHttpClientFactory httpClientFactory, IConfiguration config, IUnitOfWork unitOfWork)
        {
            _httpClient = httpClientFactory.CreateClient();
			_apiKey = config["Gemini:ApiKey"];
			_model = config["Gemini:Model"] ?? "gemini-2.5-flash";
			_unitOfWork = unitOfWork;
        }

		[Authorize]
		[HttpPost]
		public async Task<IActionResult> GetSuggestion()
        {
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

					var ordered = restaurants
						.OrderBy(r => DistKm(lat.Value, lng.Value, r.Latitude, r.Longitude))
						.ToList();

					List<DishOption> BuildLimitedForRes(int resId) => dishes
						.Where(d => d.ResId == resId)
						.Take(50)
						.Select(d => new DishOption { DishId = d.DishId, DishName = d.DishName })
						.ToList();

					limited = new List<DishOption>();
					foreach (var res in ordered)
					{
						limited = BuildLimitedForRes(res.ResId);
						if (limited.Count > 0) break;
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

			if (limited.Count == 0)
			{
				string fallbackPrompt = "Hãy gợi ý cho tôi một món ăn ở quán gần tôi nhất";

				var fallbackBody = new
				{
					contents = new[]
					{
						new
						{
							parts = new[]
							{
								new { text = fallbackPrompt }
							}
						}
					}
				};

				var fallbackJson = JsonSerializer.Serialize(fallbackBody);
				var fallbackContent = new StringContent(fallbackJson, Encoding.UTF8, "application/json");
				var fallbackUrl = $"https://generativelanguage.googleapis.com/v1/models/{_model}:generateContent?key={_apiKey}";
				var fallbackResponse = await _httpClient.PostAsync(fallbackUrl, fallbackContent);
				var fallbackText = await fallbackResponse.Content.ReadAsStringAsync();
				if (!fallbackResponse.IsSuccessStatusCode)
					return StatusCode((int)fallbackResponse.StatusCode, fallbackText);
				var fallbackJsonResult = JObject.Parse(fallbackText);
                string fallbackSuggestion = fallbackJsonResult["candidates"]?[0]?["content"]?["parts"]?[0]?["text"]?.ToString()
                                ?? "Không thể gợi ý món ăn lúc này.";
                return Ok(new { suggestion = fallbackSuggestion, restaurantImg = (string)null, resName = (string)null, resAddress = (string)null, distanceDisplay = (string)null });
			}

			var optionsList = string.Join("\n", limited.Select(x => $"{x.DishId}\t{x.DishName}"));
			var promptBuilder = new StringBuilder()
				.AppendLine("Hãy chọn duy nhất 1 món trong danh sách sau.")
				.AppendLine("Trả về đúng JSON với dạng {\"id\": <DishId>} không kèm giải thích.")
				.AppendLine("Ưu tiên tuân thủ sở thích và hạn chế của người dùng nếu có.");
			if (profile != null)
			{
				if (profile.IsVegetarian) promptBuilder.AppendLine("Người dùng ăn chay.");
				if (!string.IsNullOrWhiteSpace(profile.Preference)) promptBuilder.AppendLine($"Ưu tiên: {profile.Preference}");
				if (!string.IsNullOrWhiteSpace(profile.Dislike)) promptBuilder.AppendLine($"Tránh: {profile.Dislike}");
				if (!string.IsNullOrWhiteSpace(profile.Allergy)) promptBuilder.AppendLine($"Dị ứng: {profile.Allergy}");
				if (!string.IsNullOrWhiteSpace(profile.Diet)) promptBuilder.AppendLine($"Chế độ ăn: {profile.Diet}");
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
			int dishId;
			try
			{
				var parsed = JObject.Parse(text ?? "{}");
				dishId = parsed["id"]?.ToObject<int?>() ?? 0;
			}
			catch
			{
				dishId = 0;
			}

			var selected = dishId != 0 ? limited.FirstOrDefault(x => x.DishId == dishId) : limited.First();

			string distanceDisplay = null;
			string resImg = null;
			string resName = null;
			string resAddress = null;

			if (selected != null)
			{
				var fullDish = dishes.FirstOrDefault(d => d.DishId == selected.DishId);
				if (fullDish != null)
				{
					var restaurant = await _unitOfWork.RestaurantRepository.GetByIdAsync(fullDish.ResId);
					if (restaurant != null)
					{
						resImg = restaurant.RestaurantImg;
						resName = restaurant.ResName;
						resAddress = restaurant.ResAddress;
						if (lat.HasValue && lng.HasValue)
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

							var km = DistKm(lat.Value, lng.Value, restaurant.Latitude, restaurant.Longitude);
							distanceDisplay = Locations.FormatDistance(km);
						}
					}
				}
			}

			return Ok(new { suggestion = selected?.DishName ?? "Không thể gợi ý món ăn lúc này.", restaurantImg = resImg, resName, resAddress, distanceDisplay });
        }
    }
}
