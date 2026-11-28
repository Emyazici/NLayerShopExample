# syntax=docker/dockerfile:1

# ---------- build ----------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy only the solution-level and project files first so `dotnet restore`
# is cached as long as dependencies do not change.
COPY NLayerShop.Api/NLayerShop.Api.csproj NLayerShop.Api/
COPY NLayerShop.Business/NLayerShop.Business.csproj NLayerShop.Business/
COPY NLayerShop.Contracts/NLayerShop.Contracts.csproj NLayerShop.Contracts/
COPY NLayerShop.Data/NLayerShop.Data.csproj NLayerShop.Data/
COPY NLayerShop.Domain/NLayerShop.Domain.csproj NLayerShop.Domain/
RUN dotnet restore NLayerShop.Api/NLayerShop.Api.csproj

COPY . .
RUN dotnet publish NLayerShop.Api/NLayerShop.Api.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish \
    /p:UseAppHost=false

# ---------- runtime ----------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# The aspnet:8.0 image listens on port 8080 by default (ASPNETCORE_HTTP_PORTS=8080).
EXPOSE 8080

COPY --from=build /app/publish .

# Run as the non-root user that ships with the .NET 8 images.
USER $APP_UID

ENTRYPOINT ["dotnet", "NLayerShop.Api.dll"]
