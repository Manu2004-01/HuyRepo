using EatIT.Core.DTOs;
using EatIT.Core.Interface;
using EatIT.WebAPI.Errors;
using EatIT.WebAPI.MyHelper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace EatIT.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly IPaymentRepository _paymentRepository;

        public PaymentController(IPaymentService paymentService, IPaymentRepository paymentRepository)
        {
            _paymentService = paymentService;
            _paymentRepository = paymentRepository;
        }

        [Authorize]
        [HttpPost("premium")]
        public async Task<IActionResult> CreatePremiumPayment([FromQuery] CreatePaymentDTO? dto = null)
        {
            try
            {
                var currentUserId = Locations.GetCurrentUserId(User);
                if (currentUserId <= 0)
                    return Unauthorized(new BaseCommentResponse(401, "Token không hợp lệ"));

                dto ??= new CreatePaymentDTO();
                var result = await _paymentService.CreatePaymentLinkAsync(currentUserId, dto);

                return Ok(new
                {
                    checkoutUrl = result.CheckoutUrl,
                    qrCode = result.QrCode,
                    orderCode = result.OrderCode
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseCommentResponse(500, $"Lỗi khi tạo thanh toán: {ex.Message}"));
            }
        }

        [HttpPost("webhook")]
        public async Task<IActionResult> Webhook()
        {
            try
            {
                Request.EnableBuffering();
                Request.Body.Position = 0;
                
                using var reader = new StreamReader(Request.Body, Encoding.UTF8, leaveOpen: true);
                var webhookData = await reader.ReadToEndAsync();
                
                Request.Body.Position = 0;

                // Log webhook data for debugging (remove in production or use proper logging)
                Console.WriteLine($"[Webhook] Received webhook data: {webhookData}");

                var success = await _paymentService.VerifyWebhookAsync(webhookData);

                Console.WriteLine($"[Webhook] Verification result: {success}");

                // Always return success to prevent PayOS retries
                return Ok(new { code = 0, desc = "success" });
            }
            catch (Exception ex)
            {
                // Log exception but still return success to prevent retries
                Console.WriteLine($"[Webhook] Exception: {ex.Message}");
                Console.WriteLine($"[Webhook] Stack trace: {ex.StackTrace}");
                return Ok(new { code = 0, desc = "success" });
            }
        }

        [Authorize]
        [HttpGet("history")]
        public async Task<IActionResult> GetPaymentHistory()
        {
            try
            {
                var currentUserId = Locations.GetCurrentUserId(User);
                if (currentUserId <= 0)
                    return Unauthorized(new BaseCommentResponse(401, "Token không hợp lệ"));

                var payments = await _paymentRepository.GetByUserIdAsync(currentUserId);

                var history = payments.Select(p => new PaymentHistoryDTO
                {
                    PaymentId = p.PaymentId,
                    UserId = p.UserId,
                    OrderCode = p.OrderCode,
                    Amount = p.Amount,
                    Description = p.Description,
                    Status = p.Status,
                    PaymentType = p.PaymentType,
                    PremiumExpiryDate = p.PremiumExpiryDate,
                    CreatedAt = p.CreatedAt,
                    PaidAt = p.PaidAt
                }).ToList();

                return Ok(new { payments = history });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseCommentResponse(500, $"Lỗi khi lấy lịch sử thanh toán: {ex.Message}"));
            }
        }

        [Authorize]
        [HttpGet("payments")]
        public async Task<IActionResult> GetAllPayments()
        {
            try
            {
                var payments = await _paymentRepository.GetAllAsync();

                var paymentList = payments.Select(p => new PaymentHistoryDTO
                {
                    PaymentId = p.PaymentId,
                    UserId = p.UserId,
                    OrderCode = p.OrderCode,
                    Amount = p.Amount,
                    Description = p.Description,
                    Status = p.Status,
                    PaymentType = p.PaymentType,
                    PremiumExpiryDate = p.PremiumExpiryDate,
                    CreatedAt = p.CreatedAt,
                    PaidAt = p.PaidAt
                }).OrderByDescending(p => p.CreatedAt).ToList();

                var totalItems = paymentList.Count;

                return Ok(new { totalItems, payments = paymentList });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseCommentResponse(500, $"Lỗi khi lấy danh sách thanh toán: {ex.Message}"));
            }
        }

        [Authorize]
        [HttpGet("premium-status")]
        public async Task<IActionResult> GetPremiumStatus()
        {
            try
            {
                var currentUserId = Locations.GetCurrentUserId(User);
                if (currentUserId <= 0)
                    return Unauthorized(new BaseCommentResponse(401, "Token không hợp lệ"));

                var activePremium = await _paymentRepository.GetActivePremiumByUserIdAsync(currentUserId);

                if (activePremium == null)
                {
                    return Ok(new
                    {
                        hasPremium = false,
                        expiryDate = (DateTime?)null
                    });
                }

                return Ok(new
                {
                    hasPremium = true,
                    expiryDate = activePremium.PremiumExpiryDate,
                    orderCode = activePremium.OrderCode,
                    paidAt = activePremium.PaidAt
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseCommentResponse(500, $"Lỗi khi kiểm tra Premium status: {ex.Message}"));
            }
        }

        [HttpGet("success")]
        public async Task<IActionResult> PaymentSuccess([FromQuery] string? orderCode = null)
        {
            try
            {
                if (string.IsNullOrEmpty(orderCode) || !long.TryParse(orderCode, out var parsedOrderCode))
                {
                    return Ok(new
                    {
                        success = false,
                        message = "Mã đơn hàng không hợp lệ.",
                        orderCode = orderCode
                    });
                }

                // Verify and update payment status from PayOS
                var paymentVerified = await _paymentService.VerifyAndUpdatePaymentStatusAsync(parsedOrderCode);

                if (paymentVerified)
                {
                    return Ok(new
                    {
                        success = true,
                        message = "Thanh toán thành công! Gói Premium đã được kích hoạt.",
                        orderCode = orderCode
                    });
                }
                else
                {
                    // Payment might still be pending or cancelled
                    var payment = await _paymentRepository.GetByOrderCodeAsync(parsedOrderCode);
                    if (payment != null)
                    {
                        return Ok(new
                        {
                            success = payment.Status == "PAID",
                            message = payment.Status == "PAID" 
                                ? "Thanh toán thành công! Gói Premium đã được kích hoạt."
                                : payment.Status == "CANCELLED"
                                ? "Thanh toán đã bị hủy."
                                : "Thanh toán đang được xử lý. Vui lòng chờ trong giây lát.",
                            orderCode = orderCode,
                            status = payment.Status
                        });
                    }
                    
                    return Ok(new
                    {
                        success = false,
                        message = "Không tìm thấy thông tin thanh toán.",
                        orderCode = orderCode
                    });
                }
            }
            catch (Exception ex)
            {
                return Ok(new
                {
                    success = false,
                    message = $"Lỗi khi kiểm tra trạng thái thanh toán: {ex.Message}",
                    orderCode = orderCode
                });
            }
        }

        [HttpGet("cancel")]
        public IActionResult PaymentCancel([FromQuery] string? orderCode = null)
        {
            return Ok(new
            {
                success = false,
                message = "Thanh toán đã bị hủy. Bạn có thể thử lại sau.",
                orderCode = orderCode
            });
        }
    }
}

