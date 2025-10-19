using AutoMapper;
using EatIT.Core.Entities;
using EatIT.Infrastructure.Data.DTOs;

namespace EatIT.WebAPI.MyHelper
{
    public class UserImageUrlResolver : IValueResolver<Users, UserDTO, string>
    {
        private readonly IConfiguration _configuration;

        public UserImageUrlResolver(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string Resolve(Users source, UserDTO destination, string destMember, ResolutionContext context)
        {
            return ImageUrlHelper.ResolveImageUrl(source, _configuration, nameof(Users.UserImg));
        }
    }
}
