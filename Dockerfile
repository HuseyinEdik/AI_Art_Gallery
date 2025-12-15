# Build aþamasý
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Proje dosyasýný kopyala ve restore et
COPY ["AI_Art_Gallery.csproj", "./"]
RUN dotnet restore "AI_Art_Gallery.csproj"

# Tüm dosyalarý kopyala ve build et
COPY . .
RUN dotnet build "AI_Art_Gallery.csproj" -c Release -o /app/build

# Publish aþamasý
FROM build AS publish
RUN dotnet publish "AI_Art_Gallery.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime aþamasý
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Publish edilen dosyalarý kopyala
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "AI_Art_Gallery.dll"]
