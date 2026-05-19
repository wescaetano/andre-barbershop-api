# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source

COPY src/ src/
COPY tests/ tests/

RUN dotnet publish src/BarberShop.Api/BarberShop.Api.csproj \
    --configuration Release \
    --output /app/publish \
    --no-self-contained

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production

CMD ASPNETCORE_URLS=http://+:${PORT:-8080} dotnet BarberShop.Api.dll
