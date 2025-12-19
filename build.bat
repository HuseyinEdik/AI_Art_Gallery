@echo off
echo ========================================
echo   AI Art Gallery - Build Script
echo ========================================
echo.

echo [1/4] Temizleniyor...
dotnet clean
if %errorlevel% neq 0 (
    echo [HATA] Clean islemi basarisiz!
    pause
    exit /b 1
)

echo [2/4] NuGet paketleri restore ediliyor...
dotnet restore
if %errorlevel% neq 0 (
    echo [HATA] Restore islemi basarisiz!
    pause
    exit /b 1
)

echo [3/4] Proje build ediliyor...
dotnet build --configuration Release
if %errorlevel% neq 0 (
    echo [HATA] Build islemi basarisiz!
    pause
    exit /b 1
)

echo [4/4] Test ediliyor...
dotnet run --no-build --configuration Release -- --version
if %errorlevel% neq 0 (
    echo [UYARI] Run testi basarisiz!
)

echo.
echo ========================================
echo   Build basarili!
echo ========================================
echo.
echo Release binary: bin\Release\net8.0\AI_Art_Gallery.dll
echo.

pause
