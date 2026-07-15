FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Sales/BrewUp.Sales.McpServer/BrewUp.Sales.McpServer.csproj", "Sales/BrewUp.Sales.McpServer/"]
COPY ["Sales/BrewUp.Sales.Infrastructure/BrewUp.Sales.Infrastructure.csproj", "Sales/BrewUp.Sales.Infrastructure/"]
COPY ["BrewUp.Shared/BrewUp.Shared.csproj", "BrewUp.Shared/"]
COPY ["Sales/BrewUp.Sales.ReadModel/BrewUp.Sales.ReadModel.csproj", "Sales/BrewUp.Sales.ReadModel/"]
COPY ["Sales/BrewUp.Sales.SharedKernel/BrewUp.Sales.SharedKernel.csproj", "Sales/BrewUp.Sales.SharedKernel/"]
RUN dotnet restore "Sales/BrewUp.Sales.McpServer/BrewUp.Sales.McpServer.csproj"
COPY . .
WORKDIR "/src/Sales/BrewUp.Sales.McpServer"
RUN dotnet build "./BrewUp.Sales.McpServer.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./BrewUp.Sales.McpServer.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BrewUp.Sales.McpServer.dll"]
