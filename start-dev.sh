#!/bin/bash
# Hot Reload ile geliþtirme modu
echo "========================================"
echo "  AI Art Gallery - Development Mode"
echo "  Hot Reload Active"
echo "========================================"
echo ""
echo "Dosya deðiþikliklerini otomatik yükler"
echo "Uygulamayý durdurmak için Ctrl+C"
echo ""
echo "Eriþim: http://localhost:5000"
echo ""

export ASPNETCORE_ENVIRONMENT=Development
dotnet watch run
