FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Knowledge/BrewUp.Knowledge.McpServer/BrewUp.Knowledge.McpServer.csproj", "Knowledge/BrewUp.Knowledge.McpServer/"]
COPY ["Knowledge/BrewUp.Knowledge.ReadModel/BrewUp.Knowledge.ReadModel.csproj", "Knowledge/BrewUp.Knowledge.ReadModel/"]
COPY ["Knowledge/BrewUp.Knowledge.Core/BrewUp.Knowledge.Core.csproj", "Knowledge/BrewUp.Knowledge.Core/"]
COPY ["BrewUp.Shared/BrewUp.Shared.csproj", "BrewUp.Shared/"]
COPY ["Knowledge/BrewUp.Knowledge.Infrastructure/BrewUp.Knowledge.Infrastructure.csproj", "Knowledge/BrewUp.Knowledge.Infrastructure/"]
COPY ["Knowledge/BrewUp.Knowledge.SharedKernel/BrewUp.Knowledge.SharedKernel.csproj", "Knowledge/BrewUp.Knowledge.SharedKernel/"]
RUN dotnet restore "Knowledge/BrewUp.Knowledge.McpServer/BrewUp.Knowledge.McpServer.csproj"
COPY . .
WORKDIR "/src/Knowledge/BrewUp.Knowledge.McpServer"
RUN dotnet build "./BrewUp.Knowledge.McpServer.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./BrewUp.Knowledge.McpServer.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BrewUp.Knowledge.McpServer.dll"]
