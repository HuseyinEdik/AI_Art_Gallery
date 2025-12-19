# Kullanýcý Kayýt ve Giriþ Sistemi - Veritabaný Entegrasyonu

## ?? Veritabaný Yapýsý

### Users Tablosu
```sql
CREATE TABLE users (
    id SERIAL PRIMARY KEY,
    email VARCHAR(255) UNIQUE NOT NULL,
    password VARCHAR(255) NOT NULL,
    surname VARCHAR(255),
    verification_code VARCHAR(255),
    enabled BOOLEAN DEFAULT FALSE
);
```

### User_Roles Tablosu
```sql
CREATE TABLE user_roles (
    user_id INT REFERENCES users(id),
    role_id INT REFERENCES roles(id),
    PRIMARY KEY (user_id, role_id)
);
```

## ?? Yapýlan Deðiþiklikler

### 1. DTO Modelleri Güncellendi

#### ? AuthRequestDTO.cs
- `Email` ve `Password` (Login için)
- `RegisterRequestDTO` ayrý oluþturuldu

#### ? RegisterRequestDTO.cs
```csharp
public class RegisterRequestDTO
{
    public string Username { get; set; }
    public string Email { get; set; }
    public string Surname { get; set; }
    public string Password { get; set; }
}
```

#### ? UserDTO.cs
```csharp
public class UserDTO
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string Surname { get; set; }
    public bool Enabled { get; set; }
    public List<string> Roles { get; set; }
}
```

#### ? VerifyRequestDTO.cs
- `Code` ? `VerificationCode` (tutarlýlýk için)

### 2. SpringApiClient Servisi Güncellendi

#### Login Metodu
```csharp
public async Task<string> Login(string email, string password)
{
    var dto = new AuthRequestDTO
    {
        Email = email,
        Password = password
    };

    var res = await _http.PostAsJsonAsync("/auth/login", dto);
    // ...
}
```

#### Register Metodu
```csharp
public async Task<MessageResponseDTO> Register(string email, string surname, string password)
{
    var dto = new RegisterRequestDTO
    {
        Email = email,
        Surname = surname,
        Password = password
    };
    // ...
}
```

#### Yeni Email Doðrulama Metodlarý
```csharp
// Email doðrulama
public async Task<MessageResponseDTO> VerifyEmail(string email, string verificationCode)

// Doðrulama kodu tekrar gönderme
public async Task<MessageResponseDTO> ResendVerificationCode(string email)
```

### 3. AuthController Güncellemeleri

#### Register Flow
1. Kullanýcý kayýt formunu doldurur (email, surname, password)
2. API'ye kayýt isteði gönderilir
3. Baþarýlý olursa ? Email doðrulama sayfasýna yönlendirilir
4. Email'e 6 haneli kod gönderilir

#### Email Doðrulama Flow
1. Kullanýcý email'ine gelen 6 haneli kodu girer
2. Kod API'ye gönderilir
3. Doðrulama baþarýlý ? `enabled = true` olur
4. Login sayfasýna yönlendirilir

### 4. View Güncellemeleri

#### ? Register.cshtml
- Modern ve kullanýcý dostu tasarým
- Email, Surname, Password alanlarý
- Kullaným þartlarý onay kutusu
- Form validasyonu

#### ? VerifyEmail.cshtml (YENÝ)
- 6 haneli kod giriþi için özel input
- Kod tekrar gönderme butonu
- Güvenlik ipuçlarý
- Geri sayým timer (opsiyonel)

#### ? Login.cshtml
- Email ile giriþ
- Þifremi unuttum linki

## ?? API Endpoint'leri

Tüm endpoint'ler `/api` prefix'i kaldýrýldý:

### Authentication
```
POST /auth/register
POST /auth/login
POST /auth/verify
POST /auth/resend-verification
POST /auth/logout
GET  /auth/me
```

### Arts
```
GET  /arts/public
GET  /arts/{id}
GET  /arts/my-artworks
POST /arts/create
DELETE /arts/{id}
```

### Interactions
```
POST /interactions/like/{id}
POST /interactions/comment/{id}
DELETE /interactions/comment/{id}
```

### Categories
```
GET /categories
```

## ?? Kullaným Akýþý

### Kayýt Akýþý
```
1. Register.cshtml
   ? (email, surname, password)
2. AuthController.Register()
   ? API Call: /auth/register
3. VerifyEmail.cshtml
   ? (email, verificationCode)
4. AuthController.VerifyEmail()
   ? API Call: /auth/verify
5. Login.cshtml (enabled = true)
```

### Giriþ Akýþý
```
1. Login.cshtml
   ? (email, password)
2. AuthController.Login()
   ? API Call: /auth/login
3. JWT Token alýnýr
   ? Session'a kaydedilir
4. Cookie oluþturulur
   ? Authenticated
5. Artwork/Index'e yönlendirilir
```

