# Database View Ýsimleri Güncellendi

## ? Yapýlan Deðiþiklikler

### MVC Tarafý (AdminController.cs)

**Önceki:** Kebab-case isimleri
```csharp
"category-stats"
"active-users"  
"recent-uploads"
"detailed-arts"
"log-summary"
```

**Yeni:** Database view isimleri
```csharp
"vw_categorystats"
"vw_activeusers"
"vw_recentuploads"
"vw_detailedartlist"
"vw_logsummary"
```

### Spring Boot Tarafý (AdminController.java)

**Güncellenecek dosya:** `src/main/java/com/yourpackage/controller/AdminController.java`

```java
@GetMapping("/views/{viewName}")
public ResponseEntity<?> getDatabaseView(@PathVariable String viewName) {
    log.info("=== GET DATABASE VIEW: {} ===", viewName);
    
    try {
        List<Map<String, Object>> result = switch (viewName) {
            case "vw_categorystats" -> jdbcTemplate.queryForList(
                "SELECT name, COUNT(*) as totalArts " +
                "FROM vw_categorystats " +
                "GROUP BY name ORDER BY totalArts DESC"
            );
            
            case "vw_activeusers" -> jdbcTemplate.queryForList(
                "SELECT username, uploadCount FROM vw_activeusers " +
                "ORDER BY uploadCount DESC LIMIT 10"
            );
            
            case "vw_recentuploads" -> jdbcTemplate.queryForList(
                "SELECT title, created_at as createdAt " +
                "FROM vw_recentuploads " +
                "ORDER BY created_at DESC LIMIT 10"
            );
            
            case "vw_detailedartlist" -> jdbcTemplate.queryForList(
                "SELECT id, title, category_name as categoryName, owner_name as ownerName " +
                "FROM vw_detailedartlist " +
                "ORDER BY id DESC"
            );
            
            case "vw_logsummary" -> jdbcTemplate.queryForList(
                "SELECT log_message as logMessage, created_at as createdAt " +
                "FROM vw_logsummary " +
                "ORDER BY created_at DESC LIMIT 100"
            );
            
            default -> {
                log.warn("Unknown view: {}", viewName);
                yield List.of();
            }
        };
        
        log.info("Returning {} records", result.size());
        return ResponseEntity.ok(result);
        
    } catch (Exception e) {
        log.error("Error: {}", e.getMessage(), e);
        return ResponseEntity.internalServerError()
                .body(Map.of("error", e.getMessage()));
    }
}
```

## ?? Database View'lar

PostgreSQL'de oluþturulmuþ olmasý gereken view'lar:

```sql
-- 1. Kategori Ýstatistikleri
CREATE OR REPLACE VIEW vw_categorystats AS
SELECT c.name, COUNT(a.id) AS total_arts
FROM categories c
LEFT JOIN arts a ON c.id = a.category_id
GROUP BY c.id, c.name;

-- 2. Aktif Kullanýcýlar
CREATE OR REPLACE VIEW vw_activeusers AS
SELECT u.username, COUNT(a.id) AS upload_count
FROM users u
JOIN arts a ON u.id = a.user_id
GROUP BY u.id, u.username
ORDER BY upload_count DESC;

-- 3. Son Yüklemeler
CREATE OR REPLACE VIEW vw_recentuploads AS
SELECT title, created_at 
FROM arts 
ORDER BY created_at DESC 
LIMIT 10;

-- 4. Detaylý Eser Listesi
CREATE OR REPLACE VIEW vw_detailedartlist AS
SELECT a.id, a.title, c.name AS category_name, u.username AS owner_name
FROM arts a
JOIN categories c ON a.category_id = c.id
JOIN users u ON a.user_id = u.id;

-- 5. Log Özeti (Opsiyonel - logs tablosu varsa)
CREATE OR REPLACE VIEW vw_logsummary AS
SELECT log_message, created_at 
FROM logs 
ORDER BY created_at DESC;
```

## ?? Sütun Ýsimleri Eþleþtirme

### CategoryStatViewModel
```csharp
// Database ? C# Model
name         ? Name
total_arts   ? TotalArts  (veya totalArts)
```

### ActiveUserViewModel
```csharp
username      ? Username
upload_count  ? UploadCount (veya uploadCount)
```

### RecentUploadViewModel
```csharp
title        ? Title
created_at   ? CreatedAt
```

### DetailedArtViewModel
```csharp
id            ? Id
title         ? Title
category_name ? CategoryName
owner_name    ? OwnerName
```

### LogSummaryViewModel
```csharp
log_message  ? LogMessage
created_at   ? CreatedAt
```

## ?? Test

### 1. Database'de View'larý Kontrol Et

```sql
-- View'larýn var olduðunu kontrol et
SELECT * FROM vw_categorystats LIMIT 5;
SELECT * FROM vw_activeusers LIMIT 5;
SELECT * FROM vw_recentuploads LIMIT 5;
SELECT * FROM vw_detailedartlist LIMIT 5;

-- View yoksa hata verir:
-- ERROR: relation "vw_categorystats" does not exist
```

### 2. Spring Boot'u Yeniden Baþlat

```bash
./mvnw clean spring-boot:run
```

### 3. Postman ile Test Et

```
GET http://localhost:8080/api/admin/views/vw_categorystats
Authorization: Bearer ADMIN_JWT_TOKEN

Beklenen: 200 OK
[
  {
    "name": "Manzara",
    "totalArts": 5
  }
]
```

### 4. MVC'yi Yeniden Baþlat

```
Uygulamayý STOP ? RUN (Hot reload yeterli deðil!)
```

### 5. MVC'den Test Et

1. Admin olarak login ol
2. Admin Panel ? Database Views
3. Ýstatistikler yüklenmeli

## ?? Beklenen Log'lar

### MVC Output:
```
=== GET DATABASE VIEW ===
View Name: vw_categorystats
View data fetched: 3 records
Database views loaded: Categories=3, Users=5, Uploads=10
```

### Spring Boot Console:
```
=== GET DATABASE VIEW: vw_categorystats ===
Returning 3 records
```

## ?? Dikkat Edilecekler

1. **Sütun isimleri:** PostgreSQL'de snake_case (`total_arts`), C#'ta PascalCase (`TotalArts`)
2. **View isimleri:** Küçük harf ve underscore (`vw_categorystats`)
3. **Spring Boot'ta SQL:** View'dan direkt sorgulama veya tablo join'leri
4. **MVC'de DTO:** Property isimleri API response'una uymalý

## ?? Sorun Giderme

### Sorun 1: Hala 404 Alýyorum

**Çözüm:** Spring Boot'ta AdminController'ý güncelle ve yeniden baþlat.

### Sorun 2: View Bulunamýyor Hatasý

**Çözüm:** PostgreSQL'de view'larý oluþtur:
```sql
CREATE OR REPLACE VIEW vw_categorystats AS ...
```

### Sorun 3: Sütun Ýsmi Eþleþmiyor

**Çözüm:** SQL'de alias kullan:
```sql
SELECT name, COUNT(*) as totalArts  -- ? camelCase kullan
```

veya Spring Boot'ta Map kullan (otomatik çalýþýr).

## ? Özet

1. ? MVC AdminController güncelendi
2. ? Spring Boot AdminController güncellenmeli
3. ? Database view'larý oluþturulmalý
4. ? Spring Boot restart
5. ? MVC restart
6. ? Test

**Þimdi Spring Boot tarafýný güncelleyin!** ??
