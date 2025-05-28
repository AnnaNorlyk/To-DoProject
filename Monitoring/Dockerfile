# Use full SDK image for build & publish
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy only the csproj, restore packages
COPY Monitoring/Monitoring.csproj ./Monitoring/
RUN dotnet restore Monitoring/Monitoring.csproj

# Copy the rest of the source, then publish
COPY Monitoring/. ./Monitoring/
WORKDIR /src/Monitoring
RUN dotnet publish -c Release -o /app

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final

ARG APP_UID=1000
RUN adduser --uid $APP_UID --disabled-password appuser

WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080

COPY --from=build --chown=appuser:appuser /app .

# Remove write permissions for security
RUN chmod -R a-w /app


USER appuser


EXPOSE 8080

ENTRYPOINT ["dotnet", "Monitoring.dll"]
