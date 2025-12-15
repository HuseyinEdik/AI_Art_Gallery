# AI Art Gallery - Docker Deployment Guide

## ?? Gereksinimler

- Docker Desktop (Windows/Mac) veya Docker Engine (Linux)
- Docker Compose v2.0+
- Spring Boot API'nizin Docker image'ý

## ?? Hýzlý Baþlangýç

### 1. Environment Dosyasýný Hazýrlayýn

```bash
cp .env.example .env
```

`.env` dosyasýný düzenleyin ve güvenli þifreler ayarlayýn.

### 2. Development Ortamý

```bash
# Tüm servisleri baþlat
docker-compose up -d

# Loglarý izle
docker-compose logs -f

# Servisleri durdur
docker-compose down
```

**Eriþim URL'leri:**
- MVC App: http://localhost:5000
- Spring API: http://localhost:8080
- PostgreSQL: localhost:5432

### 3. Production Ortamý

```bash
# Production için build
docker build -t your-dockerhub-username/ai-gallery-mvc:latest .

# Docker Hub'a push
docker push your-dockerhub-username/ai-gallery-mvc:latest

# Production ortamýný baþlat
docker-compose -f docker-compose.prod.yml up -d
```

## ??? Docker Komutlarý

### Container Yönetimi

```bash
# Tüm container'larý listele
docker-compose ps

# Belirli bir servisin loglarýný görüntüle
docker-compose logs -f mvc-app
docker-compose logs -f spring-api
docker-compose logs -f postgres

# Container'a baðlan
docker exec -it ai-gallery-mvc bash
docker exec -it ai-gallery-spring-api bash
docker exec -it ai-gallery-postgres psql -U postgres

# Servisleri yeniden baþlat
docker-compose restart mvc-app

# Belirli bir servisi yeniden build et
docker-compose up -d --build mvc-app
```

### Veritabaný Ýþlemleri

```bash
# PostgreSQL'e baðlan
docker exec -it ai-gallery-postgres psql -U postgres -d ai_gallery_db

# Backup al
docker exec ai-gallery-postgres pg_dump -U postgres ai_gallery_db > backup.sql

# Restore et
docker exec -i ai-gallery-postgres psql -U postgres ai_gallery_db < backup.sql
```

### Temizlik

```bash
# Container'larý durdur ve sil
docker-compose down

# Volume'larý da sil (DÝKKAT: Veritabaný silinir!)
docker-compose down -v

# Kullanýlmayan image'larý temizle
docker image prune -a

# Tüm sistemý temizle
docker system prune -a --volumes
```

## ??? Yapý

```
??? Dockerfile                  # MVC App için Dockerfile
??? docker-compose.yml          # Development ortamý
??? docker-compose.prod.yml     # Production ortamý
??? .dockerignore               # Docker build'den hariç tutulacak dosyalar
??? .env.example                # Environment deðiþken örneði
??? nginx.conf                  # Nginx reverse proxy config
??? appsettings.Docker.json     # Docker ortamý için ayarlar
??? README.docker.md            # Bu dosya
```

## ?? Yapýlandýrma

### appsettings.Docker.json

Docker container'larý arasýnda iletiþim için özel yapýlandýrma:

```json
{
  "ApiSettings": {
    "BaseUrl": "http://spring-api:8080/api"
  }
}
```

### Environment Variables

`.env` dosyasýnda ayarlanabilir deðiþkenler:

| Deðiþken | Açýklama | Varsayýlan |
|----------|----------|------------|
| POSTGRES_DB | Veritabaný adý | ai_gallery_db |
| POSTGRES_USER | Veritabaný kullanýcýsý | postgres |
| POSTGRES_PASSWORD | Veritabaný þifresi | - |
| SPRING_API_IMAGE | Spring API image | - |
| MVC_APP_IMAGE | MVC App image | - |
| MVC_PORT | MVC dýþ port | 5000 |

## ?? Network

Docker Compose otomatik olarak `ai-gallery-network` adýnda bir bridge network oluþturur. Tüm servisler bu network üzerinden haberleþir:

- `postgres:5432` - PostgreSQL
- `spring-api:8080` - Spring Boot API
- `mvc-app:80` - ASP.NET Core MVC

## ?? Health Checks

PostgreSQL için health check yapýlandýrýlmýþtýr. Spring API ancak PostgreSQL hazýr olduðunda baþlar.

## ?? Güvenlik

Production ortamý için:

1. `.env` dosyasýný asla Git'e commit etmeyin
2. Güçlü þifreler kullanýn
3. HTTPS için SSL sertifikasý ekleyin
4. Nginx reverse proxy kullanýn
5. Container'larý non-root user ile çalýþtýrýn

## ?? Sorun Giderme

### MVC App Spring API'ye baðlanamýyor

```bash
# Network baðlantýsýný kontrol et
docker network inspect ai-gallery-network

# Spring API'nin hazýr olduðundan emin ol
docker-compose logs spring-api

# DNS çözümlemesini test et
docker exec ai-gallery-mvc ping spring-api
```

### Veritabaný baðlantý hatasý

```bash
# PostgreSQL'in çalýþtýðýndan emin ol
docker-compose ps postgres

# Loglarý kontrol et
docker-compose logs postgres

# Manuel baðlantý testi
docker exec -it ai-gallery-postgres psql -U postgres
```

### Port çakýþmasý

Eðer portlar kullanýmdaysa, `docker-compose.yml` dosyasýnda port mapping'leri deðiþtirin:

```yaml
ports:
  - "5001:80"  # 5000 yerine 5001
```

## ?? Notlar

- Ýlk baþlatmada Spring API'nin veritabaný þemasýný oluþturmasý birkaç dakika sürebilir
- Production ortamýnda volume backup'larý düzenli alýn
- Log rotation yapýlandýrmasý ekleyin
- Container resource limits belirleyin

## ?? Katkýda Bulunma

Docker yapýlandýrmasýnda iyileþtirme önerileriniz için pull request açabilirsiniz.
