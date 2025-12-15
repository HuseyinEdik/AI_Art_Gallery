#!/bin/bash
# AI Art Gallery - Docker Start Script (Linux/Mac)

echo "========================================"
echo "AI Art Gallery - Docker Deployment"
echo "========================================"
echo ""

# .env dosyasý kontrolü
if [ ! -f ".env" ]; then
    echo "[HATA] .env dosyasý bulunamadý!"
    echo ".env.example dosyasýndan .env oluþturun:"
    echo "  cp .env.example .env"
    echo ""
    exit 1
fi

echo "[1/4] Docker servisleri baþlatýlýyor..."
docker-compose up -d

echo ""
echo "[2/4] Container durumu kontrol ediliyor..."
docker-compose ps

echo ""
echo "[3/4] Servisler hazýr bekleniyor..."
sleep 10

echo ""
echo "[4/4] Health check yapýlýyor..."
docker-compose ps

echo ""
echo "========================================"
echo "Deployment tamamlandý!"
echo "========================================"
echo ""
echo "Eriþim URL'leri:"
echo "  MVC App:    http://localhost:5000"
echo "  Spring API: http://localhost:8080"
echo "  PostgreSQL: localhost:5432"
echo ""
echo "Loglarý izlemek için:"
echo "  docker-compose logs -f"
echo ""
echo "Servisleri durdurmak için:"
echo "  docker-compose down"
echo ""
