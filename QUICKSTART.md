# ?? AI Art Gallery - Hýzlý Baþvuru

## Baþlatma Komutlarý

### En Hýzlý Yol (Önerilen)
```cmd
start-local.bat          # Windows - Normal mod
./start-local.sh         # Linux/Mac - Normal mod
```

### Geliþtirme Modu (Hot Reload)
```cmd
start-dev.bat            # Windows - Hot reload
./start-dev.sh           # Linux/Mac - Hot reload
```

### Manuel Baþlatma
```bash
dotnet run               # Normal mod
dotnet watch run         # Hot reload ile (geliþtirme için)
```

### Build Script
```cmd
build.bat                # Windows - Release build
./build.sh               # Linux/Mac - Release build
```

## Eriþim Adresleri

| Servis | URL |
|--------|-----|
| MVC App (HTTP) | http://localhost:5000 |
| MVC App (HTTPS) | https://localhost:5001 |
| Backend API | http://localhost:8080 |

## Hýzlý Kontroller

### .NET SDK Kontrolü
```bash
dotnet --version
```

### Backend API Kontrolü
```bash
curl http://localhost:8080/api/
```

### Build Kontrolü
```bash
dotnet build
```

## Sýk Kullanýlan Komutlar

### Build & Clean
```bash
dotnet clean             # Temizle
dotnet restore           # Paketleri indir
dotnet build             # Build
dotnet build -c Release  # Release build
```

### Çalýþtýrma
```bash
dotnet run                              # Varsayýlan portlarda
dotnet run --urls "http://localhost:3000"  # Özel port
dotnet watch run                        # Hot reload
```

### Publishing
```bash
dotnet publish -c Release -o ./publish
cd publish
dotnet AI_Art_Gallery.dll
```

## Ayar Dosyalarý

### Backend API Adresi
**Dosya:** `appsettings.json`
```json
{
  "ApiSettings": {
    "BaseUrl": "http://localhost:8080/api"
  }
}
```

### Portlar
**Dosya:** `Properties/launchSettings.json`
```json
{
  "profiles": {
    "http": {
      "applicationUrl": "http://localhost:5000"
    }
  }
}
```

## Hýzlý Sorun Çözme

### Port Zaten Kullanýmda
```bash
# Farklý port kullan
dotnet run --urls "http://localhost:5002"

# veya kullanýlan portu bul ve kapat (Windows)
netstat -ano | findstr :5000
taskkill /PID <PID> /F
```

### Backend API Çalýþmýyor
```bash
# Backend klasörüne git
cd ../backend

# Baþlat
./mvnw spring-boot:run     # Maven
./gradlew bootRun          # Gradle
```

### SSL Sertifika Hatasý
```bash
dotnet dev-certs https --clean
dotnet dev-certs https --trust
```

### Build Hatasý
```bash
dotnet clean
dotnet restore
dotnet build
```

## Klasör Yapýsý

```
mvc/
??? Controllers/        # API Controllers
??? Models/            # View & Data Models
??? Views/             # Razor Views
??? wwwroot/           # Static files
?   ??? css/
?   ??? js/
?   ??? images/
??? Services/          # API Client
??? Program.cs         # Entry point
??? appsettings.json   # Configuration
??? start-local.bat    # Quick start script
```

## Debug

### Visual Studio
1. Breakpoint koy
2. F5'e bas

### Loglama Seviyesi
**appsettings.Development.json:**
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug"
    }
  }
}
```

## Geliþtirme Ýpuçlarý

### Hot Reload Aktif
```bash
dotnet watch run
```
Artýk kod deðiþikliklerinde otomatik yenileme olacak.

### API Test
```bash
# Backend API test
curl http://localhost:8080/api/artworks

# MVC endpoint test
curl http://localhost:5000/Artwork/Index
```

## Environment'lar

| Environment | Dosya | Kullaným |
|-------------|-------|----------|
| Development | appsettings.Development.json | Local geliþtirme |
| Production | appsettings.Production.json | Canlý ortam |

## Önemli Notlar

- ? Backend API **mutlaka** çalýþýyor olmalý (port 8080)
- ? .NET 8.0 SDK kurulu olmalý
- ? Ýlk çalýþtýrmada `dotnet restore` yapýn
- ? HTTPS için sertifika güvenini onaylayýn

## Yardým

- ?? Detaylý rehber: [README.local.md](README.local.md)
- ?? Ana döküman: [README.md](README.md)

---

**Hýzlý baþlangýç için `start-local.bat` çalýþtýrýn!** ??
