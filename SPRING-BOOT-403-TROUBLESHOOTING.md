# Spring Boot 403 Forbidden - Troubleshooting Guide

## ?? Hata Detaylarý

```
POST /auth/register ? 403 Forbidden
o.s.s.w.a.Http403ForbiddenEntryPoint: Pre-authenticated entry point called. Rejecting access
```

## ?? Sorunun Kaynaðý

Spring Security filter chain, `/auth/register` endpoint'ini koruma altýna alýyor. Bu genellikle þu sebeplerden kaynaklanýr:

1. **SecurityConfig doðru yapýlandýrýlmamýþ**
2. **JwtAuthenticationFilter public endpoint'leri atlamamýþ**
3. **Filter order yanlýþ ayarlanmýþ**
4. **CORS sorunlarý**

## ? Adým Adým Çözüm

### 1. SecurityConfig.java Kontrolü

**Sorunlu Kod:**
```java
@Bean
public SecurityFilterChain securityFilterChain(HttpSecurity http) throws Exception {
    http
        .authorizeHttpRequests(auth -> auth
            .requestMatchers("/auth/**").permitAll()  // ? Çalýþmýyor
            .anyRequest().authenticated()
        )
        .addFilterBefore(jwtAuthFilter, UsernamePasswordAuthenticationFilter.class);
    return http.build();
}
```

**Doðru Kod:**
```java
@Bean
public SecurityFilterChain securityFilterChain(HttpSecurity http) throws Exception {
    http
        .cors(cors -> cors.configurationSource(corsConfigurationSource()))
        .csrf(csrf -> csrf.disable())
        .authorizeHttpRequests(auth -> auth
            // Specific paths BEFORE wildcards
            .requestMatchers("/auth/register").permitAll()
            .requestMatchers("/auth/login").permitAll()
            .requestMatchers("/auth/verify").permitAll()
            .requestMatchers("/auth/resend-verification").permitAll()
            .requestMatchers(HttpMethod.GET, "/arts/public").permitAll()
            .requestMatchers(HttpMethod.GET, "/arts/{id}").permitAll()
            .requestMatchers(HttpMethod.GET, "/categories").permitAll()
            .requestMatchers("/error").permitAll()
            // Authenticated endpoints
            .requestMatchers("/auth/me").authenticated()
            .requestMatchers("/auth/logout").authenticated()
            .requestMatchers("/arts/**").authenticated()
            .requestMatchers("/interactions/**").authenticated()
            .anyRequest().authenticated()
        )
        .sessionManagement(session -> session
            .sessionCreationPolicy(SessionCreationPolicy.STATELESS)
        )
        .addFilterBefore(jwtAuthFilter, UsernamePasswordAuthenticationFilter.class);

    return http.build();
}
```

**Önemli Noktalar:**
- ? Spesifik path'ler wildcard'lardan ÖNCE olmalý
- ? CSRF disable edilmeli (REST API için)
- ? CORS yapýlandýrýlmalý
- ? SessionCreationPolicy.STATELESS olmalý

### 2. JwtAuthenticationFilter Kontrolü

**Sorunlu Kod:**
```java
@Component
public class JwtAuthenticationFilter extends OncePerRequestFilter {
    @Override
    protected void doFilterInternal(...) throws ServletException, IOException {
        // Tüm istekler için JWT kontrolü yapýyor ?
        final String jwt = authHeader.substring(7);
        // ...
        filterChain.doFilter(request, response);
    }
}
```

