@echo off
echo ========================================
echo   AI Art Gallery - Local Baslat
echo ========================================
echo.

REM .NET SDK kontrol
echo .NET SDK kontrol ediliyor...
dotnet --version >nul 2>&1
if %errorlevel% neq 0 (
    echo [HATA] .NET SDK bulunamadi!
    echo Lutfen .NET 8.0 SDK yukleyin: https://dotnet.microsoft.com/download/dotnet/8.0
    pause
    exit /b 1
)
echo [OK] .NET SDK kurulu

echo.
echo Backend API kontrol ediliyor...
curl -s http://localhost:8080/api/ >nul 2>&1
if %errorlevel% neq 0 (
    echo [UYARI] Backend API yanit vermiyor!
    echo Lutfen Spring Boot API'nin http://localhost:8080 adresinde calistigindan emin olun.
    echo.
    choice /C YN /M "Yine de devam etmek istiyor musunuz?"
    if errorlevel 2 (
        echo Islemi iptal edildi.
        pause
        exit /b 1
    )
) else (
    echo [OK] Backend API aktif
)

echo.
echo ========================================
echo   Uygulama baslatiliyor...
echo ========================================
echo.
echo Erisim adresleri:
echo   - HTTP:  http://localhost:5000
echo   - HTTPS: https://localhost:5001
echo.
echo Backend API: http://localhost:8080/api
echo.
echo Uygulamayi durdurmak icin Ctrl+C kullanin
echo.

set ASPNETCORE_ENVIRONMENT=Development
dotnet run --urls "http://localhost:5000;https://localhost:5001"

pause
