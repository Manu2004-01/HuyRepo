using AutoMapper;
using EatIT.Core.Entities;
using EatIT.Infrastructure.Data.DTOs;
using EatIT.WebAPI.MyHelper;

namespace EatIT.WebAPI.Models
{
    public class MappingUser : Profile
    {
        public MappingUser() 
        {
            CreateMap<Users, UserDTO>()
                .ForMember(d => d.RoleName, o => o.MapFrom(s => s.Role.RoleName))
                .ForMember(d => d.UserImage, o => o.MapFrom<UserImageUrlResolver>())
                .ReverseMap();

            CreateMap<CreateUserDTO, Users>()
                .ForMember(d => d.RoleId, o => o.MapFrom(s => s.userroleid))
                .ForMember(d => d.IsActive, o => o.MapFrom(_ => true))
                .ForMember(d => d.UserImg, o=> o.Ignore())
                .ReverseMap();

            CreateMap<UpdateUserDTO, Users>()
                .ForMember(d => d.RoleId, o => o.Ignore())
                .ForMember(d => d.UserImg, o => o.Ignore())
                .ForMember(d => d.CreateAt, o => o.Ignore())
                .ForMember(d => d.UpdateAt, o => o.Ignore())
                .ForMember(d => d.IsActive, o => o.Ignore());

            CreateMap<UpdateUserProfileDTO, Users>()
                .ForMember(d => d.Id, o => o.Ignore())
                .ForMember(d => d.RoleId, o => o.Ignore())
                .ForMember(d => d.Email, o => o.Ignore())
                .ForMember(d => d.Password, o => o.Ignore())
                .ForMember(d => d.UserImg, o => o.Ignore())
                .ForMember(d => d.CreateAt, o => o.Ignore())
                .ForMember(d => d.UpdateAt, o => o.Ignore())
                .ForMember(d => d.IsActive, o => o.Ignore())
                .ForMember(d => d.Role, o => o.Ignore())
                .ForMember(d => d.Favorites, o => o.Ignore())
                .ForMember(d => d.Ratings, o => o.Ignore())
                .ForMember(d => d.Restaurants, o => o.Ignore());
        }
    }
}
