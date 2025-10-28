using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EatIT.Core.Entities
{
    public class Users 
    {
        [Key]
        public int UserId { get; set; }
        public int RoleId { get; set; }
        public string? UserImg { get; set; }
        public string UserName { get; set; } = "";
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
        public string? PhoneNumber { get; set;}
        public string? UserAddress { get; set; }

        public string? Preference { get; set; } //Mon an ua thich
        public string? Dislike { get; set; } //Mon k thich or k an dc
        public string? Allergy { get; set; } //Mon di ung
        public string? Diet { get; set; } //Che do an


        public string? ResetPasswordToken { get; set; }
        public DateTime? ResetPasswordTokenExpiry { get; set; }
        public double? UserLatitude { get; set; }
        public double? UserLongitude { get; set; }
        public DateTime? LastLocationUpdate { get; set; }
        public DateTime CreateAt { get; set; }
        public DateTime UpdateAt { get; set; }
        public bool IsActive { get; set; }
        public virtual UserRole Role { get; set; }
        public ICollection<Favorites> Favorites { get; set; }
        public ICollection<Rating> Ratings { get; set; }
        
    }
}
