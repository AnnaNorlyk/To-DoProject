# Stage 1: Build & Publish
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore
COPY Monitoring/Monitoring.csproj Monitoring/
RUN dotnet restore Monitoring/Monitoring.csproj

# Copy rest of source and publish
COPY Monitoring/ Monitoring/
WORKDIR /src/Monitoring
RUN dotnet publish -c Release -o /app --no-restore

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
ARG APP_UID=1000

RUN adduser --uid ${APP_UID} --disabled-password appuser

WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080

# Copy published output with correct ownership
COPY --from=build --chown=appuser:appuser /app ./

# Strip all write permissions for security
RUN chmod -R a-w /app

USER appuser

EXPOSE 8080
ENTRYPOINT ["dotnet", "Monitoring.dll"]
