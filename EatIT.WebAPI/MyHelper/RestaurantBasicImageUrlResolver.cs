using AutoMapper;
using EatIT.Core.DTOs;
using EatIT.Core.Entities;

namespace EatIT.WebAPI.MyHelper
{
    public class RestaurantBasicImageUrlResolver : IValueResolver<Restaurants, TagDTO.TagByIdRestaurantDTO, string>
    {
        private readonly IConfiguration _configuration;

        public RestaurantBasicImageUrlResolver(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string Resolve(Restaurants source, TagDTO.TagByIdRestaurantDTO destination, string destMember, ResolutionContext context)
        {
            return ImageUrlHelper.ResolveImageUrl(source, _configuration, nameof(Restaurants.RestaurantImg));
        }
    }
}


