using AutoMapper;
using EatIT.Core.DTOs;
using EatIT.Core.Entities;

namespace EatIT.WebAPI.MyHelper
{
    public class TagDetailImageUrlResolver : IValueResolver<Tags, TagDTO.TagByIdDTO, string>
    {
        private readonly IConfiguration _configuration;

        public TagDetailImageUrlResolver(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string Resolve(Tags source, TagDTO.TagByIdDTO destination, string destMember, ResolutionContext context)
        {
            return ImageUrlHelper.ResolveImageUrl(source, _configuration, nameof(Tags.TagImg));
        }
    }
}


