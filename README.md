# AI Art Gallery - MVC Frontend

ASP.NET Core 8.0 MVC uygulamasý ile geliþtirilmiþ AI Art Gallery projesi.

> ? **Local Setup Tamamlandý!** Local ortamda çalýþmaya hazýr.

## ?? Dökümanlar

- ?? **[SETUP_COMPLETE.md](SETUP_COMPLETE.md)** - Setup özeti ve hýzlý baþlangýç
- ?? **[QUICKSTART.md](QUICKSTART.md)** - Hýzlý baþvuru ve komutlar
- ?? **[README.local.md](README.local.md)** - Detaylý local kurulum rehberi
- ?? **[PROJECT_STATUS.md](PROJECT_STATUS.md)** - Proje durumu ve deðiþiklikler

## ?? Gereksinimler

- .NET 8.0 SDK
- Spring Boot API (Backend) - Port 8080'de çalýþýyor olmalý
- Visual Studio 2022 veya Visual Studio Code

## ?? Kurulum

### Hýzlý Baþlat

**Windows için:**
```cmd
start-local.bat          # Normal mod
start-dev.bat            # Hot reload ile (geliþtirme)
build.bat                # Release build
```

**Linux/Mac için:**
```bash
chmod +x start-local.sh start-dev.sh build.sh
./start-local.sh         # Normal mod
./start-dev.sh           # Hot reload ile (geliþtirme)
./build.sh               # Release build
```

Bu scriptler otomatik olarak .NET SDK ve Backend API kontrolü yapar, sonra uygulamayý baþlatýr.

### Manuel Kurulum

1. **.NET 8.0 SDK Kurun**
   
   [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)'yý indirip kurun.
   
   ```bash
   dotnet --version  # Kontrol edin
   ```

2. **Backend API'yi Baþlatýn**
   
   Spring Boot API'nizin **http://localhost:8080** adresinde çalýþtýðýndan emin olun.

3. **Projeyi Çalýþtýrýn**

   **Visual Studio:**
   - `AI_Art_Gallery.sln` dosyasýný açýn
   - F5'e basýn
   
   **Komut Satýrý:**
   ```bash
   dotnet restore
   dotnet run
   ```
   
   **Hot Reload (Geliþtirme):**
   ```bash
   dotnet watch run
   ```

4. **Uygulamaya Eriþim**
   - HTTP: http://localhost:5000
   - HTTPS: https://localhost:5001

?? **Detaylý kurulum rehberi için:** [README.local.md](README.local.md)

## ?? Yapýlandýrma

### API Ayarlarý

`appsettings.json` dosyasýnda backend API adresini deðiþtirebilirsiniz:

```json
{
  "ApiSettings": {
    "BaseUrl": "http://localhost:8080/api"
  }
}
```

Backend API'niz farklý bir portta çalýþýyorsa bu deðeri güncelleyin.

### Port Deðiþtirme

`Properties/launchSettings.json` dosyasýndan veya komut satýrýndan port deðiþtirebilirsiniz:

```bash
dotnet run --urls "http://localhost:3000"
```

## ?? Build

### Development Build

```bash
dotnet build
```

### Release Build

```bash
dotnet build --configuration Release
```

### Publish

```bash
dotnet publish --configuration Release --output ./publish
```

## ?? Geliþtirme

### Hot Reload

Geliþtirme sýrasýnda dosya deðiþikliklerini otomatik yüklemek için:

```bash
dotnet watch run
```

### Debug

Visual Studio'da F5 ile debug modunda çalýþtýrabilir veya VS Code ile:

1. `.vscode/launch.json` yapýlandýrmasý oluþturun
2. F5 ile debug baþlatýn

## ?? Proje Yapýsý

```
AI_Art_Gallery/
??? Controllers/         # MVC Controllers
??? Models/             # View Models ve Data Models
??? Views/              # Razor Views
??? wwwroot/           # Static files (CSS, JS, images)
?   ??? images/        # Yüklenen görseller
??? Services/          # API Client ve diðer servisler
??? Data/              # Data access layer
??? Program.cs         # Uygulama baþlangýç noktasý
??? appsettings.json   # Yapýlandýrma dosyasý
```

## ?? Sorun Giderme

### Backend API'ye Baðlanamýyor

Hata: "Connection refused" veya API isteði baþarýsýz

**Çözüm:**
1. Backend API'nin çalýþtýðýndan emin olun:
   ```bash
   curl http://localhost:8080/api/health
   ```
2. `appsettings.json` dosyasýndaki API URL'sini kontrol edin
3. Firewall ayarlarýný kontrol edin

### Port Zaten Kullanýmda

Hata: "Address already in use"

**Çözüm:**
```bash
# Farklý bir port kullanýn
dotnet run --urls "http://localhost:5002"
```

### SSL Sertifika Hatasý

Hata: HTTPS sertifika uyarýsý

**Çözüm:**
```bash
# Development sertifikasýný güvenilir olarak iþaretleyin
dotnet dev-certs https --trust
```

## ?? Güvenlik

- Development ortamýnda otomatik oluþturulan sertifikalar kullanýlýr
- Production'da geçerli SSL sertifikasý kullanýn
- Hassas bilgileri `appsettings.json` yerine User Secrets veya Environment Variables kullanarak saklayýn

```bash
# User Secrets kullanýmý
dotnet user-secrets init
dotnet user-secrets set "ApiSettings:BaseUrl" "http://your-api-url"
```

## ?? Katkýda Bulunma

1. Fork edin
2. Feature branch oluþturun (`git checkout -b feature/amazing-feature`)
3. Deðiþikliklerinizi commit edin (`git commit -m 'Add amazing feature'`)
4. Branch'inizi push edin (`git push origin feature/amazing-feature`)
5. Pull Request açýn

## ?? Lisans

Bu proje açýk kaynak kodludur.

## ?? Ýletiþim

Proje sahibi: [@HuseyinEdik](https://github.com/HuseyinEdik)

## ?? Özellikler

- Modern ve responsive tasarým
- AI tarafýndan üretilen sanat eserlerini görüntüleme
- Kullanýcý kimlik doðrulama sistemi
- Resim yükleme ve yönetimi
- Session yönetimi
- Cookie tabanlý authentication

## ?? Güncellemeler

Projeyi güncel tutmak için:

```bash
git pull origin master
dotnet restore
dotnet build
```