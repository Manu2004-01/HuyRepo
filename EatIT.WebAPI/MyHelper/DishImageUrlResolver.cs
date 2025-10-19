using AutoMapper;
using EatIT.Core.DTOs;
using EatIT.Core.Entities;

namespace EatIT.WebAPI.MyHelper
{
    public class DishImageUrlResolver : IValueResolver<Dishes, DishDTO, string>
    {
        private readonly IConfiguration _configuration;

        public DishImageUrlResolver(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string Resolve(Dishes source, DishDTO destination, string destMember, ResolutionContext context)
        {
            return ImageUrlHelper.ResolveImageUrl(source, _configuration, nameof(Dishes.DishImage));
        }
    }
}
