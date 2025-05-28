# Base runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
ARG APP_UID=1000
RUN adduser --uid "$APP_UID" --disabled-password appuser
USER appuser
WORKDIR /app
EXPOSE 80
ENV ASPNETCORE_URLS=http://+:80
ENV DOTNET_USE_POLLING_FILE_WATCHER=true

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy csproj files and restore
COPY ["API/API.csproj", "API/"]
COPY ["Monitoring/Monitoring.csproj", "Monitoring/"]
COPY ["FeatureFlags/FeatureFlags.csproj", "FeatureFlags/"]
RUN --mount=type=cache,target=/root/.nuget \
    dotnet restore "API/API.csproj"

# Copy and build all
COPY ["API/", "API/"]
COPY ["Monitoring/", "Monitoring/"]
COPY ["FeatureFlags/", "FeatureFlags/"]
WORKDIR "/src/API"
RUN dotnet build "API.csproj" --configuration "$BUILD_CONFIGURATION" --output /app/build

# Publish stage
FROM build AS publish
WORKDIR "/src/API"
RUN dotnet publish "API.csproj" \
    --configuration "$BUILD_CONFIGURATION" \
    --output /app/publish \
    /p:UseAppHost=false

# Final runtime image
FROM base AS final
WORKDIR /app

# Copy published output (owned by appuser)
COPY --from=publish --chown=appuser:appuser /app/publish .

# Remove write permissions for security
RUN chmod -R a-w /app

# Drop back to non-root
USER appuser

ENTRYPOINT ["dotnet", "API.dll"]
