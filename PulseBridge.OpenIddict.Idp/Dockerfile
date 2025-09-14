# 1) Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy only csproj first (for restore layer caching)
COPY PulseBridge.OpenIddict.Idp/*.csproj src/PulseBridge.OpenIddict.Idp/

# Restore using the project (pulls transitive refs automatically)
RUN dotnet restore src/PulseBridge.OpenIddict.Idp/PulseBridge.OpenIddict.Idp.csproj --nologo

# Copy the rest and publish
COPY . .
RUN dotnet publish -c Release -o /app/out
#RUN dotnet publish src/PulseBridge.OpenIddict.Idp/PulseBridge.OpenIddict.Idp.csproj -c Release -o /app/out --no-restore

# 2) Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Create non-root user
RUN useradd -m appuser
USER appuser

# Make port explicit so Traefik & app agree
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/out ./

# Optional: container-level health probe (expects /health endpoint)
#HEALTHCHECK --interval=30s --timeout=3s CMD wget -qO- http://127.0.0.1:8080/health || exit 1

ENTRYPOINT ["dotnet", "PulseBridge.OpenIddict.Idp.dll"]