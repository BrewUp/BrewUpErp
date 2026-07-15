FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Warehouse/BrewUp.Warehouse.McpServer/BrewUp.Warehouse.McpServer.csproj", "Warehouse/BrewUp.Warehouse.McpServer/"]
COPY ["BrewUp.Shared/BrewUp.Shared.csproj", "BrewUp.Shared/"]
COPY ["Warehouse/BrewUp.Warehouse.Infrastructure/BrewUp.Warehouse.Infrastructure.csproj", "Warehouse/BrewUp.Warehouse.Infrastructure/"]
COPY ["Warehouse/BrewUp.Warehouse.ReadModel/BrewUp.Warehouse.ReadModel.csproj", "Warehouse/BrewUp.Warehouse.ReadModel/"]
COPY ["Warehouse/BrewUp.Warehouse.Entities/BrewUp.Warehouse.Entities.csproj", "Warehouse/BrewUp.Warehouse.Entities/"]
COPY ["Warehouse/BrewUp.Warehouse.SharedKernel/BrewUp.Warehouse.SharedKernel.csproj", "Warehouse/BrewUp.Warehouse.SharedKernel/"]
RUN dotnet restore "Warehouse/BrewUp.Warehouse.McpServer/BrewUp.Warehouse.McpServer.csproj"
COPY . .
WORKDIR "/src/Warehouse/BrewUp.Warehouse.McpServer"
RUN dotnet build "./BrewUp.Warehouse.McpServer.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./BrewUp.Warehouse.McpServer.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BrewUp.Warehouse.McpServer.dll"]
