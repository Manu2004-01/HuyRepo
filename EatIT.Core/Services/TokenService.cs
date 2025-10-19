using EatIT.Core.Entities;
using EatIT.Core.Interface;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;


namespace EatIT.Core.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _config;
        public TokenService(IConfiguration config) => _config = config;

        public string CreateToken(Users users, string roleName)
        {
            var jwt = _config.GetSection("Jwt");
            var keyBytes = Encoding.UTF8.GetBytes(jwt["Key"] ?? throw new ArgumentNullException("Jwt:Key"));
            var key = new SymmetricSecurityKey(keyBytes);
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, users.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, users.Email ?? string.Empty),
                new Claim(ClaimTypes.Name, users.UserName ?? string.Empty),
                new Claim(ClaimTypes.Role, roleName ?? string.Empty)
            };

            var token = new JwtSecurityToken(
                issuer: jwt["Issuer"],
                audience: jwt["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(int.Parse(jwt["ExpiresMinutes"] ?? "60")),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
