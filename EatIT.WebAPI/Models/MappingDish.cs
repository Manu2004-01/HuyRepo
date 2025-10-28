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
                .ForMember(d => d.RestaurantName, o => o.MapFrom(s => s.Restaurant.ResName))
                .ForMember(d => d.DishImage, o => o.MapFrom<DishImageUrlResolver>())
                .ReverseMap();

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
