# Hướng dẫn Google Authentication cho EatIT API

## Tổng quan
EatIT API đã được tích hợp Google Authentication để cho phép user đăng nhập và đăng ký tài khoản bằng Google account.

## Cấu hình Google OAuth

### 1. Tạo Google OAuth Application
1. Truy cập [Google Cloud Console](https://console.cloud.google.com/)
2. Tạo project mới hoặc chọn project hiện có
3. Kích hoạt Google+ API
4. Tạo OAuth 2.0 credentials:
   - Chọn "Web application"
   - Thêm authorized redirect URIs:
     - `https://yourdomain.com/api/auth/google-response` (production)
     - `https://localhost:7091/api/auth/google-response` (development)

### 2. Cấu hình trong appsettings.json
```json
{
  "Google": {
    "ClientId": "YOUR_GOOGLE_CLIENT_ID",
    "ClientSecret": "YOUR_GOOGLE_CLIENT_SECRET"
  }
}
```

## API Endpoints

### 1. Google Login (Server-side OAuth)
```
GET /api/auth/google-login
```
- Redirect user đến Google OAuth page
- Sau khi user xác thực, Google sẽ redirect về `/api/auth/google-response`

### 2. Google Response Handler
```
GET /api/auth/google-response
```
- Xử lý response từ Google OAuth
- Tự động tạo tài khoản mới nếu chưa tồn tại
- Trả về JWT token và thông tin user

### 3. Google Login với Token (Client-side)
```
POST /api/auth/google-login-token
Content-Type: application/json

{
  "googleId": "google_user_id",
  "email": "user@example.com",
  "name": "User Name",
  "picture": "https://profile-picture-url.com"
}
```

**Response:**
```json
{
  "token": "jwt_token_here",
  "user": {
    "id": 1,
    "userName": "User Name",
    "email": "user@example.com",
    "roleId": 2,
    "RoleName": "User",
    "userImg": "https://profile-picture-url.com"
  },
  "isNewUser": true,
  "message": "Đăng ký tài khoản thành công!"
}
```

### 4. Google Registration (Chỉ đăng ký)
```
POST /api/auth/google-register
Content-Type: application/json

{
  "googleId": "google_user_id",
  "email": "user@example.com",
  "name": "User Name",
  "picture": "https://profile-picture-url.com"
}
```

**Response:**
```json
{
  "token": "jwt_token_here",
  "user": {
    "id": 1,
    "userName": "User Name",
    "email": "user@example.com",
    "roleId": 2,
    "RoleName": "User",
    "userImg": "https://profile-picture-url.com"
  },
  "message": "Đăng ký tài khoản thành công!"
}
```

## Frontend Integration

### 1. Sử dụng Google Sign-In JavaScript Library

```html
<!-- Thêm Google Sign-In script -->
<script src="https://accounts.google.com/gsi/client" async defer></script>

<!-- Google Sign-In Button -->
<div id="g_id_onload"
     data-client_id="YOUR_GOOGLE_CLIENT_ID"
     data-callback="handleCredentialResponse">
</div>
<div class="g_id_signin" data-type="standard"></div>
```

```javascript
function handleCredentialResponse(response) {
    // Decode JWT token
    const responsePayload = decodeJwtResponse(response.credential);
    
    // Gửi thông tin đến API
    const userData = {
        googleId: responsePayload.sub,
        email: responsePayload.email,
        name: responsePayload.name,
        picture: responsePayload.picture
    };
    
    // Gọi API đăng nhập/đăng ký
    fetch('/api/auth/google-login-token', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify(userData)
    })
    .then(response => response.json())
    .then(data => {
        if (data.token) {
            // Lưu token và redirect
            localStorage.setItem('token', data.token);
            if (data.isNewUser) {
                alert('Đăng ký tài khoản thành công!');
            } else {
                alert('Đăng nhập thành công!');
            }
            window.location.href = '/dashboard';
        }
    })
    .catch(error => {
        console.error('Error:', error);
    });
}

function decodeJwtResponse(token) {
    var base64Url = token.split('.')[1];
    var base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
    var jsonPayload = decodeURIComponent(atob(base64).split('').map(function(c) {
        return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
    }).join(''));
    return JSON.parse(jsonPayload);
}
```

### 2. Sử dụng React với @google-cloud/oauth2

```jsx
import { GoogleLogin } from '@react-oauth/google';

function LoginComponent() {
    const handleGoogleSuccess = (credentialResponse) => {
        const responsePayload = decodeJwtResponse(credentialResponse.credential);
        
        const userData = {
            googleId: responsePayload.sub,
            email: responsePayload.email,
            name: responsePayload.name,
            picture: responsePayload.picture
        };
        
        // Gọi API
        fetch('/api/auth/google-login-token', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(userData)
        })
        .then(response => response.json())
        .then(data => {
            if (data.token) {
                localStorage.setItem('token', data.token);
                // Handle success
            }
        });
    };

    return (
        <GoogleLogin
            onSuccess={handleGoogleSuccess}
            onError={() => {
                console.log('Login Failed');
            }}
        />
    );
}
```

## Database Schema

### Users Table
```sql
ALTER TABLE Users ADD GoogleId VARCHAR(100) NULL;
```

## Error Handling

### Common Error Responses
- `400`: Dữ liệu đầu vào không hợp lệ
- `401`: Thông tin đăng nhập không hợp lệ
- `500`: Lỗi máy chủ nội bộ

### Error Response Format
```json
{
  "statusCode": 400,
  "message": "Error message here"
}
```

## Security Notes

1. **HTTPS Required**: Google OAuth yêu cầu HTTPS trong production
2. **Client Secret**: Không bao giờ expose client secret ở frontend
3. **Token Validation**: Luôn validate JWT token ở backend
4. **CORS**: Cấu hình CORS đúng cách cho domain của bạn

## Testing

### Development
- Sử dụng `https://localhost:7091` cho redirect URI
- Đảm bảo Google Client ID được cấu hình đúng

### Production
- Cập nhật redirect URI thành domain production
- Sử dụng HTTPS
- Cấu hình CORS cho domain production
