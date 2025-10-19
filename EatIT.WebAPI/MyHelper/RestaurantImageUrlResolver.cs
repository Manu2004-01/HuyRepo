using AutoMapper;
using EatIT.Core.DTOs;
using EatIT.Core.Entities;

namespace EatIT.WebAPI.MyHelper
{
    public class RestaurantImageUrlResolver : IValueResolver<Restaurants, RestaurantDTO, string>
    {
        private readonly IConfiguration _configuration;

        public RestaurantImageUrlResolver(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string Resolve(Restaurants source, RestaurantDTO destination, string destMember, ResolutionContext context)
        {
            return ImageUrlHelper.ResolveImageUrl(source, _configuration, nameof(Restaurants.RestaurantImg));
        }
    }
}
