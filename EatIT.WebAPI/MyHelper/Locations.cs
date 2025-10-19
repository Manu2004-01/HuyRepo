using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace EatIT.WebAPI.MyHelper
{
    public class Locations
    {
        public static int GetCurrentUserId(ClaimsPrincipal user)
        {
            var userIdClaim = user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
            {
                userIdClaim = user.FindFirst("sub")?.Value;
            }

            if (string.IsNullOrEmpty(userIdClaim))
            {
                userIdClaim = user.FindFirst("UserId")?.Value;
            }

            return int.TryParse(userIdClaim, out var userId) ? userId : 0;
        }

        public static string FormatDistance(double distanceKm)
        {
            if (distanceKm < 1)
                return $"{(distanceKm * 1000):0} m";
            else
                return $"{distanceKm:0.1} km";
        }
    }
}
