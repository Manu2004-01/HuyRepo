using AutoMapper;
using EatIT.Core.DTOs;
using EatIT.Core.Entities;
using EatIT.Infrastructure.Data.DTOs;
using EatIT.WebAPI.MyHelper;

namespace EatIT.WebAPI.Models
{
    public class MappingDish : Profile
    {
        public MappingDish() 
        {
            CreateMap<Dishes, DishDTO>()
                .ForMember(d => d.Id, o => o.MapFrom(s => s.DishId))
                .ForMember(d => d.DishImage, o => o.MapFrom<DishImageUrlResolver>())
                .ForMember(d => d.RestaurantImg, o => o.MapFrom(s => s.Restaurant != null ? s.Restaurant.RestaurantImg : null))
                .ForMember(d => d.ResName, o => o.MapFrom(s => s.Restaurant != null ? s.Restaurant.ResName : null))
                .ForMember(d => d.ResAddress, o => o.MapFrom(s => s.Restaurant != null ? s.Restaurant.ResAddress : null))
                .ForMember(d => d.ResPhoneNumber, o => o.MapFrom(s => s.Restaurant != null ? s.Restaurant.ResPhoneNumber : null))
                .ForMember(d => d.OpeningHours, o => o.MapFrom(s => s.Restaurant != null ? s.Restaurant.OpeningHours : null))
                .ReverseMap();

            CreateMap<Dishes, DishBasicDTO>()
                .ForMember(d => d.Id, o => o.MapFrom(s => s.DishId))
                .ForMember(d => d.DishImage, o => o.MapFrom<DishBasicImageUrlResolver>());

            CreateMap<CreateDishDTO, Dishes>()
                .ForMember(d => d.ResId, o => o.MapFrom(s => s.restaurantid))
                .ForMember(d => d.DishImage, o => o.Ignore())
                .ReverseMap();

            CreateMap<UpdateDishDTO, Dishes>()
                .ForMember(d => d.ResId, d => d.Ignore())
                .ForMember(d => d.DishImage, d => d.Ignore())
                .ForMember(d => d.CreateAt, o => o.Ignore())
                .ForMember(d => d.UpdateAt, o => o.Ignore());
        }
    }
}
