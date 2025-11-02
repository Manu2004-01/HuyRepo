using System;
using System.ComponentModel.DataAnnotations;

namespace EatIT.Core.Entities
{
    public class Payment
    {
        [Key]
        public int PaymentId { get; set; }
        public int UserId { get; set; }
        public long OrderCode { get; set; }
        public string PaymentLinkId { get; set; } = string.Empty;
        public int Amount { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string PaymentType { get; set; } = "Premium";
        public DateTime? PremiumExpiryDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? PaidAt { get; set; }
        public virtual Users User { get; set; }
    }
}

