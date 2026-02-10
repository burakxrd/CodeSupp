# 1. Build Aşaması (ELİMİZDEKİ ALPINE SÜRÜMÜ)
FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine3.23 AS build
WORKDIR /src

COPY ["CodeSupp.csproj", "./"]
RUN dotnet restore "CodeSupp.csproj"

COPY . .
RUN dotnet publish "CodeSupp.csproj" -c Release -o /app/publish /p:UseAppHost=false

# 2. Run Aşaması (ELİMİZDEKİ ALPINE SÜRÜMÜ)
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine3.23 AS final
WORKDIR /app
EXPOSE 8080

# Alpine Linux'ta SQL ve Türkçe karakter sorunu olmasın diye gerekli kütüphane
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
RUN apk add --no-cache icu-libs

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "CodeSupp.dll"]