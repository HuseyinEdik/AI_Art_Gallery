# Spring Boot 405 Method Not Allowed Hatasý - Çözüm

## ? Hata
```
Response status code does not indicate success: 405 (Method Not Allowed)
```

## ?? Sorun Analizi

405 hatasý þu sebeplerden kaynaklanýr:
1. Controller'da POST mapping yok
2. Endpoint path'i yanlýþ
3. CORS preflight (OPTIONS) baþarýsýz
4. Security filter POST'u engelliyor

## ? Spring Boot Kontrol Listesi

### 1. ArtController - POST Mapping Kontrolü

```java
@RestController
@RequestMapping("/api/arts")  // ? /api prefix'i OLMALI
public class ArtController {
    
    @PostMapping(value = "/create", consumes = MediaType.MULTIPART_FORM_DATA_VALUE)
    // ? consumes önemli! Multipart form data kabul etmeli
    public ResponseEntity<ArtDTO> createArt(
            @RequestParam("title") String title,
            @RequestParam("promptText") String promptText,
            @RequestParam("imageFile") MultipartFile imageFile,
            @RequestParam(value = "categoryIds", required = false) List<Integer> categoryIds,
            @AuthenticationPrincipal UserDetails userDetails) {
        
        log.info("=== CREATE ART REQUEST ===");
        log.info("Title: {}", title);
        log.info("PromptText: {}", promptText);
        log.info("ImageFile: {} ({} bytes)", imageFile.getOriginalFilename(), imageFile.getSize());
        log.info("CategoryIds: {}", categoryIds);
        log.info("User: {}", userDetails.getUsername());
        
        ArtDTO art = artService.createArt(title, promptText, imageFile, categoryIds, userDetails.getUsername());
        
        return ResponseEntity.ok(art);
    }
}
```

**Önemli Noktalar:**
- ? `@PostMapping` kullanýlmalý (`@GetMapping` deðil!)
- ? `consumes = MediaType.MULTIPART_FORM_DATA_VALUE` olmalý
- ? Path: `/api/arts/create` (BaseURL: `http://localhost:8080`)
- ? Full URL: `http://localhost:8080/api/arts/create`

### 2. SecurityConfig - POST Ýzni

```java
@Bean
public SecurityFilterChain securityFilterChain(HttpSecurity http) throws Exception {
    http
        .csrf(csrf -> csrf.disable())  // ? CSRF disabled olmalý
        .cors(cors -> cors.configurationSource(corsConfigurationSource()))
        .authorizeHttpRequests(auth -> auth
            // Public endpoints
            .requestMatchers("/api/auth/**").permitAll()
            .requestMatchers(HttpMethod.GET, "/api/arts/public").permitAll()
            .requestMatchers(HttpMethod.GET, "/api/categories").permitAll()
            
            // Authenticated endpoints
            .requestMatchers(HttpMethod.POST, "/api/arts/create").authenticated()  // ? BU SATIR
            .requestMatchers(HttpMethod.POST, "/api/arts/**").authenticated()
            .requestMatchers("/api/interactions/**").authenticated()
            
            .anyRequest().authenticated()
        )
        .sessionManagement(session -> session
            .sessionCreationPolicy(SessionCreationPolicy.STATELESS)
        )
        .addFilterBefore(jwtAuthFilter, UsernamePasswordAuthenticationFilter.class);
    
    return http.build();
}
```

### 3. CORS Configuration - OPTIONS Ýzni

```java
@Bean
public CorsConfigurationSource corsConfigurationSource() {
    CorsConfiguration configuration = new CorsConfiguration();
    
    configuration.setAllowedOrigins(Arrays.asList(
        "http://localhost:5000",
        "http://localhost:5220",
        "http://localhost:3000"
    ));
    
    configuration.setAllowedMethods(Arrays.asList(
        "GET", "POST", "PUT", "DELETE", "OPTIONS", "PATCH"  // ? OPTIONS önemli!
    ));
    
    configuration.setAllowedHeaders(Arrays.asList("*"));
    configuration.setAllowCredentials(true);
    configuration.setMaxAge(3600L);
    
    // Expose headers for multipart upload
    configuration.setExposedHeaders(Arrays.asList(
        "Content-Length", 
        "Content-Type",
        "Authorization"
    ));
    
    UrlBasedCorsConfigurationSource source = new UrlBasedCorsConfigurationSource();
    source.registerCorsConfiguration("/**", configuration);
    return source;
}
```

### 4. JwtAuthenticationFilter - Multipart Support

```java
@Component
public class JwtAuthenticationFilter extends OncePerRequestFilter {

    private static final List<String> PUBLIC_PATHS = Arrays.asList(
        "/api/auth/register",
        "/api/auth/login",
        "/api/auth/verify",
        "/api/arts/public",
        "/api/categories"
    );

    @Override
    protected void doFilterInternal(...) throws ServletException, IOException {
        
        String requestPath = request.getServletPath();
        
        // Skip JWT for public paths
        if (isPublicPath(requestPath)) {
            filterChain.doFilter(request, response);
            return;
        }

        // Handle OPTIONS requests (CORS preflight)
        if ("OPTIONS".equals(request.getMethod())) {
            filterChain.doFilter(request, response);
            return;
        }

        // JWT validation logic...
        String authHeader = request.getHeader("Authorization");
        
        if (authHeader == null || !authHeader.startsWith("Bearer ")) {
            filterChain.doFilter(request, response);
            return;
        }

        // Token validation...
        filterChain.doFilter(request, response);
    }
}
```

