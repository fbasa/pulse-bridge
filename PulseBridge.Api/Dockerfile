# 1) Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project files ONLY (better restore cache)
COPY /PulseBridge.Contracts/*.csproj src/PulseBridge.Contracts/
COPY /PulseBridge.Infrastructure/*.csproj src/PulseBridge.Infrastructure/
COPY /PulseBridge.Api/*.csproj src/PulseBridge.Api/

# Restore using the project (pulls transitive refs automatically)
RUN dotnet restore src/PulseBridge.Api/PulseBridge.Api.csproj --nologo

COPY . .
RUN dotnet publish -c Release -o /app/out

# 2) Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app

# Create non-root user
RUN useradd -m appuser
USER appuser

# ENV ASPNETCORE_URLS=http://+:8080
# EXPOSE 8080

COPY --from=build /app/out ./

# Optional: container-level health probe (expects /health endpoint)
HEALTHCHECK --interval=30s --timeout=3s CMD wget -qO- http://127.0.0.1:8080/health || exit 1

ENTRYPOINT ["dotnet", "PulseBridge.Api.dll"]