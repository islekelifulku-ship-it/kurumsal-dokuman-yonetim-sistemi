@echo off
echo Arka planda calisan sistem durduruluyor...
taskkill /F /IM "Kurumsal Doküman ve Karar Yönetim Sistemi.exe" /T >nul 2>&1
taskkill /F /IM "dotnet.exe" /T >nul 2>&1
echo.
echo Sistem basariyla durduruldu.
pause
