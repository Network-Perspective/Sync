FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["SingleContainer/NetworkPerspective.Sync.SingleContainer.Host/NetworkPerspective.Sync.SingleContainer.Host.csproj", "SingleContainer/NetworkPerspective.Sync.SingleContainer.Host/"]
RUN dotnet restore "SingleContainer/NetworkPerspective.Sync.SingleContainer.Host/NetworkPerspective.Sync.SingleContainer.Host.csproj"
COPY . .
WORKDIR "/src/SingleContainer/NetworkPerspective.Sync.SingleContainer.Host"
RUN dotnet build "NetworkPerspective.Sync.SingleContainer.Host.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "NetworkPerspective.Sync.SingleContainer.Host.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NetworkPerspective.Sync.SingleContainer.Host.dll"]
