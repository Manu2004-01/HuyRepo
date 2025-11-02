# Hướng Dẫn API Thanh Toán (Payment API)

## Tổng Quan

Hệ thống tích hợp PayOS để xử lý thanh toán cho gói Premium. API hỗ trợ tạo link thanh toán, xác thực webhook, và quản lý lịch sử thanh toán.

**Base URL:** `/api/payment`

**Lưu ý:** Hầu hết các endpoint yêu cầu authentication (JWT token).

---

## 1. Tạo Link Thanh Toán Premium

Tạo link thanh toán cho gói Premium 1 tháng (29,000 VNĐ).

### Endpoint
```
POST /api/payment/premium
```

### Authentication
✅ Yêu cầu (Bearer Token)

### Request Query Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `returnUrl` | string | No | URL chuyển hướng sau khi thanh toán thành công. Mặc định: `{API_url}/payment/success` |
| `cancelUrl` | string | No | URL chuyển hướng khi người dùng hủy thanh toán. Mặc định: `{API_url}/payment/cancel` |

### Request Example

```bash
curl -X POST "https://your-api.com/api/payment/premium?returnUrl=https://yoursite.com/success&cancelUrl=https://yoursite.com/cancel" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

### Response Success (200 OK)

```json
{
  "checkoutUrl": "https://pay.payos.vn/web/...",
  "qrCode": "data:image/png;base64,...",
  "orderCode": 123456789012
}
```

### Response Fields

| Field | Type | Description |
|-------|------|-------------|
| `checkoutUrl` | string | URL trang thanh toán PayOS |
| `qrCode` | string | Mã QR Code dạng base64 để quét thanh toán |
| `orderCode` | long | Mã đơn hàng duy nhất |

### Response Error

```json
{
  "statusCode": 500,
  "message": "Lỗi khi tạo thanh toán: {error message}"
}
```

---

## 2. Webhook PayOS

Endpoint nhận thông báo từ PayOS khi có sự kiện thanh toán (thành công, hủy, v.v.)

### Endpoint
```
POST /api/payment/webhook
```

### Authentication
❌ Không yêu cầu (PayOS gọi trực tiếp)

### Request Body

PayOS sẽ gửi dữ liệu JSON theo format của WebhookType:

```json
{
  "code": "00",
  "desc": "success",
  "success": true,
  "data": {
    "orderCode": 123456789012,
    "amount": 29000,
    "description": "Gói Premium EatIT - 1 tháng",
    "accountNumber": "0399609015",
    "reference": "FT23325781308800",
    "transactionDateTime": "2023-11-21 15:20:34",
    "currency": "VND",
    "paymentLinkId": "b646a39ca8654d8fa03e0dc8bec7264c",
    "code": "00",
    "desc": "success"
  },
  "signature": "1f2eb76896a3a8e10e1f560bed4087f788c5d654af6d0a1d394351806a34d6dd"
}
```

### Response

Endpoint luôn trả về success để PayOS không retry:

```json
{
  "code": 0,
  "desc": "success"
}
```

### Lưu ý

- Endpoint này tự động cập nhật trạng thái thanh toán trong database
- Nếu `code == "00"` → Cập nhật status = "PAID"
- Nếu không → Cập nhật status = "CANCELLED"
- Cần cấu hình webhook URL trong PayOS Dashboard: `https://your-api.com/api/payment/webhook`

---

## 3. Lấy Lịch Sử Thanh Toán

Lấy danh sách tất cả các giao dịch thanh toán của user hiện tại.

### Endpoint
```
GET /api/payment/history
```

### Authentication
✅ Yêu cầu (Bearer Token)

### Request Example

