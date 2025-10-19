using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EatIT.Core.Entities
{
    public class Users : BasicEntity<int>
    {
        //public int UserId { get; set; }
        public int RoleId { get; set; }
        public string? UserImg { get; set; }
        public string UserName { get; set; } = "";
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
        public string? PhoneNumber { get; set;}
        public string? UserAddress { get; set; }
        public string? GoogleId { get; set; }
        public string? ResetPasswordToken { get; set; }
        public DateTime? ResetPasswordTokenExpiry { get; set; }
        public float? UserLatitude { get; set; }
        public float? UserLongitude { get; set; }
        public DateTime? LastLocationUpdate { get; set; }
        public DateTime CreateAt { get; set; }
        public DateTime UpdateAt { get; set; }
        public bool IsActive { get; set; }
        public virtual UserRole Role { get; set; }
        public ICollection<Favorites> Favorites { get; set; }
        public ICollection<Rating> Ratings { get; set; }
        public ICollection<Restaurants> Restaurants { get; set; }
    }
}
