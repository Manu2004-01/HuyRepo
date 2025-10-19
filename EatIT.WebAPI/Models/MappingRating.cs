using AutoMapper;
using EatIT.Core.DTOs;
using EatIT.Core.Entities;

namespace EatIT.WebAPI.Models
{
    public class MappingRating : Profile
    {
        public MappingRating() 
        {
            CreateMap<Rating, RatingDTO>()
                .ForMember(d => d.UserName, o => o.MapFrom(s => s.User.UserName))
                .ForMember(d => d.RestaurantName, o => o.MapFrom(s => s.Restaurant.ResName))
                .ReverseMap();

            CreateMap<CreateRatingDTO, Rating>()
                .ForMember(d => d.UserId, o => o.MapFrom(s => s.userid))
                .ForMember(d => d.RestaurantId, o => o.MapFrom(s => s.restaurantid))
                .ReverseMap();

            CreateMap<UpdateRatingDTO, Rating>()
                .ForMember(d => d.UserId, o => o.Ignore())
                .ForMember(d => d.RestaurantId, o => o.Ignore())
                .ForMember(d => d.CreateAt, o => o.Ignore());
        }
    }
}
