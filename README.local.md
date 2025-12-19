# AI Art Gallery - Local Setup Rehberi

## ?? Hýzlý Baþlangýç

### Windows için:

```cmd
start-local.bat
```

### Linux/Mac için:

```bash
chmod +x start-local.sh
./start-local.sh
```

Bu scriptler otomatik olarak:
- ? .NET SDK kontrolü yapar
- ? Backend API kontrolü yapar
- ? Uygulamayý baþlatýr

## ?? Ön Gereksinimler

### 1. .NET 8.0 SDK

**Windows:**
- [.NET 8.0 SDK Ýndir](https://dotnet.microsoft.com/download/dotnet/8.0)
- Ýndirdiðiniz kurulum dosyasýný çalýþtýrýn
- Kurulum tamamlandýktan sonra terminal/cmd'yi yeniden baþlatýn

**Linux (Ubuntu/Debian):**
```bash
wget https://dot.net/v1/dotnet-install.sh
chmod +x dotnet-install.sh
./dotnet-install.sh --channel 8.0
```

**macOS:**
```bash
brew install dotnet@8
```

**Kontrol:**
```bash
dotnet --version
# Çýktý: 8.0.x olmalý
```

### 2. Spring Boot Backend API

Backend API'nin **http://localhost:8080** adresinde çalýþýyor olmasý gerekiyor.

Backend API'yi baþlatmak için backend projesinin klasörüne gidin:

```bash
# Maven kullanýyorsanýz
./mvnw spring-boot:run

# Gradle kullanýyorsanýz
./gradlew bootRun

# JAR dosyasýndan çalýþtýrma
java -jar target/backend-api.jar
```

**Kontrol:**
```bash
curl http://localhost:8080/api/
# veya tarayýcýda http://localhost:8080 açýn
```

## ?? Uygulamayý Çalýþtýrma

### Yöntem 1: Hazýr Scriptler (Önerilen)

**Windows:**
```cmd
start-local.bat
```

**Linux/Mac:**
```bash
chmod +x start-local.sh
./start-local.sh
```

### Yöntem 2: Manuel Çalýþtýrma

```bash
# 1. Restore (ilk seferde)
dotnet restore

# 2. Çalýþtýr
dotnet run

# veya belirli bir URL ile
dotnet run --urls "http://localhost:5000"
```

### Yöntem 3: Visual Studio

1. `AI_Art_Gallery.sln` dosyasýný Visual Studio ile açýn
2. **F5** tuþuna basýn veya üstteki ?? butonuna týklayýn
3. Tarayýcý otomatik açýlacaktýr

### Yöntem 4: Hot Reload (Geliþtirme için)

Dosya deðiþikliklerini otomatik yüklemek için:

```bash
dotnet watch run
```

## ?? Eriþim

Uygulama baþladýktan sonra:

- **HTTP:** http://localhost:5000
- **HTTPS:** https://localhost:5001

Tarayýcýnýzda bu adreslerden birine girin.

## ?? Yapýlandýrma

### Backend API Adresi Deðiþtirme

Eðer backend API farklý bir adreste çalýþýyorsa:

**appsettings.json:**
```json
{
  "ApiSettings": {
    "BaseUrl": "http://localhost:9090/api"
  }
}
```

### Port Deðiþtirme

**Geçici olarak:**
```bash
dotnet run --urls "http://localhost:3000"
```

**Kalýcý olarak - Properties/launchSettings.json:**
```json
{
  "profiles": {
    "http": {
      "applicationUrl": "http://localhost:3000"
    }
  }
}
```

### HTTPS Sertifika Güvenliði

Ýlk çalýþtýrmada HTTPS uyarýsý alabilirsiniz:

```bash
dotnet dev-certs https --trust
```

## ?? Sorun Giderme

### Problem 1: "dotnet komutu bulunamadý"

**Çözüm:**
- .NET SDK'nýn kurulu olduðundan emin olun: `dotnet --version`
- Kuruluysa, terminal/cmd'yi yeniden baþlatýn
- PATH environment variable'ýna eklendi mi kontrol edin

### Problem 2: "Backend API'ye baðlanýlamýyor"

**Kontrol listesi:**
- ? Backend API çalýþýyor mu? ? `curl http://localhost:8080/api/`
- ? Port doðru mu? ? `appsettings.json` kontrol edin
- ? Firewall engelliyor mu? ? Firewall ayarlarýný kontrol edin

**Çözüm:**
```bash
# Backend API'yi baþlatýn
cd path/to/backend
./mvnw spring-boot:run

# API'nin çalýþtýðýný test edin
curl http://localhost:8080/api/health
```

### Problem 3: "Port zaten kullanýmda"

**Hata:**
```
Unable to bind to http://localhost:5000
```

**Çözüm 1 - Farklý port kullanýn:**
```bash
dotnet run --urls "http://localhost:5002"
```

**Çözüm 2 - Kullanýlan portu kapatýn:**

**Windows:**
```cmd
# Port 5000'i kullanan iþlemi bulun
netstat -ano | findstr :5000

# Ýþlemi sonlandýrýn (PID'yi deðiþtirin)
taskkill /PID <PID> /F
```

**Linux/Mac:**
```bash
# Port 5000'i kullanan iþlemi bulun
lsof -i :5000

# Ýþlemi sonlandýrýn
kill -9 <PID>
```

### Problem 4: "Build hatasý - CS1061 veya CS0246"

**Çözüm:**
```bash
# NuGet paketlerini temizle ve restore et
dotnet clean
dotnet restore
dotnet build
```

### Problem 5: "Database connection error"

Bu proje doðrudan database baðlantýsý kullanmýyor, backend API üzerinden çalýþýyor. Backend API'nin database baðlantýsýný kontrol edin.

### Problem 6: "SSL Sertifika Hatasý"

**Development sertifikasýný yeniden oluþturun:**
```bash
dotnet dev-certs https --clean
dotnet dev-certs https --trust
```

## ?? Loglama ve Debug

### Loglama Seviyesi

**appsettings.Development.json:**
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Debug"
    }
  }
}
```

### Debug Modu

**Visual Studio:**
- Breakpoint koyun
- F5 ile debug baþlatýn

**VS Code:**
1. `.vscode/launch.json` oluþturun:
```json
{
  "version": "0.2.0",
  "configurations": [
    {
      "name": ".NET Core Launch",
      "type": "coreclr",
      "request": "launch",
      "preLaunchTask": "build",
      "program": "${workspaceFolder}/bin/Debug/net8.0/AI_Art_Gallery.dll",
      "args": [],
      "cwd": "${workspaceFolder}",
      "stopAtEntry": false
    }
  ]
}
```

2. F5 ile debug baþlatýn

## ?? Geliþtirme Ýpuçlarý

### Hot Reload Kullanýmý

```bash
dotnet watch run
```

Artýk CSS, Razor View veya C# dosyalarýnda yaptýðýnýz deðiþiklikler otomatik yüklenecek.

### Frontend Asset'leri

Static dosyalar `wwwroot/` klasöründe:
- CSS: `wwwroot/css/`
- JavaScript: `wwwroot/js/`
- Resimler: `wwwroot/images/`

### API Ýstekleri Test Etme

**Postman veya cURL ile:**
```bash
# Backend API'ye direkt istek
curl http://localhost:8080/api/artworks

