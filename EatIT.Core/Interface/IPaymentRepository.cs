using EatIT.Core.Entities;

namespace EatIT.Core.Interface
{
    public interface IPaymentRepository : IGenericRepository<Payment>
    {
        Task<Payment?> GetByOrderCodeAsync(long orderCode);
        Task<IEnumerable<Payment>> GetByUserIdAsync(int userId);
        Task<Payment?> GetActivePremiumByUserIdAsync(int userId);
        Task<bool> UpdatePaymentStatusAsync(long orderCode, string status, DateTime? paidAt = null);
    }
}

