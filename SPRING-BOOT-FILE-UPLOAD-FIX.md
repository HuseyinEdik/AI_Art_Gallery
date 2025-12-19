# Spring Boot 413 Payload Too Large Hatasý - Çözüm

## ? MVC Tarafý Güncellemeleri Tamamlandý

### Yapýlan Deðiþiklikler:

1. **Program.cs**
   - HttpClient timeout: 5 dakika
   - MaxRequestContentBufferSize: 10 MB
   
2. **appsettings.json**
   - MaxRequestBodySize: 50 MB
   - RequestHeadersTimeout: 5 dakika
   
3. **SpringApiClient.cs**
   - Detaylý logging eklendi
   - Dosya boyutu kontrolü
   - 413 hatasý için özel mesaj

4. **ArtworkController.cs**
   - RequestSizeLimit: 10 MB
   - Server-side dosya boyutu kontrolü

5. **Create.cshtml**
   - JavaScript dosya boyutu kontrolü (10 MB)

## ?? Spring Boot Tarafýnda Yapýlmasý Gerekenler

### 1. application.properties

```properties
# Multipart dosya upload ayarlarý
spring.servlet.multipart.enabled=true
spring.servlet.multipart.max-file-size=50MB
spring.servlet.multipart.max-request-size=50MB

# Tomcat ayarlarý
server.tomcat.max-swallow-size=50MB
server.tomcat.max-http-post-size=50MB

# Connection timeout
server.connection-timeout=300000

# Spring MVC
spring.mvc.async.request-timeout=300000
```

### 2. application.yml

```yaml
spring:
  servlet:
    multipart:
      enabled: true
      max-file-size: 50MB
      max-request-size: 50MB
      
server:
  tomcat:
    max-swallow-size: 50MB
    max-http-post-size: 50MB
  connection-timeout: 300000
  
  mvc:
    async:
      request-timeout: 300000
```

### 3. SecurityConfig (CORS için)

```java
@Bean
public CorsConfigurationSource corsConfigurationSource() {
    CorsConfiguration configuration = new CorsConfiguration();
    configuration.setAllowedOrigins(Arrays.asList("http://localhost:5000", "http://localhost:5220"));
    configuration.setAllowedMethods(Arrays.asList("GET", "POST", "PUT", "DELETE", "OPTIONS"));
    configuration.setAllowedHeaders(Arrays.asList("*"));
    configuration.setAllowCredentials(true);
    configuration.setMaxAge(3600L);
    configuration.setExposedHeaders(Arrays.asList("Content-Length", "Content-Type"));
    
    UrlBasedCorsConfigurationSource source = new UrlBasedCorsConfigurationSource();
    source.registerCorsConfiguration("/**", configuration);
    return source;
}
```

### 4. ArtController

```java
@PostMapping(value = "/create", consumes = MediaType.MULTIPART_FORM_DATA_VALUE)
public ResponseEntity<ArtDTO> createArt(
        @RequestParam("title") String title,
        @RequestParam("promptText") String promptText,
        @RequestParam("imageFile") MultipartFile imageFile,
        @RequestParam(value = "categoryIds", required = false) List<Integer> categoryIds,
        @AuthenticationPrincipal UserDetails userDetails) {
    
    // Dosya boyutu kontrolü
    if (imageFile.getSize() > 50 * 1024 * 1024) {
        throw new MaxUploadSizeExceededException(imageFile.getSize());
    }
    
    // Dosya tipi kontrolü
    String contentType = imageFile.getContentType();
    if (contentType == null || !contentType.startsWith("image/")) {
        throw new IllegalArgumentException("Only image files are allowed");
    }
    
    log.info("Creating artwork: title={}, size={} MB, categories={}", 
        title, imageFile.getSize() / (1024.0 * 1024.0), categoryIds);
    
    ArtDTO art = artService.createArt(title, promptText, imageFile, categoryIds, userDetails.getUsername());
    
    return ResponseEntity.ok(art);
}
```

### 5. GlobalExceptionHandler

