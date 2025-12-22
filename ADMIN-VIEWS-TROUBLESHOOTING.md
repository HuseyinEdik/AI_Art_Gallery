# Admin Database Views - Sorun Giderme

## ?? Sorun
MVC'den admin database view'larýna istek atýlýyor ama Spring Boot 404 döndürüyor:
```
GET http://localhost:8080/api/admin/views/category-stats ? 404 Not Found
```

## ? Kontrol Listesi

### 1. Spring Boot Controller Var mý?

**Dosya:** `src/main/java/com/yourpackage/controller/AdminController.java`

```bash
# Dosya var mý kontrol et
ls src/main/java/com/yourpackage/controller/AdminController.java

# Yoksa oluþtur:
```

```java
package com.yourpackage.controller;

import lombok.RequiredArgsConstructor;
import lombok.extern.slf4j.Slf4j;
import org.springframework.http.ResponseEntity;
import org.springframework.jdbc.core.JdbcTemplate;
import org.springframework.security.access.prepost.PreAuthorize;
import org.springframework.web.bind.annotation.*;

import java.sql.Timestamp;
import java.time.LocalDateTime;
import java.util.List;
import java.util.Map;

@Slf4j
@RestController
@RequestMapping("/api/admin")
@RequiredArgsConstructor
@PreAuthorize("hasRole('ADMIN')")
public class AdminController {
    
    private final JdbcTemplate jdbcTemplate;
    
    @GetMapping("/views/{viewName}")
    public ResponseEntity<?> getDatabaseView(@PathVariable String viewName) {
        log.info("=== GET DATABASE VIEW ===");
        log.info("View Name: {}", viewName);
        
        try {
            List<?> result = switch (viewName) {
                case "detailed-arts" -> getDetailedArts();
                case "category-stats" -> getCategoryStats();
                case "active-users" -> getActiveUsers();
                case "recent-uploads" -> getRecentUploads();
                case "log-summary" -> getLogSummary();
                default -> {
                    log.warn("Unknown view: {}", viewName);
                    yield List.of();
                }
            };
            
            log.info("Returning {} records", result.size());
            return ResponseEntity.ok(result);
            
        } catch (Exception e) {
            log.error("Error fetching view: {}", viewName, e);
            return ResponseEntity.internalServerError()
                    .body(Map.of("error", e.getMessage()));
        }
    }
    
    private List<Map<String, Object>> getDetailedArts() {
        String sql = """
            SELECT 
                a.id,
                a.title,
                c.name as categoryName,
                u.username as ownerName
            FROM arts a
            LEFT JOIN categories c ON a.category_id = c.id
            LEFT JOIN users u ON a.user_id = u.id
            ORDER BY a.id DESC
        """;
        
        return jdbcTemplate.queryForList(sql);
    }
    
    private List<Map<String, Object>> getCategoryStats() {
        String sql = """
            SELECT 
                c.name,
                COUNT(a.id) as totalArts
            FROM categories c
            LEFT JOIN arts a ON c.id = a.category_id
            GROUP BY c.id, c.name
            ORDER BY totalArts DESC
        """;
        
        return jdbcTemplate.queryForList(sql);
    }
    
    private List<Map<String, Object>> getActiveUsers() {
        String sql = """
            SELECT 
                u.username,
                COUNT(a.id) as uploadCount
            FROM users u
            LEFT JOIN arts a ON u.id = a.user_id
            GROUP BY u.id, u.username
            HAVING COUNT(a.id) > 0
            ORDER BY uploadCount DESC
            LIMIT 10
        """;
        
        return jdbcTemplate.queryForList(sql);
    }
    
    private List<Map<String, Object>> getRecentUploads() {
        String sql = """
            SELECT 
                title,
                created_at as createdAt
            FROM arts
            ORDER BY created_at DESC
            LIMIT 10
        """;
        
        return jdbcTemplate.queryForList(sql);
    }
    
    private List<Map<String, Object>> getLogSummary() {
        try {
            String sql = """
                SELECT 
                    log_message as logMessage,
                    created_at as createdAt
                FROM logs
                ORDER BY created_at DESC
                LIMIT 100
            """;
            
            return jdbcTemplate.queryForList(sql);
        } catch (Exception e) {
            log.warn("Logs table not found");
            return List.of();
        }
    }
}
```

### 2. SecurityConfig'de Ýzin Var mý?

**Dosya:** `src/main/java/com/yourpackage/config/SecurityConfig.java`

```java
@Bean
public SecurityFilterChain securityFilterChain(HttpSecurity http) throws Exception {
    http
        // ...
        .authorizeHttpRequests(auth -> auth
            // Public
            .requestMatchers("/api/auth/**").permitAll()
            .requestMatchers(HttpMethod.GET, "/api/arts/public").permitAll()
            
            // ?? BU SATIR OLMALIDIR
            .requestMatchers("/api/admin/**").hasRole("ADMIN")
            
            // Authenticated
            .requestMatchers("/api/arts/**").authenticated()
            
            .anyRequest().authenticated()
        );
    
    return http.build();
}
```

### 3. Spring Boot Baþlatýldý mý?

