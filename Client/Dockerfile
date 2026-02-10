# ----------------------------
# 1. AŞAMA: Frontend Derleme (Client Klasörü)
# ----------------------------
FROM node:20-alpine AS frontend-build
WORKDIR /frontend

# Client klasöründeki package.json dosyasını içeri al
COPY Client/package*.json ./
RUN npm install

# Client klasöründeki TÜM dosyaları (src dahil) içeri al
COPY Client/ .

# Vite ile derle (Bu işlem 'dist' klasörünü oluşturur)
RUN npm run build

# ----------------------------
# 2. AŞAMA: Backend Derleme (CodeSupp Klasörü)
# ----------------------------
FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine3.23 AS backend-build
WORKDIR /backend

# Proje dosyasını kopyala ve restore et
COPY CodeSupp/CodeSupp.csproj ./
RUN dotnet restore

# Backend kodlarını kopyala
COPY CodeSupp/ .

# --- KRİTİK NOKTA ---
# Yukarıda derlediğimiz Frontend dosyalarını (dist), 
# Backend'in çalışacağı "wwwroot" klasörüne otomatik kopyalıyoruz.
# Sen hiçbir şeye dokunmuyorsun.
COPY --from=frontend-build /frontend/dist ./wwwroot

# Uygulamayı yayınla (Publish)
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# ----------------------------
# 3. AŞAMA: Çalıştırma (Runtime)
# ----------------------------
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine3.23 AS final
WORKDIR /app
EXPOSE 8080

# Türkçe karakter ve veritabanı desteği
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
RUN apk add --no-cache icu-libs

# Derlenmiş backend ve içindeki frontend'i buraya al
COPY --from=backend-build /app/publish .

ENTRYPOINT ["dotnet", "CodeSupp.dll"]