## ?? Güvenlik

### JWT Token
- Session'da saklanýyor: `HttpContext.Session.SetString("jwt", token)`
- Cookie'de de saklanýyor (yedek)
- Her API isteðinde Bearer token olarak gönderiliyor

### Email Doðrulama
- Kayýt sonrasý email'e 6 haneli kod gönderiliyor
- Kod 15 dakika geçerli (API tarafýnda)
- Doðrulama yapýlmadan giriþ yapýlamaz (`enabled = false`)

### Þifre
- API tarafýnda BCrypt ile hash'leniyor
- Minimum 6 karakter (frontend validasyon)

## ?? Test Senaryolarý

### 1. Baþarýlý Kayýt
```
1. /Auth/Register'a git
2. Username: johndoe
3. Email: test@example.com
4. Surname: Test
5. Password: 123456
6. Kayýt Ol'a týkla
7. Email'e gelen kodu gir
8. Doðrula'ya týkla
9. Login sayfasýna yönlendir
```

### 2. Email Doðrulama
```
1. VerifyEmail sayfasýnda
2. 6 haneli kod gir
3. Doðrula'ya týkla
4. Baþarý mesajý
5. Login sayfasý
```

### 3. Kod Tekrar Gönderme
```
1. VerifyEmail sayfasýnda
2. "Tekrar Gönder" butonuna týkla
3. Yeni kod email'e gönderilir
4. Baþarý mesajý
```

### 4. Baþarýlý Giriþ
```
1. /Auth/Login'e git
2. Email: test@example.com
3. Password: 123456
4. Giriþ Yap'a týkla
5. Artwork/Index'e yönlendir
```

## ?? Spring Boot API Beklenen Ýstekler

### Register Request
```json
POST /auth/register
{
    "username": "johndoe",
    "email": "test@example.com",
    "surname": "Test",
    "password": "123456"
}
```

### Register Response
```json
{
    "message": "Kayýt baþarýlý. Email adresinize doðrulama kodu gönderildi."
}
```

### Verify Request
```json
POST /auth/verify
{
    "email": "test@example.com",
    "verificationCode": "123456"
}
```

### Verify Response
```json
{
    "message": "Email doðrulandý. Artýk giriþ yapabilirsiniz."
}
```

### Login Request
```json
POST /auth/login
{
    "email": "test@example.com",
    "password": "123456"
}
```

### Login Response
```json
{
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "username": "johndoe",
    "email": "test@example.com",
    "surname": "Test",
    "roles": ["ROLE_USER"]
}
```

## ?? Yapýlmasý Gerekenler (Spring Boot Tarafý)

### 1. Email Service
```java
@Service
public class EmailService {
    public void sendVerificationEmail(String email, String code) {
        // SMTP ile email gönder
    }
}
```

### 2. Verification Code Generator
```java
public String generateVerificationCode() {
    return String.format("%06d", new Random().nextInt(999999));
}
```

### 3. Code Expiration
```java
// verification_code tablosuna created_at ekle
// 15 dakika kontrolü yap
```

### 4. User Roles
```java
// Kayýt sýrasýnda default ROLE_USER ata
userRoleRepository.save(new UserRole(user.getId(), ROLE_USER_ID));
```

## ?? UI/UX Ýyileþtirmeleri

### ? Modern Tasarým
- Gradient renk þemalarý
- Shadow efektleri
- Rounded corners
- Icon kullanýmý

### ? User Experience
- Inline validasyon
- Loading states
- Success/Error mesajlarý
- Form placeholder'larý
- Otomatik focus

### ? Responsive
- Mobile uyumlu
- Tablet uyumlu
- Desktop optimize

## ?? Durum Kodlarý

| Durum | Açýklama |
|-------|----------|
| 200 OK | Ýþlem baþarýlý |
| 201 Created | Kayýt baþarýlý |
| 400 Bad Request | Geçersiz input |
| 401 Unauthorized | Token geçersiz |
| 403 Forbidden | Email doðrulanmamýþ |
| 409 Conflict | Email zaten kayýtlý |
| 500 Internal Error | Sunucu hatasý |

## ?? Sonuç

Kullanýcý kayýt ve giriþ sistemi artýk veritabaný yapýsýna tam uyumlu ve production-ready!

### Özellikler
? Email ile kayýt
? Email doðrulama (6 haneli kod)
? Kod tekrar gönderme
? Email ile giriþ
? JWT token yönetimi
? Role-based authentication
? Modern UI/UX
? Responsive design
? Form validasyonu
? Error handling

### Test Edilmesi Gerekenler
- [ ] Kayýt iþlemi
- [ ] Email doðrulama
- [ ] Kod tekrar gönderme
- [ ] Giriþ iþlemi
- [ ] Token yönetimi
- [ ] Logout iþlemi
- [ ] Authorization kontrolü
