#!/bin/bash

echo "========================================"
echo "  AI Art Gallery - Build Script"
echo "========================================"
echo ""

echo "[1/4] Temizleniyor..."
dotnet clean
if [ $? -ne 0 ]; then
    echo "[HATA] Clean iþlemi baþarýsýz!"
    exit 1
fi

echo "[2/4] NuGet paketleri restore ediliyor..."
dotnet restore
if [ $? -ne 0 ]; then
    echo "[HATA] Restore iþlemi baþarýsýz!"
    exit 1
fi

echo "[3/4] Proje build ediliyor..."
dotnet build --configuration Release
if [ $? -ne 0 ]; then
    echo "[HATA] Build iþlemi baþarýsýz!"
    exit 1
fi

echo "[4/4] Test ediliyor..."
dotnet run --no-build --configuration Release -- --version
if [ $? -ne 0 ]; then
    echo "[UYARI] Run testi baþarýsýz!"
fi

echo ""
echo "========================================"
echo "  Build baþarýlý!"
echo "========================================"
echo ""
echo "Release binary: bin/Release/net8.0/AI_Art_Gallery.dll"
echo ""
