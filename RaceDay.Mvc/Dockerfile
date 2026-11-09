
# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080

# SDK image for building the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy project files
COPY ["RaceDay.Mvc/RaceDay.Mvc.csproj", "RaceDay.Mvc/"]
COPY ["RaceDay.Contracts/RaceDay.Contracts.csproj", "RaceDay.Contracts/"]

# Restore NuGet packages
RUN dotnet restore "RaceDay.Mvc/RaceDay.Mvc.csproj"

# Copy the remaining source files
COPY . .

# Build the MVC application
WORKDIR "/src/RaceDay.Mvc"
RUN dotnet build "RaceDay.Mvc.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/build

# Publish the application
FROM build AS publish

ARG BUILD_CONFIGURATION=Release

RUN dotnet publish "RaceDay.Mvc.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/publish \
    /p:UseAppHost=false

# Final runtime container
FROM base AS final

WORKDIR /app

COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "RaceDay.Mvc.dll"]
