# Use the official .NET runtime as a base image
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Use the SDK image to build the application
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["QuickBiteMenuAPI.csproj", "QuickBiteMenuAPI/"]
RUN dotnet restore "QuickBiteMenuAPI/QuickBiteMenuAPI.csproj"
COPY . QuickBiteMenuAPI/
WORKDIR "/src/QuickBiteMenuAPI"
RUN dotnet build "QuickBiteMenuAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "QuickBiteMenuAPI.csproj" -c Release -o /app/publish

# Final stage - runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create a directory for the SQLite database
RUN mkdir -p /app/data

# Set environment variables
ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Production

# Ensure the SQLite database is in a persistent location
ENV ConnectionStrings__DefaultConnection="Data Source=/app/data/quickbite_menu.db"

ENTRYPOINT ["dotnet", "QuickBiteMenuAPI.dll"]

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:80/api/menuitem || exit 1