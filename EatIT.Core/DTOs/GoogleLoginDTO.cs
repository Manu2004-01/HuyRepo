using System.ComponentModel.DataAnnotations;

namespace EatIT.Core.DTOs
{
    public class GoogleLoginDTO
    {
        [Required]
        public string GoogleId { get; set; } = "";
        
        [Required]
        [EmailAddress]
        public string Email { get; set; } = "";
        
        public string? Name { get; set; }
        
        public string? Picture { get; set; }
    }
}
