@echo off
REM Hot Reload ile geliþtirme modu
echo ========================================
echo   AI Art Gallery - Development Mode
echo   Hot Reload Active
echo ========================================
echo.
echo Dosya degisikliklerini otomatik yukler
echo Uygulamayi durdurmak icin Ctrl+C
echo.
echo Erisim: http://localhost:5000
echo.

set ASPNETCORE_ENVIRONMENT=Development
dotnet watch run

pause
