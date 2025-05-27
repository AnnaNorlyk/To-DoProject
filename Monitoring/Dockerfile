# Use the full SDK image for build & publish
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
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080

# Copy the published output
COPY --from=build /app .  

ENTRYPOINT ["dotnet", "Monitoring.dll"]
