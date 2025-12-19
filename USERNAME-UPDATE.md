# Username Alaný Eklendi - Güncelleme Özeti

## ?? Veritabaný Yapýsý (Güncellenmiþ)

### Users Tablosu
```sql
CREATE TABLE users (
    id SERIAL PRIMARY KEY,
    username VARCHAR(255) UNIQUE NOT NULL,  -- ? YENÝ EKLENDI
    email VARCHAR(255) UNIQUE NOT NULL,
    password VARCHAR(255) NOT NULL,
    surname VARCHAR(255),
    verification_code VARCHAR(255),
    enabled BOOLEAN DEFAULT FALSE
);
```

## ? Yapýlan Deðiþiklikler

### 1. DTO Güncellemeleri

#### AuthRequestDTO.cs
```csharp
// Login için
public class AuthRequestDTO
{
    public string Email { get; set; }
    public string Password { get; set; }
}

// Register için
public class RegisterRequestDTO
{
    public string Username { get; set; }  // ? YENÝ
    public string Email { get; set; }
    public string Surname { get; set; }
    public string Password { get; set; }
}
```

#### UserDTO.cs
```csharp
public class UserDTO
{
    public int Id { get; set; }
    public string Username { get; set; }  // ? YENÝ
    public string Email { get; set; }
    public string Surname { get; set; }
    public bool Enabled { get; set; }
    public List<string> Roles { get; set; }
}
```

### 2. SpringApiClient.cs
```csharp
// Register metodu güncellendi
public async Task<MessageResponseDTO> Register(
    string username,  // ? YENÝ
    string email, 
    string surname, 
    string password)
{
    var dto = new RegisterRequestDTO
    {
        Username = username,  // ? YENÝ
        Email = email,
        Surname = surname,
        Password = password
    };
    // ...
}
```

### 3. AuthController.cs
```csharp
[HttpPost]
public async Task<IActionResult> Register(
    string username,  // ? YENÝ
    string email, 
    string surname, 
    string password)
{
    var response = await _api.Register(username, email, surname, password);
    // ...
}
```

### 4. Views\Auth\Register.cshtml
```html
<div class="mb-3">
    <label>KULLANICI ADI</label>
    <input type="text" name="username" required />
    <small>Benzersiz olmalýdýr.</small>
</div>

<div class="mb-3">
    <label>E-POSTA</label>
    <input type="email" name="email" required />
</div>

<div class="mb-3">
    <label>SOYAD</label>
    <input type="text" name="surname" required />
</div>

<div class="mb-4">
    <label>ÞÝFRE</label>
    <input type="password" name="password" minlength="6" required />
</div>
```

## ?? Kayýt Form Alanlarý

| Alan | Tip | Zorunlu | Açýklama |
|------|-----|---------|----------|
| **Username** | text | ? | Benzersiz kullanýcý adý |
| **Email** | email | ? | Benzersiz email adresi |
| **Surname** | text | ? | Kullanýcýnýn soyadý |
| **Password** | password | ? | Min. 6 karakter |

## ?? Ýþ Akýþý (Güncellenmiþ)

### Kayýt Akýþý
```
1. Register sayfasý açýlýr
2. Kullanýcý formu doldurur:
   - Username: johndoe
   - Email: test@example.com
   - Surname: Doe
   - Password: 123456
3. POST /auth/register isteði gönderilir
4. Backend:
   - Username unique kontrolü
   - Email unique kontrolü
   - Verification code üretilir
   - Email gönderilir
5. VerifyEmail sayfasýna yönlendirilir
6. 6 haneli kod girilir
7. Doðrulama yapýlýr (enabled = true)
8. Login sayfasýna yönlendirilir
```

### Giriþ Akýþý
```
1. Login sayfasý açýlýr
2. Email ve þifre girilir
3. POST /auth/login isteði gönderilir
4. Backend:
   - Email/password kontrolü
   - enabled = true kontrolü
   - JWT token üretilir
5. Token session'a kaydedilir
6. Artwork/Index'e yönlendirilir
```

## ?? Test Senaryolarý

### Test 1: Username Unique Kontrolü
```
1. Kayýt yap: username=johndoe, email=john@test.com
2. Doðrula
3. Tekrar kayýt dene: username=johndoe, email=different@test.com
4. Beklenen: "Bu kullanýcý adý zaten kullanýlýyor" hatasý
```

