using AutoMapper;
using EatIT.Core.DTOs;
using EatIT.Core.Entities;
using EatIT.WebAPI.MyHelper;

namespace EatIT.WebAPI.Models
{
    public class MappingTag : Profile
    {
        public MappingTag() 
        {
            CreateMap<Tags, TagDTO>()
                .ForMember(t => t.TagID, t => t.MapFrom(x => x.TagId))
                .ForMember(t => t.TagImg, t => t.MapFrom<TagImageUrlResolver>())
                .ReverseMap();

            CreateMap<CreateTagDTO, Tags>()
                .ForMember(t => t.TagImg, t => t.Ignore()).ReverseMap();

            CreateMap<UpdateTagDTO, Tags>()
                .ForMember(t => t.TagImg, t => t.Ignore());
        }
    }
}
