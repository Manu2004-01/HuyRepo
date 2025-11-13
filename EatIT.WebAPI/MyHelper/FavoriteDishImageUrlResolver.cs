using AutoMapper;
using EatIT.Core.DTOs;
using EatIT.Core.Entities;

namespace EatIT.WebAPI.MyHelper
{
    public class FavoriteDishImageUrlResolver : IValueResolver<Favorites, FavoriteDTO, string>
    {
        private readonly IConfiguration _configuration;

        public FavoriteDishImageUrlResolver(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string Resolve(Favorites source, FavoriteDTO destination, string destMember, ResolutionContext context)
        {
            if (source?.Dish == null) return null;
            return ImageUrlHelper.ResolveImageUrl(source.Dish, _configuration, nameof(Dishes.DishImage));
        }
    }
}

