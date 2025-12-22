# ?? Spring Boot API - Rol Yapýsý Düzeltmeleri

## ?? Sorun
Veritabanýnda roller **"USER"** ve **"ADMIN"** olarak tutuluyor, ancak MVC uygulamasý **"ROLE_USER"** ve **"ROLE_ADMIN"** formatýný bekliyor.

## ? Çözüm

### Seçenek 1: API Response'unda Rol Formatýný Deðiþtir (Önerilen)

Spring Boot'ta Login response'unda rolleri **ROLE_ prefix'i ile** döndürün.

#### ?? Dosya: `AuthController.java`

```java
@RestController
@RequestMapping("/api/auth")
@CrossOrigin(origins = "http://localhost:5173")
public class AuthController {

    @Autowired
    private AppUserRepository userRepository;
    
    @Autowired
    private PasswordEncoder passwordEncoder;
    
    @Autowired
    private JwtTokenProvider jwtTokenProvider;

    @PostMapping("/login")
    public LoginResponseDTO login(@RequestBody AuthRequestDTO request) {
        AppUser user = userRepository.findByEmail(request.getEmail())
            .orElseThrow(() -> new RuntimeException("User not found"));
        
        if (!passwordEncoder.matches(request.getPassword(), user.getPassword())) {
            throw new RuntimeException("Invalid password");
        }
        
        // JWT token oluþtur
        String token = jwtTokenProvider.generateToken(user.getEmail());
        
        // ÖNEMLÝ: Rolleri ROLE_ prefix'i ile döndür
        List<String> roleNames = user.getRoles().stream()
            .map(role -> {
                String roleName = role.getName();
                // Eðer zaten ROLE_ ile baþlýyorsa olduðu gibi dön
                if (roleName.startsWith("ROLE_")) {
                    return roleName;
                }
                // Deðilse baþýna ROLE_ ekle (USER ? ROLE_USER, ADMIN ? ROLE_ADMIN)
                return "ROLE_" + roleName;
            })
            .collect(Collectors.toList());
        
        LoginResponseDTO response = new LoginResponseDTO();
        response.setToken(token);
        response.setId(user.getId());
        response.setUsername(user.getUsername());
        response.setEmail(user.getEmail());
        response.setSurname(user.getSurname());
        response.setRoles(roleNames); // "ROLE_USER", "ROLE_ADMIN" formatýnda
        
        return response;
    }
}
```

---

### Seçenek 2: Veritabanýndaki Rol Ýsimlerini Deðiþtir

Eðer API'yi deðiþtirmek istemiyorsanýz, veritabanýndaki rol isimlerini güncelleyin.

#### ?? SQL Script

```sql
-- PostgreSQL
UPDATE role SET name = 'ROLE_USER' WHERE name = 'USER';
UPDATE role SET name = 'ROLE_ADMIN' WHERE name = 'ADMIN';

-- MySQL
UPDATE role SET name = 'ROLE_USER' WHERE name = 'USER';
UPDATE role SET name = 'ROLE_ADMIN' WHERE name = 'ADMIN';
```

---

### Seçenek 3: Spring Security Configurasyon'u (Ýleri Seviye)

Eðer Spring Security kullanýyorsanýz, `RoleHierarchy` ekleyebilirsiniz.

#### ?? Dosya: `SecurityConfig.java`

```java
@Configuration
@EnableWebSecurity
public class SecurityConfig {

    @Bean
    public RoleHierarchy roleHierarchy() {
        RoleHierarchyImpl roleHierarchy = new RoleHierarchyImpl();
        // ADMIN rolü USER yetkilerine de sahip olsun
        roleHierarchy.setHierarchy("ROLE_ADMIN > ROLE_USER");
        return roleHierarchy;
    }
}
```

---

## ?? Test

### 1. Postman/Thunder Client ile Login Test

```http
POST http://localhost:8080/api/auth/login
Content-Type: application/json

{
  "email": "admin@example.com",
  "password": "admin123"
}
```

**Beklenen Response:**

```json
{
  "token": "eyJhbGc...",
  "id": 1,
  "username": "admin",
  "email": "admin@example.com",
  "surname": "User",
  "roles": [
    "ROLE_ADMIN",  // ? ROLE_ prefix'i ile
    "ROLE_USER"
  ]
}
```

### 2. Veritabaný Kontrolü

```sql
-- Roller kontrol
SELECT * FROM role;
```

**Beklenen:**

| id | name |
|----|------|
| 1  | USER veya ROLE_USER |
| 2  | ADMIN veya ROLE_ADMIN |

### 3. MVC Uygulamasýnda Test

1. Admin kullanýcýsý ile login yapýn
2. Navbar'da **"Admin"** linki görünmeli
3. `/Admin/Index` sayfasýna eriþebilmeli

---

## ?? Rol Formatlarý Karþýlaþtýrma

| Kaynak | USER Rolü | ADMIN Rolü |
|--------|-----------|------------|
| Veritabaný (Önce) | USER | ADMIN |
| API Response (Önce) | USER | ADMIN |
| MVC Beklentisi | ROLE_USER | ROLE_ADMIN |
| **API Response (Sonra)** | **ROLE_USER** | **ROLE_ADMIN** |

---

## ?? Önemli Notlar

1. **Seçenek 1 Önerilir**: API response'unda rol formatýný deðiþtirmek en temiz çözümdür.
2. **Veritabaný deðiþikliði**: Eðer baþka uygulamalar da ayný veritabanýný kullanýyorsa dikkatli olun.
3. **Backward Compatibility**: MVC uygulamasý **her iki formatý da** destekler:
   - `User.IsInRole("ROLE_ADMIN")` ? Yeni format
   - `User.IsInRole("Admin")` ? Eski format (fallback)

---

## ? MVC Tarafýnda Yapýlan Deðiþiklikler

MVC uygulamasý **her iki formatý da** desteklemek üzere güncellendi:

```csharp
// AuthController.cs
foreach (var role in loginResponse.Roles)
{
    var normalizedRole = role.StartsWith("ROLE_") ? role : $"ROLE_{role}";
    claims.Add(new Claim(ClaimTypes.Role, normalizedRole));
}

// _Layout.cshtml
var isAdmin = User.IsInRole("ROLE_ADMIN") || User.IsInRole("Admin");

// AdminController.cs
[Authorize(Roles = "ROLE_ADMIN,Admin")] // Her iki formatý da destekle
```

---

**Hazýrlayan:** GitHub Copilot  
**Tarih:** 2024-12-18  
**Versiyon:** 1.0
