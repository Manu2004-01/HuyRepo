using EatIT.Core.Entities;
using EatIT.Core.Interface;
using EatIT.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EatIT.Infrastructure.Repository
{
    public class PaymentRepository : GenericRepository<Payment>, IPaymentRepository
    {
        private readonly ApplicationDBContext _context;

        public PaymentRepository(ApplicationDBContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Payment?> GetByOrderCodeAsync(long orderCode)
        {
            return await _context.Payments
                .FirstOrDefaultAsync(p => p.OrderCode == orderCode);
        }

        public async Task<IEnumerable<Payment>> GetByUserIdAsync(int userId)
        {
            return await _context.Payments
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<Payment?> GetActivePremiumByUserIdAsync(int userId)
        {
            return await _context.Payments
                .Where(p => p.UserId == userId 
                    && p.PaymentType == "Premium" 
                    && p.Status == "PAID"
                    && (p.PremiumExpiryDate == null || p.PremiumExpiryDate > DateTime.UtcNow))
                .OrderByDescending(p => p.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> UpdatePaymentStatusAsync(long orderCode, string status, DateTime? paidAt = null)
        {
            var payment = await GetByOrderCodeAsync(orderCode);
            if (payment == null) return false;

            payment.Status = status;
            if (paidAt.HasValue)
            {
                payment.PaidAt = paidAt.Value;
            }
            else if (status == "PAID")
            {
                payment.PaidAt = DateTime.UtcNow;
                if (payment.PaymentType == "Premium")
                {
                    payment.PremiumExpiryDate = DateTime.UtcNow.AddMonths(1);
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }
    }
}

