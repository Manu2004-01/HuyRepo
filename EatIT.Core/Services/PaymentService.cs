using EatIT.Core.DTOs;
using EatIT.Core.Interface;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Net.payOS.Types;
using PayOSClient = Net.payOS.PayOS;
using System.Net;

namespace EatIT.Core.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly PayOSClient _payOS;
        private readonly IPaymentRepository _paymentRepository;
        private readonly IConfiguration _configuration;

        public PaymentService(IConfiguration configuration, IPaymentRepository paymentRepository, IHostEnvironment environment)
        {
            _configuration = configuration;
            _paymentRepository = paymentRepository;
            
            // Ensure SSL certificate validation bypass is set before creating PayOS client
            if (environment.IsDevelopment())
            {
                ServicePointManager.ServerCertificateValidationCallback = 
                    (sender, certificate, chain, sslPolicyErrors) => true;
            }
            
            var clientId = _configuration["PayOS:ClientId"] ?? throw new InvalidOperationException("PayOS:ClientId not configured");
            var apiKey = _configuration["PayOS:ApiKey"] ?? throw new InvalidOperationException("PayOS:ApiKey not configured");
            var checksumKey = _configuration["PayOS:ChecksumKey"] ?? throw new InvalidOperationException("PayOS:ChecksumKey not configured");
            
            _payOS = new PayOSClient(clientId, apiKey, checksumKey);
        }

        public async Task<PaymentResponseDTO> CreatePaymentLinkAsync(int userId, CreatePaymentDTO dto)
        {
            var random = new Random();
            var orderCode = (long)(random.NextDouble() * (999999999999 - 100000000000) + 100000000000);
            
            var apiUrl = _configuration["API_url"] ?? "https://localhost:7091/";
            var returnUrl = dto.ReturnUrl ?? $"{apiUrl}payment/success";
            var cancelUrl = dto.CancelUrl ?? $"{apiUrl}payment/cancel";

            var paymentData = new PaymentData(
                orderCode: orderCode,
                amount: 29000,
                description: "Gói Premium EatIT - 1 tháng",
                items: new List<ItemData>
                {
                    new ItemData("Gói Premium EatIT", 1, 29000)
                },
                cancelUrl: cancelUrl,
                returnUrl: returnUrl
            );

            var result = await _payOS.createPaymentLink(paymentData);

            var payment = new EatIT.Core.Entities.Payment
            {
                UserId = userId,
                OrderCode = orderCode,
                PaymentLinkId = result.paymentLinkId,
                Amount = 29000,
                Description = "Gói Premium EatIT - 1 tháng",
                Status = result.status,
                PaymentType = "Premium",
                CreatedAt = DateTime.UtcNow
            };

            await _paymentRepository.AddAsync(payment);

            return new PaymentResponseDTO
            {
                CheckoutUrl = result.checkoutUrl,
                QrCode = result.qrCode,
                OrderCode = orderCode
            };
        }

        public async Task<bool> VerifyWebhookAsync(string webhookData)
        {
            try
            {
                var options = new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                var webhookType = System.Text.Json.JsonSerializer.Deserialize<WebhookType>(webhookData, options);
                if (webhookType == null || !webhookType.success) return false;

                var webhookDataVerified = _payOS.verifyPaymentWebhookData(webhookType);

                if (webhookDataVerified != null && webhookDataVerified.code == "00")
                {
                    DateTime paidDate = DateTime.UtcNow;
                    if (!string.IsNullOrEmpty(webhookDataVerified.transactionDateTime))
                    {
                        var dateFormats = new[] { 
                            "yyyy-MM-dd HH:mm:ss",
                            "yyyy-MM-ddTHH:mm:ss",
                            "yyyy-MM-ddTHH:mm:ssZ",
                            "yyyy-MM-dd HH:mm:ss.fff"
                        };
                        
                        var parsed = false;
                        foreach (var format in dateFormats)
                        {
                            if (DateTime.TryParseExact(webhookDataVerified.transactionDateTime, format, 
                                System.Globalization.CultureInfo.InvariantCulture, 
                                System.Globalization.DateTimeStyles.None, out var date))
                            {
                                paidDate = date;
                                parsed = true;
                                break;
                            }
                        }
                        
                        if (!parsed && DateTime.TryParse(webhookDataVerified.transactionDateTime, out var date2))
                        {
                            paidDate = date2;
                        }
                    }
                        
                    await _paymentRepository.UpdatePaymentStatusAsync(
                        webhookDataVerified.orderCode, 
                        "PAID", 
                        paidDate
                    );
                    return true;
                }
                else if (webhookDataVerified != null)
                {
                    await _paymentRepository.UpdatePaymentStatusAsync(
                        webhookDataVerified.orderCode, 
                        "CANCELLED"
                    );
                }

                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<bool> HasActivePremiumAsync(int userId)
        {
            var activePremium = await _paymentRepository.GetActivePremiumByUserIdAsync(userId);
            return activePremium != null;
        }

        public async Task<bool> VerifyAndUpdatePaymentStatusAsync(long orderCode)
        {
            try
            {
                // Get payment from database
                var payment = await _paymentRepository.GetByOrderCodeAsync(orderCode);
                if (payment == null) return false;

                // If already paid, no need to check
                if (payment.Status == "PAID") return true;

                // Try to get payment information from PayOS using orderCode
                try
                {
                    var paymentInfo = await _payOS.getPaymentLinkInformation(orderCode);
                    
                    if (paymentInfo != null)
                    {
                        // Use reflection to safely access properties since PaymentLinkInformation structure may vary
                        var paymentInfoType = paymentInfo.GetType();
                        var statusProperty = paymentInfoType.GetProperty("status");
                        
                        if (statusProperty != null)
                        {
                            var status = statusProperty.GetValue(paymentInfo)?.ToString();
                            
                            if (status == "PAID")
                            {
                                // Update payment status - use current time if transactionDateTime is not available
                                DateTime paidDate = DateTime.UtcNow;
                                
                                // Try to get transaction date if available
                                try
                                {
                                    // Check for data property (nested structure)
                                    var dataProperty = paymentInfoType.GetProperty("data");
                                    if (dataProperty != null)
                                    {
                                        var data = dataProperty.GetValue(paymentInfo);
                                        if (data != null)
                                        {
                                            var dataType = data.GetType();
                                            var transactionDateTimeProperty = dataType.GetProperty("transactionDateTime");
                                            if (transactionDateTimeProperty != null)
                                            {
                                                var transactionDateTime = transactionDateTimeProperty.GetValue(data)?.ToString();
                                                if (!string.IsNullOrEmpty(transactionDateTime))
                                                {
                                                    var dateFormats = new[] { 
                                                        "yyyy-MM-dd HH:mm:ss",
                                                        "yyyy-MM-ddTHH:mm:ss",
                                                        "yyyy-MM-ddTHH:mm:ssZ",
                                                        "yyyy-MM-dd HH:mm:ss.fff"
                                                    };
                                                    
                                                    foreach (var format in dateFormats)
                                                    {
                                                        if (DateTime.TryParseExact(transactionDateTime, format, 
                                                            System.Globalization.CultureInfo.InvariantCulture, 
                                                            System.Globalization.DateTimeStyles.None, out var date))
                                                        {
                                                            paidDate = date;
                                                            break;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                catch
                                {
                                    // If we can't extract transaction date, use current time
                                }
                                
                                await _paymentRepository.UpdatePaymentStatusAsync(orderCode, "PAID", paidDate);
                                return true;
                            }
                            else if (status == "CANCELLED" || status == "EXPIRED")
                            {
                                await _paymentRepository.UpdatePaymentStatusAsync(orderCode, "CANCELLED");
                                return false;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    // If we can't get payment info, just return false (payment might not be completed yet)
                    return false;
                }

                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}

