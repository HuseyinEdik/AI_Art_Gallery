# AI Art Gallery - Docker Deployment Özeti

## ? Oluþturulan Dosyalar

### 1. Dockerfile
- Multi-stage build ile optimize edilmiþ .NET 8 container
- Build, publish ve runtime aþamalarý
- Port 80 ve 443 expose edilmiþ

### 2. docker-compose.yml (Development)
Üç servis içerir:
- **postgres**: PostgreSQL 15 database
- **spring-api**: Spring Boot backend API
- **mvc-app**: ASP.NET Core MVC frontend

### 3. docker-compose.prod.yml (Production)
Production ortamý için ek olarak:
- **nginx**: Reverse proxy ve load balancing
- Environment variable desteði
- Health check'ler
- Auto-restart politikalarý

### 4. appsettings.Docker.json
- Docker network için API base URL: `http://spring-api:8080/api`
- Container içi servis discovery

### 5. .dockerignore
- Build süresini kýsaltmak için gereksiz dosyalarý hariç tutar
- bin/, obj/, .git/ vb.

### 6. .env.example
- Environment deðiþken þablonu
- PostgreSQL, Spring API ve MVC App ayarlarý

### 7. nginx.conf
- Reverse proxy yapýlandýrmasý
- MVC ve API yönlendirmeleri
- Static file caching
- HTTPS yapýlandýrmasý (yorum satýrýnda)

### 8. Baþlatma Scriptleri
- `docker-start.bat` (Windows)
- `docker-start.sh` (Linux/Mac)
- `docker-stop.bat` (Windows)
- `docker-stop.sh` (Linux/Mac)

### 9. README.docker.md
- Detaylý deployment guide
- Komut örnekleri
- Sorun giderme

## ?? Kullaným

### Hýzlý Baþlangýç (Development)

```bash
# 1. Environment dosyasýný oluþtur
copy .env.example .env

# 2. .env dosyasýný düzenle (þifreler vb.)

# 3. Docker servislerini baþlat
docker-compose up -d

# 4. Eriþim
# MVC: http://localhost:5000
# API: http://localhost:8080
```

### Production Deployment

```bash
# 1. Docker image oluþtur
docker build -t yourusername/ai-gallery-mvc:latest .

# 2. Docker Hub'a push et
docker push yourusername/ai-gallery-mvc:latest

# 3. .env dosyasýný güncelle (image adlarý)

# 4. Production ortamýný baþlat
docker-compose -f docker-compose.prod.yml up -d
```

## ?? Yapýlandýrma Deðiþiklikleri

### Program.cs
? Docker ortamý desteði eklendi:
- Docker environment detection
- Dinamik API base URL configuration
- HttpClient factory pattern
- HTTPS redirection Docker'da kapalý
- Session timeout ayarlarý

### SpringApiClient.cs
? Dependency injection iyileþtirildi:
- ILogger eklenmiþ
- HttpClient factory'den base address alýyor
- Fallback URL mekanizmasý

## ?? Yapýlmasý Gerekenler

### Spring Boot API Tarafý

Spring Boot projenizde de benzer Dockerfile oluþturmanýz gerekiyor:

```dockerfile
FROM openjdk:17-jdk-slim AS build
WORKDIR /app
COPY . .
RUN ./mvnw clean package -DskipTests

FROM openjdk:17-jdk-slim
WORKDIR /app
COPY --from=build /app/target/*.jar app.jar
EXPOSE 8080
ENTRYPOINT ["java", "-jar", "app.jar"]
```

### .env Dosyasý Ayarlarý

`.env.example` dosyasýný `.env` olarak kopyalayýn ve þunlarý güncelleyin:

```env
POSTGRES_PASSWORD=güçlü-bir-þifre
SPRING_API_IMAGE=yourusername/your-spring-api:latest
MVC_APP_IMAGE=yourusername/ai-gallery-mvc:latest
JWT_SECRET=güvenli-jwt-secret-key
```

### SSL Sertifikasý (Production)

HTTPS için SSL sertifikasý ekleyin:

```bash
# Let's Encrypt ile
certbot certonly --standalone -d yourdomain.com

# Sertifikalarý nginx/ssl/ klasörüne kopyalayýn
```

## ?? Network Yapýsý

```
???????????????????????
?   Client Browser    ?
???????????????????????
           ?
           ?
    ????????????????
    ?    Nginx     ? :80, :443
    ? Reverse Proxy?
    ????????????????
       ?       ?
       ?       ?
???????????? ????????????
? MVC App  ? ?Spring API?
?   :80    ? ?  :8080   ?
???????????? ????????????
                  ?
                  ?
            ????????????
            ?PostgreSQL?
            ?  :5432   ?
            ????????????
```

## ?? Güvenlik Kontrol Listesi

- [ ] `.env` dosyasý `.gitignore`'a eklenmiþ
- [ ] Güçlü þifreler kullanýlmýþ
- [ ] JWT secret güvenli
- [ ] Production'da HTTPS aktif
- [ ] Database volume backup stratejisi var
- [ ] Container'lar non-root user ile çalýþýyor
- [ ] Network izolasyonu yapýlandýrýlmýþ
- [ ] Rate limiting eklenmeli (Nginx)
- [ ] Firewall kurallarý yapýlandýrýlmýþ

## ?? Monitoring

Container'larý izlemek için:

```bash
# CPU ve Memory kullanýmý
docker stats

# Log streaming
docker-compose logs -f

# Container health
docker-compose ps
```

## ?? Sorun Giderme

### 1. API Baðlantý Hatasý
```bash
# Network kontrolü
docker network inspect ai-gallery-network

# DNS test
docker exec ai-gallery-mvc ping spring-api
```

### 2. Port Çakýþmasý
`docker-compose.yml` dosyasýnda port deðiþtirin:
```yaml
ports:
  - "5001:80"  # 5000 yerine
```

### 3. Database Migration
```bash
# Spring API loglarýný kontrol et
docker-compose logs spring-api

# Manuel migration gerekirse
docker exec -it ai-gallery-postgres psql -U postgres
```

## ?? Destek

Sorun yaþarsanýz:
1. `docker-compose logs` ile loglarý kontrol edin
2. GitHub Issues'da yeni issue açýn
3. README.docker.md dosyasýna bakýn

## ?? Sonuç

Docker deployment hazýr! Artýk projenizi kolayca containerize edip deploy edebilirsiniz.

**Sonraki Adýmlar:**
1. Spring Boot API için Dockerfile oluþtur
2. .env dosyasýný yapýlandýr
3. `docker-compose up -d` ile baþlat
4. Production'da Nginx ve HTTPS ekle
