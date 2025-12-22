# Spring Boot - Admin Database Views API

## Controller Oluþtur

**`src/main/java/com/yourpackage/controller/AdminController.java`**

```java
package com.yourpackage.controller;

import com.yourpackage.dto.*;
import com.yourpackage.service.AdminService;
import lombok.RequiredArgsConstructor;
import lombok.extern.slf4j.Slf4j;
import org.springframework.http.ResponseEntity;
import org.springframework.security.access.prepost.PreAuthorize;
import org.springframework.web.bind.annotation.*;

import java.util.List;

@Slf4j
@RestController
@RequestMapping("/api/admin")
@RequiredArgsConstructor
@PreAuthorize("hasRole('ADMIN')")
public class AdminController {
    
    private final AdminService adminService;
    
    /**
     * GET /api/admin/views/{viewName}
     * Database view'larýný döner
     */
    @GetMapping("/views/{viewName}")
    public ResponseEntity<?> getDatabaseView(@PathVariable String viewName) {
        log.info("=== GET DATABASE VIEW ===");
        log.info("View Name: {}", viewName);
        
        try {
            switch (viewName) {
                case "vw_detailedartlist":
                    List<DetailedArtDTO> detailedArts = adminService.getDetailedArts();
                    log.info("Detailed arts fetched: {} records", detailedArts.size());
                    return ResponseEntity.ok(detailedArts);
                    
                case "vw_categorystats":
                    List<CategoryStatDTO> categoryStats = adminService.getCategoryStats();
                    log.info("Category stats fetched: {} records", categoryStats.size());
                    return ResponseEntity.ok(categoryStats);
                    
                case "vw_activeusers":
                    List<ActiveUserDTO> activeUsers = adminService.getActiveUsers();
                    log.info("Active users fetched: {} records", activeUsers.size());
                    return ResponseEntity.ok(activeUsers);
                    
                case "vw_recentuploads":
                    List<RecentUploadDTO> recentUploads = adminService.getRecentUploads();
                    log.info("Recent uploads fetched: {} records", recentUploads.size());
                    return ResponseEntity.ok(recentUploads);
                    
                case "vw_logsummary":
                    List<LogSummaryDTO> logSummary = adminService.getLogSummary();
                    log.info("Log summary fetched: {} records", logSummary.size());
                    return ResponseEntity.ok(logSummary);
                    
                default:
                    log.warn("Unknown view name: {}", viewName);
                    return ResponseEntity.badRequest().body("Unknown view: " + viewName);
            }
        } catch (Exception e) {
            log.error("Error fetching database view: {}", viewName, e);
            return ResponseEntity.internalServerError().body("Error fetching view: " + e.getMessage());
        }
    }
}
```

## DTO Sýnýflarý

**`src/main/java/com/yourpackage/dto/DetailedArtDTO.java`**
```java
@Data
public class DetailedArtDTO {
    private Integer id;
    private String title;
    private String categoryName;
    private String ownerName;
}
```

**`src/main/java/com/yourpackage/dto/CategoryStatDTO.java`**
```java
@Data
public class CategoryStatDTO {
    private String name;
    private Integer totalArts;
}
```

**`src/main/java/com/yourpackage/dto/ActiveUserDTO.java`**
```java
@Data
public class ActiveUserDTO {
    private String username;
    private Integer uploadCount;
}
```

**`src/main/java/com/yourpackage/dto/RecentUploadDTO.java`**
```java
@Data
public class RecentUploadDTO {
    private String title;
    private LocalDateTime createdAt;
}
```

**`src/main/java/com/yourpackage/dto/LogSummaryDTO.java`**
```java
@Data
public class LogSummaryDTO {
    private String logMessage;
    private LocalDateTime createdAt;
}
```

## Service Sýnýfý

**`src/main/java/com/yourpackage/service/AdminService.java`**
```java
@Service
@RequiredArgsConstructor
@Slf4j
public class AdminService {
    
    private final JdbcTemplate jdbcTemplate;
    
    public List<DetailedArtDTO> getDetailedArts() {
        String sql = "SELECT * FROM vw_DetailedArtList";
        
        return jdbcTemplate.query(sql, (rs, rowNum) -> {
            DetailedArtDTO dto = new DetailedArtDTO();
            dto.setId(rs.getInt("id"));
            dto.setTitle(rs.getString("title"));
            dto.setCategoryName(rs.getString("category_name"));
            dto.setOwnerName(rs.getString("owner_name"));
            return dto;
        });
    }
    
    public List<CategoryStatDTO> getCategoryStats() {
        String sql = "SELECT * FROM vw_CategoryStats";
        
        return jdbcTemplate.query(sql, (rs, rowNum) -> {
            CategoryStatDTO dto = new CategoryStatDTO();
            dto.setName(rs.getString("name"));
            dto.setTotalArts(rs.getInt("total_arts"));
            return dto;
        });
    }
    
    public List<ActiveUserDTO> getActiveUsers() {
        String sql = "SELECT * FROM vw_ActiveUsers";
        
        return jdbcTemplate.query(sql, (rs, rowNum) -> {
            ActiveUserDTO dto = new ActiveUserDTO();
            dto.setUsername(rs.getString("username"));
            dto.setUploadCount(rs.getInt("upload_count"));
            return dto;
        });
    }
    
    public List<RecentUploadDTO> getRecentUploads() {
        String sql = "SELECT * FROM vw_RecentUploads";
        
        return jdbcTemplate.query(sql, (rs, rowNum) -> {
            RecentUploadDTO dto = new RecentUploadDTO();
            dto.setTitle(rs.getString("title"));
            dto.setCreatedAt(rs.getTimestamp("created_at").toLocalDateTime());
            return dto;
        });
    }
    
    public List<LogSummaryDTO> getLogSummary() {
        String sql = "SELECT * FROM vw_LogSummary";
        
        return jdbcTemplate.query(sql, (rs, rowNum) -> {
            LogSummaryDTO dto = new LogSummaryDTO();
            dto.setLogMessage(rs.getString("log_message"));
            dto.setCreatedAt(rs.getTimestamp("created_at").toLocalDateTime());
            return dto;
        });
    }
}
```

## SecurityConfig Güncellemesi

```java
@Bean
public SecurityFilterChain securityFilterChain(HttpSecurity http) throws Exception {
    http
        // ...
        .authorizeHttpRequests(auth -> auth
            // Public endpoints
            .requestMatchers("/api/auth/**").permitAll()
            .requestMatchers(HttpMethod.GET, "/api/arts/public").permitAll()
            
            // Admin endpoints
            .requestMatchers("/api/admin/**").hasRole("ADMIN")  // ? BU SATIR
            
            // Authenticated endpoints
            .requestMatchers("/api/arts/**").authenticated()
            
            .anyRequest().authenticated()
        );
    
    return http.build();
}
```

## Test

### Postman ile Test Edin

```
GET http://localhost:8080/api/admin/views/category-stats
Authorization: Bearer ADMIN_JWT_TOKEN

Beklenen: 200 OK
[
  {
    "name": "Manzara",
    "totalArts": 15
  },
  {
    "name": "Portre",
    "totalArts": 8
  }
]
```

## Kurulum

1. **View'larý oluþturun** (PostgreSQL)
2. **Controller, Service, DTO ekleyin**
3. **SecurityConfig'i güncelleyin**
4. **Spring Boot'u yeniden baþlatýn**
5. **MVC'den test edin: `/Admin/DatabaseViews`**
