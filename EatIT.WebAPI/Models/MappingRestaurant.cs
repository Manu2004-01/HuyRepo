using AutoMapper;
using EatIT.Core.DTOs;
using EatIT.Core.Entities;
using EatIT.Infrastructure.Data.DTOs;
using EatIT.WebAPI.MyHelper;

namespace EatIT.WebAPI.Models
{
    public class MappingRestaurant : Profile
    {
        public MappingRestaurant() 
        {
            // Map Restaurants -> RestaurantDTO
            CreateMap<Restaurants, RestaurantDTO>()
                .ForMember(d => d.Id, o => o.MapFrom(s => s.ResId))
                .ForMember(d => d.TagName, o => o.MapFrom(s => s.Tag.TagName))
                .ForMember(d => d.RestaurantImg, o => o.MapFrom<RestaurantImageUrlResolver>());

            // Map CreateRestaurantDTO -> Restaurants
            CreateMap<CreateRestaurantDTO, Restaurants>()
                .ForMember(d => d.ResId, o => o.Ignore())
                .ForMember(d => d.TagId, o => o.MapFrom(s => s.tagid))
                .ForMember(d => d.RestaurantImg, o => o.Ignore())
                .ForMember(d => d.Tag, o => o.Ignore())
                .ForMember(d => d.Ratings, o => o.Ignore())
                .ForMember(d => d.Dishes, o => o.Ignore())
                .ForMember(d => d.Favorites, o => o.Ignore());

            // Map UpdateRestaurantDTO -> Restaurants
            CreateMap<UpdateRestaurantDTO, Restaurants>()
                .ForMember(d => d.ResId, o => o.Ignore())
                .ForMember(d => d.TagId, o => o.Ignore())
                .ForMember(d => d.RestaurantImg, o => o.Ignore())
                .ForMember(d => d.Tag, o => o.Ignore())
                .ForMember(d => d.Ratings, o => o.Ignore())
                .ForMember(d => d.Dishes, o => o.Ignore())
                .ForMember(d => d.Favorites, o => o.Ignore());
        }
    }
}
