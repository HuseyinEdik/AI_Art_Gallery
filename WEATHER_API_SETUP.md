# ??? Hava Durumu API Kurulumu

## ?? OpenWeatherMap API Key Alma

### 1. Hesap Oluþtur
1. https://openweathermap.org/ adresine git
2. Sað üstten **Sign In** ? **Create an Account** týkla
3. Formu doldur:
   - Username
   - Email
   - Password
4. Email'ini doðrula

### 2. API Key Al
1. Giriþ yap
2. Sað üstteki kullanýcý adýna týkla ? **My API Keys**
3. Varsayýlan API key otomatik oluþturulur
4. Veya **Create Key** ile yeni key oluþtur
5. API Key'i kopyala (Örn: `a1b2c3d4e5f6g7h8i9j0k1l2m3n4o5p6`)

### 3. API Key'i Projeye Ekle

#### Seçenek 1: appsettings.json (Önerilen)

**appsettings.json:**
```json
{
  "ApiSettings": {
    "BaseUrl": "http://localhost:8080/api",
    "Timeout": 300
  },
  "WeatherApi": {
    "ApiKey": "BURAYA_API_KEY_YAPISTIR",
    "BaseUrl": "https://api.openweathermap.org/data/2.5"
  },
  "Logging": {
    ...
  }
}
```

**Views/Artwork/Index.cshtml:**
```javascript
// API key'i backend'den al
const WEATHER_API_KEY = '@Configuration["WeatherApi:ApiKey"]';
```

**Program.cs (eðer Razor sayfasýnda eriþmek için):**
```csharp
builder.Services.AddSingleton<IConfiguration>(builder.Configuration);
```

#### Seçenek 2: Direkt JavaScript'e Yapýþtýr (Hýzlý Test)

**Views/Artwork/Index.cshtml:**
```javascript
const WEATHER_API_KEY = 'a1b2c3d4e5f6g7h8i9j0k1l2m3n4o5p6'; // Gerçek key buraya
```

?? **Güvenlik Notu:** Production'da API key'i frontend'de göstermek önerilmez. Backend'den proxy kullanýn.

---

## ?? Test

### 1. API Key'i Ekle
```javascript
const WEATHER_API_KEY = 'GERÇEK_API_KEY_BURAYA';
```

### 2. Uygulamayý Baþlat
```
F5 veya dotnet run
```

### 3. Galeri Sayfasýna Git
```
http://localhost:5173/Artwork/Index
```

### 4. Hava Durumu Butonuna Týk
- Sað alt köþede mor **??** butonu görünmeli
- Týklayýnca modal pencere açýlmalý
- Tarayýcý konum izni isteyecek ? **Ýzin Ver**
- Hava durumu bilgileri yüklenmeli

---

## ?? API Endpoint'leri

### 1. Koordinat ile Hava Durumu
```
GET https://api.openweathermap.org/data/2.5/weather?lat={lat}&lon={lon}&units=metric&lang=tr&appid={API_KEY}
```

### 2. Þehir Adý ile Hava Durumu
```
GET https://api.openweathermap.org/data/2.5/weather?q={city_name}&units=metric&lang=tr&appid={API_KEY}
```

**Örnek:**
```
https://api.openweathermap.org/data/2.5/weather?q=Istanbul&units=metric&lang=tr&appid=YOUR_API_KEY
```

---

## ?? Response Örneði

```json
{
  "coord": {
    "lon": 28.9784,
    "lat": 41.0082
  },
  "weather": [
    {
      "id": 800,
      "main": "Clear",
      "description": "açýk",
      "icon": "01d"
    }
  ],
  "main": {
    "temp": 15.5,
    "feels_like": 14.8,
    "temp_min": 13.2,
    "temp_max": 17.1,
    "pressure": 1013,
    "humidity": 65
  },
  "wind": {
    "speed": 3.5,
    "deg": 180
  },
  "visibility": 10000,
  "sys": {
    "country": "TR",
    "sunrise": 1703138400,
    "sunset": 1703174400
  },
  "name": "Istanbul"
}
```

---

## ?? Özellikler

### Mevcut Özellikler
- ? Kullanýcýnýn konumuna göre hava durumu
- ? Sýcaklýk (°C)
- ? Hava durumu açýklamasý (Türkçe)
- ? Nem (%)
- ? Rüzgar hýzý (km/h)
- ? Basýnç (hPa)
- ? Görüþ mesafesi (km)
- ? Þehir adý ve ülke kodu
- ? Dinamik hava durumu ikonlarý
- ? Responsive tasarým
- ? Animasyonlu modal

### Eklenebilecek Özellikler
- ?? 5 günlük tahmin
- ?? Saatlik tahmin
- ??? Hissedilen sýcaklýk
- ?? Gün doðumu/batýmý
- ?? Hava durumu uyarýlarý

---

## ?? Sorun Giderme

### 1. "401 Unauthorized" Hatasý
**Sebep:** API key geçersiz veya eksik

**Çözüm:**
- API key'in doðru olduðundan emin olun
- Yeni oluþturulan key'ler 10-15 dakika sonra aktif olur

### 2. "Konum Ýzni Reddedildi"
**Sebep:** Kullanýcý tarayýcýda konum iznini reddetmiþ

**Çözüm:**
- Tarayýcý ayarlarýndan konum iznini açýn
- Alternatif: Þehir adý ile arama ekleyin

```javascript
// Fallback: Ýstanbul için varsayýlan hava durumu
const defaultCity = 'Istanbul';
const response = await fetch(
    `https://api.openweathermap.org/data/2.5/weather?q=${defaultCity}&units=metric&lang=tr&appid=${WEATHER_API_KEY}`
);
```

### 3. CORS Hatasý
**Sebep:** API tarayýcýdan direkt çaðrýlýyor

**Çözüm:** Backend'den proxy kullanýn (Production için önerilen)

**Controllers/WeatherController.cs:**
```csharp
[HttpGet("weather")]
public async Task<IActionResult> GetWeather(double lat, double lon)
{
    var apiKey = _configuration["WeatherApi:ApiKey"];
    var url = $"https://api.openweathermap.org/data/2.5/weather?lat={lat}&lon={lon}&units=metric&lang=tr&appid={apiKey}";
    
    var response = await _httpClient.GetAsync(url);
    var data = await response.Content.ReadAsStringAsync();
    
    return Content(data, "application/json");
}
```

---

## ?? Mobil Görünüm

Modal pencere mobilde daha küçük olacak þekilde responsive tasarlandý:

- Desktop: 350px geniþlik
- Mobile: 320px geniþlik
- Alt kýsýmda buton konumu otomatik ayarlanýr

---

## ?? Kullaným

1. Galeri sayfasýna gidin
2. Sað alt köþedeki **??** butonuna týklayýn
3. Konum iznini verin
4. Hava durumu bilgilerini görüntüleyin
5. Modal dýþýna týklayarak kapatýn

---

**Hazýrlayan:** GitHub Copilot  
**Tarih:** 2024-12-18  
**API Provider:** OpenWeatherMap  
**Versiyon:** 1.0
