#!/bin/bash
# AI Art Gallery - Docker Stop Script (Linux/Mac)

echo "========================================"
echo "AI Art Gallery - Stopping Docker Services"
echo "========================================"
echo ""

read -p "Volume'larý da silmek istiyor musunuz? (Veritabaný silinir!) [y/N]: " choice

if [[ "$choice" == "y" || "$choice" == "Y" ]]; then
    echo ""
    echo "[UYARI] Volume'lar silinecek! Veritabaný kaybolacak!"
    echo ""
    docker-compose down -v
else
    echo ""
    echo "Container'lar durduruluyor..."
    echo ""
    docker-compose down
fi

echo ""
echo "========================================"
echo "Servisler durduruldu!"
echo "========================================"
echo ""
