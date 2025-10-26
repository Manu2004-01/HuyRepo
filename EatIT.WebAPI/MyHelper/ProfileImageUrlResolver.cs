using AutoMapper;
using EatIT.Core.Entities;
using EatIT.Infrastructure.Data.DTOs;

namespace EatIT.WebAPI.MyHelper
{
    public class ProfileImageUrlResolver : IValueResolver<Users, UserProfileDTO, string>
    {
        private readonly IConfiguration _configuration;
        public ProfileImageUrlResolver(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string Resolve(Users source, UserProfileDTO destination, string destMember, ResolutionContext context)
        {
            return ImageUrlHelper.ResolveImageUrl(source, _configuration, nameof(Users.UserImg));
        }
    }
}
