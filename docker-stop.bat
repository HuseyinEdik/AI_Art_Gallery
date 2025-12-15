@echo off
REM AI Art Gallery - Docker Stop Script (Windows)

echo ========================================
echo AI Art Gallery - Stopping Docker Services
echo ========================================
echo.

set /p choice="Volumelari da silmek istiyor musunuz? (Veritabani silinir!) [y/N]: "

if /i "%choice%"=="y" (
    echo.
    echo [UYARI] Volume'lar silinecek! Veritabani kaybolacak!
    echo.
    docker-compose down -v
) else (
    echo.
    echo Container'lar durduruluyor...
    echo.
    docker-compose down
)

echo.
echo ========================================
echo Servisler durduruldu!
echo ========================================
echo.

pause