```java
@RestControllerAdvice
public class GlobalExceptionHandler {
    
    @ExceptionHandler(MaxUploadSizeExceededException.class)
    public ResponseEntity<ErrorResponse> handleMaxSizeException(MaxUploadSizeExceededException ex) {
        return ResponseEntity.status(HttpStatus.PAYLOAD_TOO_LARGE)
            .body(new ErrorResponse("File size exceeds maximum limit of 50 MB"));
    }
    
    @ExceptionHandler(IllegalArgumentException.class)
    public ResponseEntity<ErrorResponse> handleIllegalArgument(IllegalArgumentException ex) {
        return ResponseEntity.badRequest()
            .body(new ErrorResponse(ex.getMessage()));
    }
}
```

## ?? Test Adýmlarý

### 1. Spring Boot Loglarýný Kontrol Et

```
2025-12-17 ... : Creating artwork: title=Test, size=2.45 MB, categories=[1, 2]
2025-12-17 ... : Saving file: test-image.jpg
2025-12-17 ... : Artwork created successfully with ID: 123
```

### 2. MVC Loglarýný Kontrol Et

```
=== CREATE ARTWORK ===
Title: Test
Image File: test.jpg (2570240 bytes / 2.45 MB)
Category IDs: 1, 2
Total form parts: title, promptText, imageFile, 2 categories
Sending POST request to arts/create...
Response received with status: OK
Artwork created successfully with ID: 123
```

### 3. Browser Console'u Kontrol Et

```
File selected: test.jpg
File size: 2.45 MB
=== FORM SUBMIT ===
Submitting with categories: 1, 2
```

## ?? Debug Checklist

### MVC Tarafý:
- [ ] HttpClient timeout 5 dakika mý? ? Program.cs
- [ ] Kestrel limits 50 MB mý? ? appsettings.json
- [ ] Controller'da RequestSizeLimit var mý? ? ArtworkController.cs
- [ ] JavaScript dosya kontrolü çalýþýyor mu? ? Create.cshtml

### Spring Boot Tarafý:
- [ ] `spring.servlet.multipart.max-file-size=50MB` ?
- [ ] `spring.servlet.multipart.max-request-size=50MB` ?
- [ ] `server.tomcat.max-swallow-size=50MB` ?
- [ ] CORS yapýlandýrmasý doðru mu? ?
- [ ] Controller `consumes = MediaType.MULTIPART_FORM_DATA_VALUE` ?
- [ ] Spring Boot yeniden baþlatýldý mý? ?

## ?? Yaygýn Hatalar

### 1. Spring Boot Restart Edilmedi
```bash
# Spring Boot'u yeniden baþlat
./mvnw spring-boot:run
# veya
./gradlew bootRun
```

### 2. application.properties Okumuyor
```java
// Logla ve kontrol et
@Value("${spring.servlet.multipart.max-file-size}")
private String maxFileSize;

@PostConstruct
public void init() {
    log.info("Max file size: {}", maxFileSize);
}
```

### 3. Tomcat vs Undertow
Eðer Undertow kullanýyorsan:
```properties
server.undertow.max-http-post-size=50MB
```

### 4. Nginx/Reverse Proxy Varsa
```nginx
client_max_body_size 50M;
```

## ?? Dosya Boyutu Testi

Farklý boyutlarda test edin:

| Dosya Boyutu | Beklenen Sonuç |
|--------------|----------------|
| 1 MB | ? Baþarýlý |
| 5 MB | ? Baþarýlý |
| 10 MB | ? Baþarýlý (MVC limiti) |
| 15 MB | ?? MVC tarafýnda reddedilir |
| 50 MB | ?? Spring Boot limiti |
| 51 MB | ? Reddedilir |

## ?? Sonuç

1. **MVC limiti: 10 MB** (kullanýcý tarafýnda kontrol)
2. **Spring Boot limiti: 50 MB** (server güvenliði)
3. **Ýletiþim timeout: 5 dakika** (büyük dosyalar için)

Tüm ayarlarý yaptýktan sonra:
1. Spring Boot'u yeniden baþlat
2. MVC'yi yeniden baþlat
3. 5 MB'lýk bir görsel ile test et
4. Log'larý kontrol et

## ?? Referanslar

- [Spring Boot File Upload](https://spring.io/guides/gs/uploading-files/)
- [ASP.NET Core File Upload](https://docs.microsoft.com/en-us/aspnet/core/mvc/models/file-uploads)
- [HttpClient Configuration](https://docs.microsoft.com/en-us/dotnet/api/system.net.http.httpclient)