**Doðru Kod:**
```java
@Component
public class JwtAuthenticationFilter extends OncePerRequestFilter {

    private static final List<String> PUBLIC_PATHS = Arrays.asList(
        "/auth/register",
        "/auth/login",
        "/auth/verify",
        "/auth/resend-verification",
        "/arts/public",
        "/categories",
        "/error"
    );

    @Override
    protected void doFilterInternal(...) throws ServletException, IOException {
        final String requestPath = request.getServletPath();
        
        // Skip JWT for public endpoints ?
        if (isPublicPath(requestPath)) {
            filterChain.doFilter(request, response);
            return;
        }

        final String authHeader = request.getHeader("Authorization");
        
        if (authHeader == null || !authHeader.startsWith("Bearer ")) {
            filterChain.doFilter(request, response);
            return;
        }

        // JWT validation logic...
        filterChain.doFilter(request, response);
    }

    private boolean isPublicPath(String requestPath) {
        return PUBLIC_PATHS.stream().anyMatch(publicPath -> 
            requestPath.equals(publicPath) || 
            requestPath.startsWith(publicPath + "/")
        );
    }

    @Override
    protected boolean shouldNotFilter(HttpServletRequest request) {
        String path = request.getServletPath();
        return isPublicPath(path);
    }
}
```

### 3. CORS Yapýlandýrmasý

**CorsConfig.java:**
```java
@Configuration
public class CorsConfig implements WebMvcConfigurer {
    
    @Override
    public void addCorsMappings(CorsRegistry registry) {
        registry.addMapping("/**")
            .allowedOrigins(
                "http://localhost:5000",
                "http://localhost:5220",
                "http://localhost:3000"
            )
            .allowedMethods("GET", "POST", "PUT", "DELETE", "OPTIONS", "PATCH")
            .allowedHeaders("*")
            .allowCredentials(true)
            .maxAge(3600);
    }
}
```

**VEYA SecurityConfig içinde:**
```java
@Bean
public CorsConfigurationSource corsConfigurationSource() {
    CorsConfiguration configuration = new CorsConfiguration();
    configuration.setAllowedOrigins(Arrays.asList(
        "http://localhost:5000",
        "http://localhost:5220"
    ));
    configuration.setAllowedMethods(Arrays.asList("*"));
    configuration.setAllowedHeaders(Arrays.asList("*"));
    configuration.setAllowCredentials(true);
    
    UrlBasedCorsConfigurationSource source = new UrlBasedCorsConfigurationSource();
    source.registerCorsConfiguration("/**", configuration);
    return source;
}
```

### 4. Application Properties

**application.properties veya application.yml:**
```properties
# Logging
logging.level.org.springframework.security=DEBUG
logging.level.org.springframework.web=DEBUG
logging.level.com.yourpackage=DEBUG

# CORS (if not using CorsConfig)
spring.web.cors.allowed-origins=http://localhost:5000,http://localhost:5220
spring.web.cors.allowed-methods=GET,POST,PUT,DELETE,OPTIONS
spring.web.cors.allowed-headers=*
spring.web.cors.allow-credentials=true
```

## ?? Test Adýmlarý

### Test 1: Postman ile Register Test
```
POST http://localhost:8080/auth/register
Headers:
  Content-Type: application/json
Body:
{
    "username": "testuser",
    "email": "test@example.com",
    "surname": "Test",
    "password": "123456"
}

Beklenen: 200 OK
Gerçek: 403 Forbidden ?
```

### Test 2: Curl ile Test
```bash
curl -X POST http://localhost:8080/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "username": "testuser",
    "email": "test@example.com",
    "surname": "Test",
    "password": "123456"
  }' -v
```

### Test 3: Spring Boot Loglarý
```
2025-12-16T14:31:37.373+03:00 DEBUG [...] o.s.security.web.FilterChainProxy : Securing POST /auth/register
2025-12-16T14:31:37.400+03:00 DEBUG [...] o.s.s.w.a.Http403ForbiddenEntryPoint : Pre-authenticated entry point called. Rejecting access
```

**Beklenen Log:**
```
DEBUG [...] o.s.security.web.FilterChainProxy : Secured POST /auth/register
DEBUG [...] JwtAuthenticationFilter : Skipping JWT validation for public path: /auth/register
```

## ?? Hýzlý Fix Kontrolü

### Checklist

