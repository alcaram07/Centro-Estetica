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
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "AestheticCenter.Web.dll"]