```bash
curl -X GET "https://your-api.com/api/payment/history" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

### Response Success (200 OK)

```json
{
  "payments": [
    {
      "paymentId": 1,
      "orderCode": 123456789012,
      "amount": 29000,
      "description": "Gói Premium EatIT - 1 tháng",
      "status": "PAID",
      "paymentType": "Premium",
      "premiumExpiryDate": "2024-12-21T15:20:34Z",
      "createdAt": "2023-11-21T15:20:00Z",
      "paidAt": "2023-11-21T15:20:34Z"
    },
    {
      "paymentId": 2,
      "orderCode": 123456789013,
      "amount": 29000,
      "description": "Gói Premium EatIT - 1 tháng",
      "status": "PENDING",
      "paymentType": "Premium",
      "premiumExpiryDate": null,
      "createdAt": "2023-11-22T10:00:00Z",
      "paidAt": null
    }
  ]
}
```

### Response Fields

| Field | Type | Description |
|-------|------|-------------|
| `paymentId` | int | ID thanh toán |
| `orderCode` | long | Mã đơn hàng |
| `amount` | int | Số tiền (VNĐ) |
| `description` | string | Mô tả thanh toán |
| `status` | string | Trạng thái: PENDING, PAID, CANCELLED |
| `paymentType` | string | Loại thanh toán (Premium) |
| `premiumExpiryDate` | DateTime? | Ngày hết hạn Premium (null nếu chưa thanh toán) |
| `createdAt` | DateTime | Thời gian tạo |
| `paidAt` | DateTime? | Thời gian thanh toán (null nếu chưa thanh toán) |

---

## 4. Kiểm Tra Trạng Thái Premium

Kiểm tra xem user hiện tại có đang sở hữu gói Premium đang hoạt động hay không.

### Endpoint
```
GET /api/payment/premium-status
```

### Authentication
✅ Yêu cầu (Bearer Token)

### Request Example

```bash
curl -X GET "https://your-api.com/api/payment/premium-status" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

### Response Success (200 OK)

#### Trường hợp có Premium

```json
{
  "hasPremium": true,
  "expiryDate": "2024-12-21T15:20:34Z",
  "orderCode": 123456789012,
  "paidAt": "2023-11-21T15:20:34Z"
}
```

#### Trường hợp không có Premium

```json
{
  "hasPremium": false,
  "expiryDate": null
}
```

### Response Fields

| Field | Type | Description |
|-------|------|-------------|
| `hasPremium` | bool | Có Premium đang hoạt động không |
| `expiryDate` | DateTime? | Ngày hết hạn Premium |
| `orderCode` | long? | Mã đơn hàng Premium (chỉ có khi hasPremium = true) |
| `paidAt` | DateTime? | Thời gian thanh toán (chỉ có khi hasPremium = true) |

---

## Cấu Hình PayOS

Cần cấu hình trong `appsettings.json`:

```json
{
  "PayOS": {
    "ClientId": "your-client-id",
    "ApiKey": "your-api-key",
    "ChecksumKey": "your-checksum-key"
  },
  "API_url": "https://your-api.com/"
}
```

## Luồng Thanh Toán

1. **Client gọi** `POST /api/payment/premium` → Nhận `checkoutUrl` và `qrCode`
2. **User thanh toán** trên PayOS
3. **PayOS gọi webhook** `POST /api/payment/webhook` → Hệ thống tự động cập nhật status
4. **User được redirect** về `returnUrl` hoặc `cancelUrl`
5. **Client có thể kiểm tra** status qua `GET /api/payment/premium-status`

## Mã Lỗi Thường Gặp

| Status Code | Mô tả |
|-------------|-------|
| 200 | Thành công |
| 401 | Token không hợp lệ hoặc chưa đăng nhập |
| 500 | Lỗi server (xem message để biết chi tiết) |

## Ghi Chú

- Tất cả giá trị tiền tệ là VNĐ
- OrderCode được generate ngẫu nhiên trong khoảng 100000000000 - 999999999999
- Premium có thời hạn 1 tháng (30 ngày) từ ngày thanh toán
- Webhook endpoint cần public accessible (không được protect bằng authentication)

