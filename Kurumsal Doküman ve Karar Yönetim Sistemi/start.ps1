Write-Host "Kurumsal Dokuman ve Karar Yonetim Sistemi Baslatiliyor..." -ForegroundColor Cyan
Write-Host "Lutfen sunucunun baslamasi icin birkac saniye bekleyin." -ForegroundColor Yellow

# API'yi arka planda başlat
Start-Process "dotnet" -ArgumentList "run" -NoNewWindow

# Sunucunun ayağa kalkması için kısa bir süre bekle
Start-Sleep -Seconds 4

# Uygulamayı varsayılan tarayıcıda aç
Start-Process "http://localhost:5000/index.html"

Write-Host "Tarayici acildi! (Eger acilmazsa http://localhost:5000 veya https://localhost:5001 adresini kontrol edin)" -ForegroundColor Green
Write-Host "Sistemi durdurmak icin terminali kapatabilir veya Ctrl+C yapabilirsiniz." -ForegroundColor White
