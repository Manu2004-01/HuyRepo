using AutoMapper;
using EatIT.Core.DTOs;
using EatIT.Core.Entities;
using EatIT.Infrastructure.Data.DTOs;

namespace EatIT.WebAPI.MyHelper
{
    public class TagImageUrlResolver : IValueResolver<Tags, TagDTO, string>
    {
        private readonly IConfiguration _configuration;

        public TagImageUrlResolver(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string Resolve(Tags source, TagDTO destination, string destMember, ResolutionContext context)
        {
            return ImageUrlHelper.ResolveImageUrl(source, _configuration, nameof(Tags.TagImg));
        }
    }
}
