using AutoMapper;
using EatIT.Core.Entities;
using EatIT.Infrastructure.Data.DTOs;

namespace EatIT.WebAPI.Models
{
    public class MappingUserRole : Profile
    {
        public MappingUserRole() 
        {
            CreateMap<UserRoleDTO, UserRole>().ReverseMap();
            
            // Map UserRole -> ListUserRoleDTO
            CreateMap<UserRole, ListUserRoleDTO>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.RoleId));
            
            // Map ListUserRoleDTO -> UserRole (for reverse mapping)
            CreateMap<ListUserRoleDTO, UserRole>()
                .ForMember(dest => dest.RoleId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Users, opt => opt.Ignore());
        }
    }
}