## ?? Test Adýmlarý

### 1. Postman ile Test

```
POST http://localhost:8080/api/arts/create
Headers:
  Authorization: Bearer YOUR_JWT_TOKEN
  Content-Type: multipart/form-data
  
Body (form-data):
  title: Test Artwork
  promptText: A beautiful landscape
  imageFile: [SELECT FILE]
  categoryIds: 1
  categoryIds: 2

Beklenen: 200 OK
```

### 2. cURL ile Test

```bash
curl -X POST http://localhost:8080/api/arts/create \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -F "title=Test" \
  -F "promptText=Test prompt" \
  -F "imageFile=@/path/to/image.jpg" \
  -F "categoryIds=1" \
  -F "categoryIds=2" \
  -v
```

### 3. Browser Console Test

```javascript
const formData = new FormData();
formData.append('title', 'Test');
formData.append('promptText', 'Test prompt');
formData.append('imageFile', fileInput.files[0]);
formData.append('categoryIds', '1');
formData.append('categoryIds', '2');

fetch('http://localhost:8080/api/arts/create', {
    method: 'POST',
    headers: {
        'Authorization': 'Bearer ' + token
    },
    body: formData
})
.then(res => res.json())
.then(data => console.log(data))
.catch(err => console.error(err));
```

## ?? Debug

### Spring Boot Loglarýnda Bakýlacaklar

```
# DOÐRU - 200 OK
POST /api/arts/create
Content-Type: multipart/form-data
Authorization: Bearer eyJ...
Status: 200 OK

# YANLIÞ - 405
POST /api/arts/create
Status: 405 Method Not Allowed
Allow: GET  ? Bu satýr varsa controller'da @PostMapping eksik

# YANLIÞ - 403
POST /api/arts/create
Status: 403 Forbidden  ? SecurityConfig'de izin yok

# YANLIÞ - 401
POST /api/arts/create
Status: 401 Unauthorized  ? JWT token geçersiz
```

### MVC Log'larýnda Bakýlacaklar

```
=== CREATE ARTWORK ===
Title: Test
Image File: test.jpg (5242880 bytes / 5.00 MB)
Category IDs: 1, 2
Sending POST request to arts/create...
Full URL: http://localhost:8080/api/arts/create  ? Bu URL'yi kontrol et

=== API RESPONSE ===
Status Code: MethodNotAllowed (405)
Response Body: {"timestamp":"...","status":405,"error":"Method Not Allowed","path":"/api/arts/create"}
Response Headers:
  Allow: GET  ? Eðer bu varsa controller'da @PostMapping yok!
```

## ?? Yaygýn Hatalar

### 1. Controller'da GetMapping Kullanýlmýþ

```java
// ? YANLIÞ
@GetMapping("/create")
public ResponseEntity<ArtDTO> createArt(...) { }

// ? DOÐRU
@PostMapping("/create")
public ResponseEntity<ArtDTO> createArt(...) { }
```

### 2. RequestMapping Yanlýþ

```java
// ? YANLIÞ - /api eksik
@RequestMapping("/arts")

// ? DOÐRU
@RequestMapping("/api/arts")
```

### 3. Consumes Eksik

```java
// ? YANLIÞ - multipart kabul etmiyor
@PostMapping("/create")

// ? DOÐRU
@PostMapping(value = "/create", consumes = MediaType.MULTIPART_FORM_DATA_VALUE)
```

### 4. CSRF Aktif

```java
// ? YANLIÞ - CSRF token isteniyor
http.csrf()  // default enabled

// ? DOÐRU
http.csrf(csrf -> csrf.disable())
```

### 5. OPTIONS Engellenmiþ

```java
// ? YANLIÞ - OPTIONS request'i reddediliyor
.requestMatchers("/**").authenticated()

// ? DOÐRU - OPTIONS'a izin ver
if ("OPTIONS".equals(request.getMethod())) {
    filterChain.doFilter(request, response);
    return;
}
```

## ?? Kontrol Listesi

Spring Boot tarafýnda:
- [ ] `@PostMapping("/create")` var mý?
- [ ] `consumes = MediaType.MULTIPART_FORM_DATA_VALUE` var mý?
- [ ] `@RequestMapping("/api/arts")` doðru mu?
- [ ] SecurityConfig'de `.requestMatchers(HttpMethod.POST, "/api/arts/create").authenticated()` var mý?
- [ ] CSRF disabled mý?
- [ ] CORS yapýlandýrmasý doðru mu?
- [ ] OPTIONS request'ine izin veriliyor mu?
- [ ] JWT filter multipart'ý engelliyor mu?

MVC tarafýnda:
- [ ] URL: `http://localhost:8080/api/arts/create` (log'larda kontrol et)
- [ ] Method: POST (log'larda kontrol et)
- [ ] Authorization header ekleniyor mu?
- [ ] MultipartFormDataContent doðru oluþturulmuþ mu?

## ?? Hýzlý Fix

**1. ArtController.java:**
```java
@PostMapping(value = "/create", consumes = MediaType.MULTIPART_FORM_DATA_VALUE)
```

**2. SecurityConfig.java:**
```java
.requestMatchers(HttpMethod.POST, "/api/arts/**").authenticated()
```

**3. JwtAuthenticationFilter.java:**
```java
if ("OPTIONS".equals(request.getMethod())) {
    filterChain.doFilter(request, response);
    return;
}
```

**4. Spring Boot'u yeniden baþlat**

Bu deðiþiklikleri yaptýktan sonra tekrar deneyin!