```bash
# Spring Boot'u baþlat
./mvnw spring-boot:run

# veya
./gradlew bootRun

# Baþlatma log'larýnda þunu ara:
# Mapped "{[/api/admin/views/{viewName}],methods=[GET]}"
```

### 4. Endpoint Test Et

**Postman veya cURL:**

```bash
# 1. Admin token al
curl -X POST http://localhost:8080/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@example.com","password":"admin123"}'

# Response:
# {"token":"eyJhbGc...","roles":["ROLE_ADMIN"]}

# 2. View'ý test et
curl -X GET http://localhost:8080/api/admin/views/category-stats \
  -H "Authorization: Bearer TOKEN_BURAYA"

# Beklenen: 200 OK
# [{"name":"Manzara","totalArts":5}]

# Hata alýyorsan: 404 = Controller yok, 403 = Admin deðilsin
```

### 5. Database Tablolarý Var mý?

```sql
-- PostgreSQL'de kontrol et
SELECT * FROM arts LIMIT 1;
SELECT * FROM categories LIMIT 1;
SELECT * FROM users LIMIT 1;

-- Yoksa hata verir:
-- ERROR: relation "arts" does not exist
```

### 6. Tablo Ýsimleri Doðru mu?

Spring Boot'ta tablo isimleri **küçük harfle** ve **snake_case** olmalý:

```java
// ? YANLIÞ
FROM Arts a              // "Arts" yerine "arts" olmalý
FROM art_categories ac   // Tablo ismi yanlýþ olabilir

// ? DOÐRU
FROM arts a
FROM categories c
```

## ?? Tam Test Senaryosu

### 1. Spring Boot Log'larýný Aç

```bash
tail -f logs/spring-boot-application.log

# veya IDE'de console'u izle
```

### 2. MVC'den Ýstek At

MVC uygulamasýnda:
1. Admin olarak login ol
2. Admin Panel ? Database Views

### 3. Spring Boot Log'larýnda Ara

? **Baþarýlý:**
```
=== GET DATABASE VIEW ===
View Name: category-stats
Returning 3 records
```

? **Hatalý (Controller yok):**
```
(hiçbir log yok - 404 NotFound)
```

? **Hatalý (SQL hatasý):**
```
=== GET DATABASE VIEW ===
View Name: category-stats
Error fetching view: relation "categories" does not exist
```

? **Hatalý (Admin deðil):**
```
Access Denied: User does not have ROLE_ADMIN
```

## ?? Hýzlý Çözümler

### Çözüm 1: Basit Controller (DTO Olmadan)

Map kullanarak hýzlýca çalýþtýr:

```java
@RestController
@RequestMapping("/api/admin")
@RequiredArgsConstructor
public class AdminController {
    
    private final JdbcTemplate jdbcTemplate;
    
    @GetMapping("/test")
    public ResponseEntity<?> test() {
        return ResponseEntity.ok(Map.of("status", "OK", "message", "Admin endpoint works!"));
    }
    
    @GetMapping("/views/category-stats")
    public ResponseEntity<?> getCategoryStats() {
        String sql = "SELECT name, COUNT(*) as count FROM categories GROUP BY name";
        List<Map<String, Object>> result = jdbcTemplate.queryForList(sql);
        return ResponseEntity.ok(result);
    }
}
```

Test:
```
GET http://localhost:8080/api/admin/test
GET http://localhost:8080/api/admin/views/category-stats
```

### Çözüm 2: SecurityConfig'i Devre Dýþý Býrak (Geçici)

**SADECE TEST ÝÇÝN:**

```java
@Bean
public SecurityFilterChain securityFilterChain(HttpSecurity http) throws Exception {
    http
        .csrf(csrf -> csrf.disable())
        .authorizeHttpRequests(auth -> auth
            .anyRequest().permitAll()  // ? Tüm endpoint'lere izin ver (TEHLÝKELÝ!)
        );
    
    return http.build();
}
```

Eðer bu çalýþýrsa ? Sorun Security'de
Eðer hala 404 alýyorsan ? Controller yüklenmemiþ

### Çözüm 3: Debug Mode'da Baþlat

```bash
./mvnw spring-boot:run -Ddebug

# Log'larda arama yap:
# "Mapped" kelimesini ara - tüm endpoint'leri gösterir
# "/api/admin" olmalý
```

## ?? Özet

1. ? `AdminController.java` oluþtur
2. ? `@PreAuthorize` ekle veya Security'de izin ver
3. ? Spring Boot'u yeniden baþlat
4. ? Postman ile test et
5. ? MVC'den tekrar dene

## ?? Hala Çalýþmýyorsa

1. **Tam log'larý paylaþ:**
   - Spring Boot baþlatma log'larý
   - Ýstek sýrasýndaki log'lar

2. **Postman screenshot'u:**
   - Request (URL, Headers, Auth)
   - Response (Status, Body)

3. **Dosya yapýsý:**
```
src/main/java/com/yourpackage/
??? controller/
?   ??? AdminController.java      ? VAR MI?
?   ??? ArtController.java
?   ??? AuthController.java
??? config/
?   ??? SecurityConfig.java       ? KONTROL ET
??? Application.java
```

**AdminController.java'yý yukarýdaki gibi oluþturup Spring Boot'u yeniden baþlatýn!** ??
