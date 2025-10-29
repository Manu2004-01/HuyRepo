using AutoMapper;
using EatIT.Core.DTOs;
using EatIT.Core.Entities;

namespace EatIT.WebAPI.MyHelper
{
    public class DishBasicImageUrlResolver : IValueResolver<Dishes, DishBasicDTO, string>
    {
        private readonly IConfiguration _configuration;

        public DishBasicImageUrlResolver(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string Resolve(Dishes source, DishBasicDTO destination, string destMember, ResolutionContext context)
        {
            return ImageUrlHelper.ResolveImageUrl(source, _configuration, nameof(Dishes.DishImage));
        }
    }
}

