# ?? Hýzlý Çözüm: Spring Boot 403 Hatasý

## ? Hata
```
POST /auth/register ? 403 Forbidden
o.s.s.w.a.Http403ForbiddenEntryPoint: Pre-authenticated entry point called. Rejecting access
```

## ? 5 Dakikada Çözüm

### Adým 1: SecurityConfig.java Güncellemesi

**Dosya:** `src/main/java/com/yourpackage/config/SecurityConfig.java`

```java
@Configuration
@EnableWebSecurity
public class SecurityConfig {

    @Bean
    public SecurityFilterChain securityFilterChain(HttpSecurity http) throws Exception {
        http
            .csrf(csrf -> csrf.disable())  // ? ÖNEMLÝ!
            .cors(cors -> cors.configurationSource(corsConfigurationSource()))
            .authorizeHttpRequests(auth -> auth
                // PUBLIC - Sýralama ÖNEMLÝ! Spesifik path'ler önce
                .requestMatchers("/auth/register").permitAll()
                .requestMatchers("/auth/login").permitAll()
                .requestMatchers("/auth/verify").permitAll()
                .requestMatchers("/auth/resend-verification").permitAll()
                .requestMatchers(HttpMethod.GET, "/arts/public").permitAll()
                .requestMatchers(HttpMethod.GET, "/arts/{id}").permitAll()
                .requestMatchers(HttpMethod.GET, "/categories").permitAll()
                .requestMatchers("/error").permitAll()
                // AUTHENTICATED
                .anyRequest().authenticated()
            )
            .sessionManagement(session -> session
                .sessionCreationPolicy(SessionCreationPolicy.STATELESS)
            );

        return http.build();
    }

    @Bean
    public CorsConfigurationSource corsConfigurationSource() {
        CorsConfiguration config = new CorsConfiguration();
        config.setAllowedOrigins(Arrays.asList("http://localhost:5000", "http://localhost:5220"));
        config.setAllowedMethods(Arrays.asList("*"));
        config.setAllowedHeaders(Arrays.asList("*"));
        config.setAllowCredentials(true);
        
        UrlBasedCorsConfigurationSource source = new UrlBasedCorsConfigurationSource();
        source.registerCorsConfiguration("/**", config);
        return source;
    }
}
```

### Adým 2: JwtAuthenticationFilter (Eðer Varsa)

**Dosya:** `src/main/java/com/yourpackage/config/JwtAuthenticationFilter.java`

```java
@Component
public class JwtAuthenticationFilter extends OncePerRequestFilter {

    private static final List<String> PUBLIC_PATHS = Arrays.asList(
        "/auth/register", "/auth/login", "/auth/verify", 
        "/auth/resend-verification", "/arts/public", "/categories"
    );

    @Override
    protected boolean shouldNotFilter(HttpServletRequest request) {
        String path = request.getServletPath();
        return PUBLIC_PATHS.stream().anyMatch(p -> 
            path.equals(p) || path.startsWith(p + "/")
        );
    }

    @Override
    protected void doFilterInternal(...) {
        // Public path'ler zaten filtered, buraya gelmez
        // JWT validation logic...
    }
}
```

### Adým 3: application.properties

```properties
# Debug için (opsiyonel)
logging.level.org.springframework.security=DEBUG
```

### Adým 4: Spring Boot'u Yeniden Baþlat

```bash
# Maven
./mvnw spring-boot:run

# Gradle
./gradlew bootRun

# IDE
Stop ? Run
```

## ?? Test

### Postman
```
POST http://localhost:8080/auth/register
Content-Type: application/json

{
    "username": "testuser",
    "email": "test@example.com",
    "surname": "Test",
    "password": "123456"
}

? Beklenen: 200 OK
```

### cURL
```bash
curl -X POST http://localhost:8080/auth/register \
  -H "Content-Type: application/json" \
  -d '{"username":"testuser","email":"test@example.com","surname":"Test","password":"123456"}'
```

## ? Baþarý Göstergeleri

### Log'larda görmeli:
```
? Securing POST /auth/register
? Matched permitAll for /auth/register
? Successfully registered user: test@example.com
```

### Log'larda görmemeli:
```
? Pre-authenticated entry point called. Rejecting access
? Access is denied
? 403 Forbidden
```

## ?? Hala Çalýþmýyorsa?

### Quick Fix (Geçici - Sadece Test Ýçin)
SecurityConfig'e ekle:
```java
@Bean
public WebSecurityCustomizer webSecurityCustomizer() {
    return web -> web.ignoring()
        .requestMatchers("/auth/**", "/arts/public", "/categories");
}
```

Bu, Spring Security'yi tamamen bypass eder. ?? **Production'da kullanma!**

## ?? Checklist

- [ ] CSRF disabled
- [ ] CORS yapýlandýrýldý
- [ ] `/auth/register` permitAll() ile iþaretlendi
- [ ] Spesifik path'ler wildcard'lardan önce
- [ ] JwtAuthenticationFilter public path'leri atlýyor (eðer varsa)
- [ ] Spring Boot yeniden baþlatýldý
- [ ] Postman'de test edildi
- [ ] Log'lar kontrol edildi

## ?? Sonuç

Bu deðiþikliklerden sonra:
- ? `/auth/register` ? 200 OK
- ? `/auth/login` ? 200 OK
- ? `/auth/verify` ? 200 OK
- ? `/arts/public` ? 200 OK
- ? `/categories` ? 200 OK

Authenticated endpoint'ler hala korunuyor:
- ?? `/auth/me` ? 401 Unauthorized (token olmadan)
- ?? `/arts/create` ? 401 Unauthorized (token olmadan)

**Bu kadar! Spring Boot'u yeniden baþlat ve test et.** ??
