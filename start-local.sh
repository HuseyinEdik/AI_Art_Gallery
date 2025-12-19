#!/bin/bash

echo "========================================"
echo "  AI Art Gallery - Local Baþlat"
echo "========================================"
echo ""

# .NET SDK kontrol
echo ".NET SDK kontrol ediliyor..."
if ! command -v dotnet &> /dev/null; then
    echo "[HATA] .NET SDK bulunamadý!"
    echo "Lütfen .NET 8.0 SDK yükleyin: https://dotnet.microsoft.com/download/dotnet/8.0"
    exit 1
fi
echo "[OK] .NET SDK kurulu: $(dotnet --version)"

echo ""
echo "Backend API kontrol ediliyor..."
if curl -s http://localhost:8080/api/ > /dev/null 2>&1; then
    echo "[OK] Backend API aktif"
else
    echo "[UYARI] Backend API yanýt vermiyor!"
    echo "Lütfen Spring Boot API'nin http://localhost:8080 adresinde çalýþtýðýndan emin olun."
    echo ""
    read -p "Yine de devam etmek istiyor musunuz? (y/n): " choice
    if [ "$choice" != "y" ] && [ "$choice" != "Y" ]; then
        echo "Ýþlem iptal edildi."
        exit 1
    fi
fi

echo ""
echo "========================================"
echo "  Uygulama baþlatýlýyor..."
echo "========================================"
echo ""
echo "Eriþim adresleri:"
echo "  - HTTP:  http://localhost:5000"
echo "  - HTTPS: https://localhost:5001"
echo ""
echo "Backend API: http://localhost:8080/api"
echo ""
echo "Uygulamayý durdurmak için Ctrl+C kullanýn"
echo ""

export ASPNETCORE_ENVIRONMENT=Development
dotnet run --urls "http://localhost:5000;https://localhost:5001"
