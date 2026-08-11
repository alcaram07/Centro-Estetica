# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj files and restore
COPY ["AestheticCenter.Web/AestheticCenter.Web.csproj", "AestheticCenter.Web/"]
COPY ["AestheticCenter.Infrastructure/AestheticCenter.Infrastructure.csproj", "AestheticCenter.Infrastructure/"]
COPY ["AestheticCenter.Core/AestheticCenter.Core.csproj", "AestheticCenter.Core/"]
RUN dotnet restore "AestheticCenter.Web/AestheticCenter.Web.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/AestheticCenter.Web"
RUN dotnet build "AestheticCenter.Web.csproj" -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish "AestheticCenter.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Final
FROM mcr.microsoft.com/dotnet/aspnet:9.0-bookworm-slim AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1

# Sin esto el arranque falla en hosts con las instancias de inotify agotadas
# ("The configured user limit (128) on the number of inotify instances has been
# reached"): CreateBuilder vigila appsettings.json para recargarlo en caliente,
# y si no puede crear el watcher no arranca. El contenedor se redespliega
# entero, asi que esa recarga no aporta nada. El polling cubre cualquier otro
# watcher que quede, sin volver a tocar inotify.
ENV DOTNET_hostBuilder__reloadConfigOnChange=false
ENV DOTNET_USE_POLLING_FILE_WATCHER=1
EXPOSE 8080

ENTRYPOINT ["dotnet", "AestheticCenter.Web.dll"]