# MVC uygulamasý üzerinden
curl http://localhost:5000/Artwork/Index
```

## ?? Production Build

### Build

```bash
dotnet build --configuration Release
```

### Publish

```bash
dotnet publish --configuration Release --output ./publish

# Publish edilen dosyalarý çalýþtýrma
cd publish
dotnet AI_Art_Gallery.dll
```

### IIS Deploy (Windows Server)

1. IIS'te site oluþturun
2. Publish klasörünü site dizinine kopyalayýn
3. Application Pool'u `.NET CLR Version: No Managed Code` olarak ayarlayýn
4. ASP.NET Core Hosting Bundle'ý yükleyin

## ?? Güvenlik Notlarý

### Development

- HTTPS sertifikalarý self-signed
- Detaylý loglama aktif
- Debug bilgileri gösteriliyor

### Production

- Geçerli SSL sertifikasý kullanýn
- Loglama seviyesini `Warning` veya `Error` yapýn
- `appsettings.Production.json` oluþturun
- Hassas bilgileri environment variable olarak saklayýn

## ?? Ek Kaynaklar

- [ASP.NET Core Dokümantasyonu](https://docs.microsoft.com/aspnet/core)
- [.NET CLI Komutlarý](https://docs.microsoft.com/dotnet/core/tools/)
- [Razor Syntax](https://docs.microsoft.com/aspnet/core/mvc/views/razor)

## ?? Yardým

Hala sorun yaþýyorsanýz:

1. **Loglarý kontrol edin** - Terminal çýktýsýný okuyun
2. **Build yapýn** - `dotnet build` ile hatalarý görün
3. **Temiz baþlangýç** - `dotnet clean` sonra `dotnet restore`
4. **Issue açýn** - GitHub'da sorununuzu detaylý açýklayýn

## ? Baþarýlý Kurulum Kontrolü

Þunlarý test edin:

1. ? Ana sayfa açýlýyor: http://localhost:5000
2. ? Backend API'den veri geliyor
3. ? Login/Register sayfalarý çalýþýyor
4. ? Statik dosyalar (CSS/JS) yükleniyor
5. ? Resimler görüntüleniyor

Herhangi biri çalýþmýyorsa yukarýdaki sorun giderme adýmlarýna bakýn.

---

**Mutlu kodlamalar! ??**
