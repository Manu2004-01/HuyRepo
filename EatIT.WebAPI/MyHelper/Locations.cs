using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Globalization;

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
                userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
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
                return string.Format(CultureInfo.InvariantCulture, "{0:0} m", distanceKm * 1000);
            else
                return string.Format(CultureInfo.InvariantCulture, "{0:0.00} km", distanceKm);
        }
    }
}
