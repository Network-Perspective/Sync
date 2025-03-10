# prepare runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# run publish
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR "/src"
COPY . .
RUN dotnet publish "SingleContainer/Orchestrator/NetworkPerspective.Sync.Orchestrator/NetworkPerspective.Sync.Orchestrator.csproj" -c Release -o /app/publish

# copy artefacts to final image
FROM base AS final
WORKDIR /app
RUN useradd -m connector -u 1000
COPY --from=build --chown=connector:connector /app/publish .
USER connector
ENTRYPOINT ["dotnet", "NetworkPerspective.Sync.Orchestrator.dll"]