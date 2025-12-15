@echo off
REM AI Art Gallery - Docker Start Script (Windows)

echo ========================================
echo AI Art Gallery - Docker Deployment
echo ========================================
echo.

REM .env dosyasý kontrolü
if not exist ".env" (
    echo [HATA] .env dosyasi bulunamadi!
    echo .env.example dosyasindan .env olusturun:
    echo   copy .env.example .env
    echo.
    pause
    exit /b 1
)

echo [1/4] Docker servisleri baslatiliyor...
docker-compose up -d

echo.
echo [2/4] Container durumu kontrol ediliyor...
docker-compose ps

echo.
echo [3/4] Servisler hazir bekleniyor...
timeout /t 10 /nobreak > nul

echo.
echo [4/4] Health check yapiliyor...
docker-compose ps

echo.
echo ========================================
echo Deployment tamamlandi!
echo ========================================
echo.
echo Erisim URL'leri:
echo   MVC App:    http://localhost:5000
echo   Spring API: http://localhost:8080
echo   PostgreSQL: localhost:5432
echo.
echo Loglari izlemek icin:
echo   docker-compose logs -f
echo.
echo Servisleri durdurmak icin:
echo   docker-compose down
echo.

pause
