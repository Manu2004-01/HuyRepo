using AutoMapper;
using EatIT.Core.DTOs;
using EatIT.Core.Entities;
using EatIT.WebAPI.MyHelper;
using System;

namespace EatIT.WebAPI.Models
{
    public class MappingFavorite : Profile
    {
        public MappingFavorite()
        {
            // Map Favorites -> FavoriteDTO (chỉ có Id, DishName, RestaurantName, DishImg)
            CreateMap<Favorites, FavoriteDTO>()
                .ForMember(d => d.Id, o => o.MapFrom(s => s.FavorId))
                .ForMember(d => d.DishName, o => o.MapFrom(s => s.Dish != null ? s.Dish.DishName : null))
                .ForMember(d => d.RestaurantName, o => o.MapFrom(s => s.Restaurant != null ? s.Restaurant.ResName : null))
                .ForMember(d => d.DishImg, o => o.MapFrom<FavoriteDishImageUrlResolver>())
                .ReverseMap();

            // Map Favorites -> FavoriteByIdDTO (có đầy đủ thông tin nhà hàng)
            CreateMap<Favorites, FavoriteByIdDTO>()
                .ForMember(d => d.Id, o => o.MapFrom(s => s.FavorId))
                .ForMember(d => d.DishName, o => o.MapFrom(s => s.Dish != null ? s.Dish.DishName : null))
                .ForMember(d => d.RestaurantName, o => o.MapFrom(s => s.Restaurant != null ? s.Restaurant.ResName : null))
                .ForMember(d => d.RestaurantImg, o => o.MapFrom<FavoriteByIdRestaurantImageUrlResolver>())
                .ForMember(d => d.ResAddress, o => o.MapFrom(s => s.Restaurant != null ? s.Restaurant.ResAddress : null))
                .ForMember(d => d.StarRating, o => o.MapFrom(s => s.Restaurant != null ? s.Restaurant.StarRating : (double?)null))
                .ForMember(d => d.ResPhoneNumber, o => o.MapFrom(s => s.Restaurant != null ? s.Restaurant.ResPhoneNumber : null))
                .ForMember(d => d.OpeningHours, o => o.MapFrom(s => s.Restaurant != null ? s.Restaurant.OpeningHours : null))
                .ReverseMap();

            CreateMap<CreateFavoriteDTO, Favorites>()
                .ForMember(d => d.DishId, o => o.MapFrom(s => s.dishid))
                .ForMember(d => d.UserId, o => o.MapFrom(s => s.userid))
                .ForMember(d => d.RestaurantId, o => o.MapFrom(s => s.restaurantid.HasValue ? s.restaurantid.Value : 0))
                .ForMember(d => d.FavorId, o => o.Ignore())
                .ReverseMap();

            CreateMap<UpdateFavoriteDTO, Favorites>()
                .ForMember(d => d.DishId, o => o.MapFrom(s => s.dishid))
                .ForMember(d => d.UserId, o => o.MapFrom(s => s.userid))
                .ForMember(d => d.RestaurantId, o => o.MapFrom(s => s.restaurantid ?? 0));
        }
    }
}