- [ ] **SecurityConfig** dosyasý var mý?
- [ ] `/auth/register` permitAll() ile iþaretlenmiþ mi?
- [ ] Spesifik path'ler wildcard'lardan önce mi?
- [ ] CSRF disabled mi?
- [ ] CORS yapýlandýrýlmýþ mý?
- [ ] **JwtAuthenticationFilter** public path'leri atlýyor mu?
- [ ] `shouldNotFilter()` metodu var mý?
- [ ] Filter order doðru mu? (JWT filter UsernamePasswordAuthenticationFilter'dan ÖNCE)
- [ ] Spring Boot yeniden baþlatýldý mý?
- [ ] Loglar DEBUG seviyesinde mi?

### Hýzlý Doðrulama Kodu

SecurityConfig'e þunu ekleyin:
```java
@Bean
public WebSecurityCustomizer webSecurityCustomizer() {
    return (web) -> web.ignoring()
        .requestMatchers("/auth/register", "/auth/login", "/auth/verify");
}
```

Bu, Spring Security'yi tamamen bypass eder (sadece test için!).

## ?? Yaygýn Hatalar

### 1. Filter Order Hatasý
```java
// ? YANLIÞ
http.addFilterAfter(jwtAuthFilter, UsernamePasswordAuthenticationFilter.class);

// ? DOÐRU
http.addFilterBefore(jwtAuthFilter, UsernamePasswordAuthenticationFilter.class);
```

### 2. Path Matching Hatasý
```java
// ? YANLIÞ - Wildcard önce
.requestMatchers("/arts/**").authenticated()
.requestMatchers("/arts/public").permitAll()

// ? DOÐRU - Spesifik path önce
.requestMatchers("/arts/public").permitAll()
.requestMatchers("/arts/**").authenticated()
```

### 3. CSRF Token Hatasý
```java
// ? YANLIÞ - CSRF enabled (REST API'de)
http.csrf() // default enabled

// ? DOÐRU
http.csrf(csrf -> csrf.disable())
```

### 4. CORS Hatasý
```java
// ? YANLIÞ - CORS yapýlandýrýlmamýþ
// Frontend: http://localhost:5000 ? Backend: http://localhost:8080
// Result: CORS error + 403

// ? DOÐRU
http.cors(cors -> cors.configurationSource(corsConfigurationSource()))
```

## ?? Debug Komutlarý

### 1. Security Filter Chain'i Göster
```java
@Bean
public FilterChainProxy filterChainProxy() {
    // This will log all security filters
}
```

### 2. Request Matcher Test
```java
@GetMapping("/test-security")
public String testSecurity() {
    Authentication auth = SecurityContextHolder.getContext().getAuthentication();
    return "Is authenticated: " + auth.isAuthenticated();
}
```

## ?? Son Kontrol

Eðer hala çalýþmýyorsa:

1. **application.properties'e ekle:**
```properties
logging.level.org.springframework.security=TRACE
```

2. **Spring Boot'u yeniden baþlat**

3. **Register isteði gönder ve loglarý incele:**
   - Hangi filter'lar çalýþýyor?
   - JwtAuthenticationFilter public path'i atlýyor mu?
   - SecurityFilterChain hangi rule'u uyguluyor?

4. **Postman'de Headers kontrol et:**
   - Content-Type: application/json olmalý
   - Authorization header OLMAMALI (public endpoint)

## ? Baþarý Kriterleri

Register isteði baþarýlý olduðunda:
```
POST /auth/register ? 200 OK
{
    "message": "Registration successful. Verification code sent to email."
}
```

Loglar þöyle olmalý:
```
DEBUG [...] JwtAuthenticationFilter : Skipping JWT for public path: /auth/register
DEBUG [...] SecurityFilterChain : Matched permitAll for /auth/register
DEBUG [...] AuthController : Registering new user: test@example.com
```

## ?? Hala Sorun Varsa

1. SecurityConfig ve JwtAuthenticationFilter kodlarýnýzý paylaþýn
2. Tam log output'unu gönderin
3. Spring Boot ve Spring Security versiyonlarýný belirtin
4. application.properties'i paylaþýn

Bu dosyalardaki kodlarý projenize uygulayýn ve Spring Boot'u yeniden baþlatýn!
