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
            CreateMap<ListUserRoleDTO, UserRole>().ReverseMap();
        }
    }
}
