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

        /// <summary>
        /// Lấy premium đang hoạt động của user.
        /// Premium được coi là active khi:
        /// - PaymentType == "Premium"
        /// - Status == "PAID"
        /// - PremiumExpiryDate != null và PremiumExpiryDate > DateTime.UtcNow (chưa hết hạn)
        /// Premium sẽ tự động hết hạn sau 1 tháng từ ngày thanh toán (PaidAt + 1 tháng)
        /// </summary>
        public async Task<Payment?> GetActivePremiumByUserIdAsync(int userId)
        {
            return await _context.Payments
                .Where(p => p.UserId == userId 
                    && p.PaymentType == "Premium" 
                    && p.Status == "PAID"
                    && p.PremiumExpiryDate != null
                    && p.PremiumExpiryDate > DateTime.UtcNow)
                .OrderByDescending(p => p.CreatedAt)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Cập nhật trạng thái thanh toán.
        /// Khi status = "PAID" và PaymentType = "Premium":
        /// - Set PremiumExpiryDate = PaidAt + 1 tháng
        /// - Premium sẽ tự động hết hạn sau 1 tháng, user sẽ không thể sử dụng premium features nữa
        /// </summary>
        public async Task<bool> UpdatePaymentStatusAsync(long orderCode, string status, DateTime? paidAt = null)
        {
            var payment = await GetByOrderCodeAsync(orderCode);
            if (payment == null) return false;

            payment.Status = status;
            
            if (status == "PAID")
            {
                // Set PaidAt từ tham số hoặc dùng thời gian hiện tại
                if (paidAt.HasValue)
                {
                    payment.PaidAt = paidAt.Value;
                    // Nếu là Premium, set PremiumExpiryDate = PaidAt + 1 tháng
                    // Sau 1 tháng, premium sẽ tự động hết hạn và không còn sử dụng được
                    if (payment.PaymentType == "Premium")
                    {
                        payment.PremiumExpiryDate = paidAt.Value.AddMonths(1);
                    }
                }
                else
                {
                    payment.PaidAt = DateTime.UtcNow;
                    // Nếu là Premium, set PremiumExpiryDate = DateTime.UtcNow + 1 tháng
                    // Sau 1 tháng, premium sẽ tự động hết hạn và không còn sử dụng được
                    if (payment.PaymentType == "Premium")
                    {
                        payment.PremiumExpiryDate = DateTime.UtcNow.AddMonths(1);
                    }
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }
    }
}

