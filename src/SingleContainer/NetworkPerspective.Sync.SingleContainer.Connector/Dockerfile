FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["SingleContainer/NetworkPerspective.Sync.SingleContainer.Connector/NetworkPerspective.Sync.SingleContainer.Connector.csproj", "SingleContainer/NetworkPerspective.Sync.SingleContainer.Connector/"]
RUN dotnet restore "SingleContainer/NetworkPerspective.Sync.SingleContainer.Connector/NetworkPerspective.Sync.SingleContainer.Connector.csproj"
COPY . .
WORKDIR "/src/SingleContainer/NetworkPerspective.Sync.SingleContainer.Connector"
RUN dotnet build "NetworkPerspective.Sync.SingleContainer.Connector.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "NetworkPerspective.Sync.SingleContainer.Connector.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NetworkPerspective.Sync.SingleContainer.Connector.dll"]
