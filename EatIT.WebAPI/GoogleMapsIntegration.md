# Hướng dẫn tích hợp Google Maps API - EatIT

## Tổng quan
Dự án EatIT đã được tích hợp Google Maps API để hiển thị vị trí các nhà hàng trên bản đồ. API key hiện tại: `AIzaSyCsY85R06SKIDf8kmU6RXjc60jMHs37D80`

## Cấu trúc dữ liệu Restaurant
Entity `Restaurants` đã có sẵn các trường tọa độ:
```csharp
public float Latitude { get; set; }
public float Longitude { get; set; }
```

## API Endpoints mới

### 1. Lấy dữ liệu restaurant cho bản đồ
```
GET /api/restaurant/get-restaurant-for-map/{id}
```
**Response:**
```json
{
  "resName": "Tên nhà hàng",
  "resAddress": "Địa chỉ",
  "latitude": 10.762622,
  "longitude": 106.660172,
  "restaurantImg": "url_ảnh",
  "openingHours": "Giờ mở cửa",
  "tagName": "Loại nhà hàng"
}
```

### 2. Lấy tất cả restaurants cho bản đồ
```
GET /api/restaurant/get-all-restaurants-for-map?search={searchTerm}
```
**Response:**
```json
{
  "totalItems": 10,
  "restaurants": [
    {
      "id": 1,
      "resName": "Tên nhà hàng",
      "resAddress": "Địa chỉ",
      "latitude": 10.762622,
      "longitude": 106.660172,
      "restaurantImg": "url_ảnh",
      "tagName": "Loại nhà hàng"
    }
  ]
}
```

## Giao diện người dùng

### 1. Bản đồ nhà hàng đơn lẻ
**File:** `wwwroot/restaurant-map.html`
**URL:** `http://localhost:5000/restaurant-map.html?id={restaurantId}`

**Tính năng:**
- Hiển thị vị trí nhà hàng trên bản đồ
- Marker với thông tin chi tiết
- Cập nhật tọa độ thủ công
- Lấy vị trí hiện tại
- Tự động load dữ liệu từ API

### 2. Bản đồ tất cả nhà hàng
**File:** `wwwroot/restaurants-map.html`
**URL:** `http://localhost:5000/restaurants-map.html`

**Tính năng:**
- Hiển thị tất cả nhà hàng trên bản đồ
- Tìm kiếm nhà hàng
- Danh sách nhà hàng bên cạnh
- Thống kê số lượng
- Click để xem chi tiết

## Cách sử dụng

### 1. Truy cập bản đồ đơn lẻ
```javascript
// Mở bản đồ cho restaurant ID = 1
window.open('restaurant-map.html?id=1', '_blank');
```

### 2. Truy cập bản đồ tổng quan
```javascript
// Mở bản đồ tất cả nhà hàng
window.open('restaurants-map.html', '_blank');

// Mở với tìm kiếm
window.open('restaurants-map.html?search=pizza', '_blank');
```

### 3. Tích hợp vào ứng dụng
```html
<!-- Thêm button xem bản đồ -->
<button onclick="viewOnMap(restaurantId)">🗺️ Xem trên bản đồ</button>

<script>
function viewOnMap(restaurantId) {
    window.open(`restaurant-map.html?id=${restaurantId}`, '_blank');
}
</script>
```

## Bảo mật API Key

⚠️ **Lưu ý quan trọng:** API key hiện tại đang được expose trong client-side code. Để bảo mật hơn:

### 1. Giới hạn API key
- Vào [Google Cloud Console](https://console.cloud.google.com/)
- Chọn API key
- Thêm HTTP referrers: `localhost:5000/*`, `yourdomain.com/*`
- Chỉ enable các API cần thiết: Maps JavaScript API, Places API

### 2. Sử dụng environment variables
```csharp
// Trong appsettings.json
{
  "GoogleMaps": {
    "ApiKey": "AIzaSyCsY85R06SKIDf8kmU6RXjc60jMHs37D80"
  }
}
```

### 3. Server-side proxy (khuyến nghị)
Tạo endpoint server-side để proxy requests đến Google Maps API:
```csharp
[HttpGet("geocode")]
public async Task<ActionResult> Geocode(string address)
{
    // Gọi Google Geocoding API từ server
    // Trả về tọa độ cho client
}
```

## Tối ưu hóa

### 1. Clustering markers
Khi có nhiều nhà hàng gần nhau, sử dụng MarkerClusterer:
```javascript
const markerCluster = new MarkerClusterer(map, markers, {
    imagePath: 'https://developers.google.com/maps/documentation/javascript/examples/markerclusterer/m'
});
```

### 2. Lazy loading
Chỉ load markers khi cần thiết:
```javascript
// Load markers trong viewport
map.addListener('bounds_changed', () => {
    const bounds = map.getBounds();
    loadMarkersInBounds(bounds);
});
```

### 3. Caching
Cache dữ liệu restaurant để giảm API calls:
```javascript
// Cache trong localStorage
localStorage.setItem('restaurants', JSON.stringify(restaurants));
```

## Troubleshooting

### 1. API key không hoạt động
- Kiểm tra API key có đúng không
- Kiểm tra HTTP referrers trong Google Cloud Console
- Kiểm tra API có được enable không

### 2. Bản đồ không hiển thị
- Kiểm tra console errors
- Kiểm tra network requests
- Kiểm tra CORS settings

### 3. Tọa độ không chính xác
- Kiểm tra format tọa độ (lat, lng)
- Kiểm tra dữ liệu trong database
- Sử dụng Google Geocoding API để validate

## Mở rộng tính năng

### 1. Directions API
Thêm tính năng chỉ đường:
```javascript
const directionsService = new google.maps.DirectionsService();
const directionsRenderer = new google.maps.DirectionsRenderer();
```

### 2. Places API
Tìm kiếm nhà hàng gần vị trí hiện tại:
```javascript
const service = new google.maps.places.PlacesService(map);
service.nearbySearch(request, callback);
```

### 3. Street View
Hiển thị Street View của nhà hàng:
```javascript
const panorama = new google.maps.StreetViewPanorama(
    document.getElementById('streetview'),
    {
        position: restaurantPosition,
        pov: { heading: 0, pitch: 0 }
    }
);
```

## Liên hệ hỗ trợ
Nếu có vấn đề với tích hợp Google Maps, vui lòng:
1. Kiểm tra console errors
2. Kiểm tra network requests
3. Liên hệ team phát triển
