using AutoMapper;
using EatIT.Core.DTOs;
using EatIT.Core.Entities;

namespace EatIT.WebAPI.MyHelper
{
    public class RestaurantDetailImageUrlResolver : IValueResolver<Restaurants, RestaurantDetailDTO, string>
    {
        private readonly IConfiguration _configuration;

        public RestaurantDetailImageUrlResolver(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string Resolve(Restaurants source, RestaurantDetailDTO destination, string destMember, ResolutionContext context)
        {
            return ImageUrlHelper.ResolveImageUrl(source, _configuration, nameof(Restaurants.RestaurantImg));
        }
    }
}

