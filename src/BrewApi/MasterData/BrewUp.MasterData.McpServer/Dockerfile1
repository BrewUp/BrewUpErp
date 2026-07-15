FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["MasterData/BrewUp.MasterData.McpServer/BrewUp.MasterData.McpServer.csproj", "MasterData/BrewUp.MasterData.McpServer/"]
COPY ["BrewUp.Shared/BrewUp.Shared.csproj", "BrewUp.Shared/"]
COPY ["MasterData/BrewUp.MasterData.Infrastructure/BrewUp.MasterData.Infrastructure.csproj", "MasterData/BrewUp.MasterData.Infrastructure/"]
COPY ["MasterData/BrewUp.MasterData.ReadModel/BrewUp.MasterData.ReadModel.csproj", "MasterData/BrewUp.MasterData.ReadModel/"]
COPY ["MasterData/BrewUp.MasterData.Entities/BrewUp.MasterData.Entities.csproj", "MasterData/BrewUp.MasterData.Entities/"]
COPY ["MasterData/BrewUp.MasterData.SharedKernel/BrewUp.MasterData.SharedKernel.csproj", "MasterData/BrewUp.MasterData.SharedKernel/"]
RUN dotnet restore "MasterData/BrewUp.MasterData.McpServer/BrewUp.MasterData.McpServer.csproj"
COPY . .
WORKDIR "/src/MasterData/BrewUp.MasterData.McpServer"
RUN dotnet build "./BrewUp.MasterData.McpServer.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./BrewUp.MasterData.McpServer.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BrewUp.MasterData.McpServer.dll"]
