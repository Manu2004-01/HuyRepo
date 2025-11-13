using AutoMapper;
using EatIT.Core.DTOs;
using EatIT.Core.Entities;

namespace EatIT.WebAPI.MyHelper
{
    public class FavoriteByIdRestaurantImageUrlResolver : IValueResolver<Favorites, FavoriteByIdDTO, string>
    {
        private readonly IConfiguration _configuration;

        public FavoriteByIdRestaurantImageUrlResolver(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string Resolve(Favorites source, FavoriteByIdDTO destination, string destMember, ResolutionContext context)
        {
            if (source?.Restaurant == null) return null;
            return ImageUrlHelper.ResolveImageUrl(source.Restaurant, _configuration, nameof(Restaurants.RestaurantImg));
        }
    }
}

