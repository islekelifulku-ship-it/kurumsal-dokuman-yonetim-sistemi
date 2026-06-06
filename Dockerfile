# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Kurumsal Doküman ve Karar Yönetim Sistemi/Kurumsal Doküman ve Karar Yönetim Sistemi.csproj", "Kurumsal Doküman ve Karar Yönetim Sistemi/"]
RUN dotnet restore "./Kurumsal Doküman ve Karar Yönetim Sistemi/Kurumsal Doküman ve Karar Yönetim Sistemi.csproj"
COPY . .
WORKDIR "/src/Kurumsal Doküman ve Karar Yönetim Sistemi"
RUN dotnet build "./Kurumsal Doküman ve Karar Yönetim Sistemi.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Kurumsal Doküman ve Karar Yönetim Sistemi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
# Sqlite dosyasının uygulamanın çalıştığı dizine yazılabilmesi için
RUN mkdir -p /app/data
# app.db veritabanı dosyasının izinlerini ayarla (çalışma zamanında oluşacağı için klasöre izin verilir)
RUN chmod 777 /app
ENTRYPOINT ["dotnet", "Kurumsal Doküman ve Karar Yönetim Sistemi.dll"]
