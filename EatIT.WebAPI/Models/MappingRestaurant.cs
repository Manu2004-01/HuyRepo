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
            CreateMap<Restaurants, RestaurantDTO>()
                .ForMember(d => d.TagName, o => o.MapFrom(s => s.Tag.TagName))
                .ForMember(d => d.RestaurantImg, o => o.MapFrom<RestaurantImageUrlResolver>())
                .ReverseMap();

            CreateMap<CreateRestaurantDTO, Restaurants>()
                .ForMember(d => d.TagId, o => o.MapFrom(s => s.tagid))
                .ForMember(d => d.RestaurantImg, o => o.Ignore())
                .ReverseMap();

            CreateMap<UpdateRestaurantDTO, Restaurants>()
                .ForMember(d => d.TagId, o => o.Ignore())
                .ForMember(d => d.RestaurantImg, o => o.Ignore());
        }
    }
}
