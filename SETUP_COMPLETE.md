# ?? Local Setup Tamamlandý!

## ? Yapýlan Deðiþiklikler

### 1. Core Files Optimized
- **Program.cs** - Docker kontrollerini kaldýrýldý, temiz local config
- **SpringApiClient.cs** - Base URL düzeltildi (/api path dahil)
- **launchSettings.json** - Standart portlar (5000/5001)

### 2. Configuration Files
- **appsettings.json** - Timeout eklendi
- **appsettings.Development.json** - Detaylý loglama
- **appsettings.Local.json** - Yeni opsiyonel config

### 3. Script'ler Eklendi
```
start-local.bat/sh    ? Hýzlý baþlatma
start-dev.bat/sh      ? Hot reload modu
build.bat/sh          ? Release build
```

### 4. Dökümanlar
```
README.md             ? Ana rehber (güncellendi)
README.local.md       ? Detaylý local setup
QUICKSTART.md         ? Hýzlý referans
PROJECT_STATUS.md     ? Proje durumu
```

## ?? Hemen Baþla

### 1. Backend API'yi Baþlat
```bash
cd ../backend
./mvnw spring-boot:run    # veya ./gradlew bootRun
```

### 2. Frontend'i Baþlat
```cmd
# Windows
start-local.bat

# Linux/Mac
chmod +x *.sh
./start-local.sh
```

### 3. Tarayýcýda Aç
http://localhost:5000

## ?? Proje Durumu

? Build: **Baþarýlý** (Debug + Release)
? Portlar: **5000/5001** (Standartlaþtýrýldý)
? API: **localhost:8080/api** (Hazýr)
? Dökümanlar: **Tam ve güncel**

## ?? Script Kullanýmý

| Script | Amaç | Hot Reload |
|--------|------|------------|
| start-local | Normal çalýþtýrma | ? |
| start-dev | Geliþtirme modu | ? |
| build | Release build | - |

## ?? Daha Fazla Bilgi

- **Hýzlý baþvuru:** `QUICKSTART.md`
- **Detaylý rehber:** `README.local.md`
- **Proje durumu:** `PROJECT_STATUS.md`

## ? Önemli Notlar

1. **Backend API zorunlu** - Port 8080'de çalýþmalý
2. **Hot reload için** - `start-dev.bat/sh` kullanýn
3. **HTTPS için** - `dotnet dev-certs https --trust`
4. **Port deðiþtirmek için** - `launchSettings.json` düzenleyin

## ?? Hazýr!

Projeniz local ortamda çalýþmaya **tamamen hazýr**!

```cmd
start-local.bat  # ? Bunu çalýþtýrýn ve baþlayýn!
```

---
**Not:** Tüm script'ler otomatik kontrol yapar (.NET SDK, Backend API)
