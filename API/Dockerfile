# Base runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
ARG APP_UID=1000
RUN adduser --uid "$APP_UID" --disabled-password appuser
USER appuser
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy csproj first, restore dependencies
COPY ["API/API.csproj", "API/"]
COPY ["Monitoring/Monitoring.csproj", "Monitoring/"]
RUN dotnet restore "API/API.csproj"

# Copy the rest of the code and build
COPY . .
WORKDIR "/src/API"
RUN dotnet build "API.csproj" --configuration "$BUILD_CONFIGURATION" --output /app/build

# Publish stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
WORKDIR "/src/API"
RUN dotnet publish "API.csproj" \
    --configuration "$BUILD_CONFIGURATION" \
    --output /app/publish \
    /p:UseAppHost=false

# Final image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "API.dll"]
