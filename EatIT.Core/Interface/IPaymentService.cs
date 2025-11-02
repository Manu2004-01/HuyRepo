using EatIT.Core.DTOs;

namespace EatIT.Core.Interface
{
    public interface IPaymentService
    {
        Task<PaymentResponseDTO> CreatePaymentLinkAsync(int userId, CreatePaymentDTO dto);
        Task<bool> VerifyWebhookAsync(string webhookData);
        Task<bool> HasActivePremiumAsync(int userId);
        Task<bool> VerifyAndUpdatePaymentStatusAsync(long orderCode);
    }
}

