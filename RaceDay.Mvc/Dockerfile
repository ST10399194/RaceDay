# Runtime image: smaller image used when the finished website runs.
FROM ://microsoft.com AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080

# SDK image: contains compilers/tools required to restore and build the project.
FROM ://microsoft.com AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy project files first so Docker can cache package restore layers.
COPY ["RaceDay.Mvc/RaceDay.Mvc.csproj", "RaceDay.Mvc/"]
COPY ["RaceDay.Contracts/RaceDay.Contracts.csproj", "RaceDay.Contracts/"]

# Run dotnet restore targeting the MVC application
RUN dotnet restore "RaceDay.Mvc/RaceDay.Mvc.csproj"

# Copy the rest of the application files across
COPY . .
WORKDIR "/src/RaceDay.Mvc"

# Build the project
RUN dotnet build "RaceDay.Mvc.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish stage to optimize binaries for distribution
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "RaceDay.Mvc.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final production stage mapping binaries into runtime image context
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RaceDay.Mvc.dll"]

dll"]