using System.Security.Cryptography;

namespace EatIT.WebAPI.MyHelper
{
    public class ResetToken
    {
        // Helper method để tạo reset token
        public string GenerateResetToken()
        {
            using (var rng = RandomNumberGenerator.Create())
            {
                var bytes = new byte[32];
                rng.GetBytes(bytes);
                return Convert.ToBase64String(bytes);
            }
        }
    }
}
