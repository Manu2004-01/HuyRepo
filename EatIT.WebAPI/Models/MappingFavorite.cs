using AutoMapper;
using EatIT.Core.DTOs;
using EatIT.Core.Entities;
using System;

namespace EatIT.WebAPI.Models
{
    public class MappingFavorite : Profile
    {
        public MappingFavorite()
        {
            CreateMap<Favorites, FavoriteDTO>()
                .ForMember(d => d.DishName, o => o.MapFrom(s => s.Dish != null ? s.Dish.DishName : null))
                .ForMember(d => d.UserName, o => o.MapFrom(s => s.User != null ? s.User.UserName : null))
                .ForMember(d => d.RestaurantName, o => o.MapFrom(s => s.Restaurant != null ? s.Restaurant.ResName : null))
                .ReverseMap();

            CreateMap<CreateFavoriteDTO, Favorites>()
                .ForMember(d => d.DishId, o => o.MapFrom(s => s.dishid))
                .ForMember(d => d.UserId, o => o.MapFrom(s => s.userid))
                .ForMember(d => d.RestaurantId, o => o.MapFrom(s => s.restaurantid))
                .ReverseMap();

            CreateMap<UpdateFavoriteDTO, Favorites>()
                .ForMember(d => d.DishId, o => o.Ignore())
                .ForMember(d => d.UserId, o => o.Ignore())
                .ForMember(d => d.RestaurantId, o => o.Ignore());
        }
    }
}