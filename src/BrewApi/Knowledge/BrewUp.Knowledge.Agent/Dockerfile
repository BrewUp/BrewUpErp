FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Knowledge/BrewUp.Knowledge.Agent/BrewUp.Knowledge.Agent.csproj", "Knowledge/BrewUp.Knowledge.Agent/"]
COPY ["BrewUp.Shared/BrewUp.Shared.csproj", "BrewUp.Shared/"]
COPY ["Knowledge/BrewUp.Knowledge.SharedKernel/BrewUp.Knowledge.SharedKernel.csproj", "Knowledge/BrewUp.Knowledge.SharedKernel/"]
RUN dotnet restore "Knowledge/BrewUp.Knowledge.Agent/BrewUp.Knowledge.Agent.csproj"
COPY . .
WORKDIR "/src/Knowledge/BrewUp.Knowledge.Agent"
RUN dotnet build "./BrewUp.Knowledge.Agent.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./BrewUp.Knowledge.Agent.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BrewUp.Knowledge.Agent.dll"]
