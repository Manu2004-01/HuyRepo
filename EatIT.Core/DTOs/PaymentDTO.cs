namespace EatIT.Core.DTOs
{
    public class CreatePaymentDTO
    {
        public string? ReturnUrl { get; set; }
        public string? CancelUrl { get; set; }
    }

    public class PaymentHistoryDTO
    {
        public int PaymentId { get; set; }
        public long OrderCode { get; set; }
        public int Amount { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string PaymentType { get; set; } = string.Empty;
        public DateTime? PremiumExpiryDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? PaidAt { get; set; }
    }

    public class PaymentResponseDTO
    {
        public string CheckoutUrl { get; set; } = string.Empty;
        public string QrCode { get; set; } = string.Empty;
        public long OrderCode { get; set; }
    }
}