### Test 2: Email Unique Kontrolü
```
1. Kayýt yap: username=johndoe, email=john@test.com
2. Doðrula
3. Tekrar kayýt dene: username=different, email=john@test.com
4. Beklenen: "Bu email zaten kullanýlýyor" hatasý
```

### Test 3: Baþarýlý Kayýt
```
1. Username: newuser
2. Email: new@test.com
3. Surname: User
4. Password: 123456
5. Kayýt Ol ? Email doðrulama sayfasý
6. Kodu gir ? Login sayfasý
7. Giriþ yap ? Galeri
```

## ?? Spring Boot API Beklenen Format

### Register Endpoint
```java
@PostMapping("/auth/register")
public ResponseEntity<MessageResponse> register(@RequestBody RegisterRequest request) {
    // request.getUsername() - ? YENÝ
    // request.getEmail()
    // request.getSurname()
    // request.getPassword()
    
    // 1. Username unique kontrolü
    if (userRepository.existsByUsername(request.getUsername())) {
        throw new BadRequestException("Username already taken");
    }
    
    // 2. Email unique kontrolü
    if (userRepository.existsByEmail(request.getEmail())) {
        throw new BadRequestException("Email already registered");
    }
    
    // 3. User oluþtur
    User user = new User();
    user.setUsername(request.getUsername());  // ? YENÝ
    user.setEmail(request.getEmail());
    user.setSurname(request.getSurname());
    user.setPassword(passwordEncoder.encode(request.getPassword()));
    user.setEnabled(false);
    user.setVerificationCode(generateVerificationCode());
    
    // 4. Kaydet
    userRepository.save(user);
    
    // 5. Default role ata
    Role userRole = roleRepository.findByName("ROLE_USER");
    user.getRoles().add(userRole);
    
    // 6. Email gönder
    emailService.sendVerificationEmail(
        user.getEmail(), 
        user.getVerificationCode()
    );
    
    return ResponseEntity.ok(new MessageResponse("Registration successful"));
}
```

### RegisterRequest.java
```java
@Data
public class RegisterRequest {
    @NotBlank
    @Size(min = 3, max = 255)
    private String username;  // ? YENÝ
    
    @NotBlank
    @Email
    private String email;
    
    @NotBlank
    private String surname;
    
    @NotBlank
    @Size(min = 6)
    private String password;
}
```

### User Entity
```java
@Entity
@Table(name = "users")
public class User {
    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Integer id;
    
    @Column(unique = true, nullable = false)
    private String username;  // ? YENÝ
    
    @Column(unique = true, nullable = false)
    private String email;
    
    private String surname;
    
    @Column(nullable = false)
    private String password;
    
    private String verificationCode;
    
    @Column(nullable = false)
    private Boolean enabled = false;
    
    @ManyToMany(fetch = FetchType.EAGER)
    @JoinTable(
        name = "user_roles",
        joinColumns = @JoinColumn(name = "user_id"),
        inverseJoinColumns = @JoinColumn(name = "role_id")
    )
    private Set<Role> roles = new HashSet<>();
}
```

## ? Kontrol Listesi

- [x] AuthRequestDTO'da RegisterRequestDTO ayrýldý
- [x] RegisterRequestDTO'ya username eklendi
- [x] UserDTO'ya username eklendi
- [x] SpringApiClient.Register() metoduna username eklendi
- [x] AuthController.Register() metoduna username eklendi
- [x] Register view'ýna username input eklendi
- [x] AppUser modelinde username zaten vardý ?
- [x] Dokümantasyon güncellendi
- [ ] Spring Boot User entity'sine username eklendi
- [ ] Spring Boot RegisterRequest'e username eklendi
- [ ] Spring Boot'ta username unique constraint eklendi
- [ ] Spring Boot'ta username validation eklendi

## ?? Sonuç

Username alaný baþarýyla eklendi! Artýk:

? Kullanýcýlar kayýt olurken username belirtmeli
? Username benzersiz olmalý
? Username veritabanýnda saklanýyor
? Username API response'larýnda dönüyor
? Frontend tarafý hazýr

Sadece Spring Boot tarafýnda ilgili deðiþiklikleri yapmanýz gerekiyor!
