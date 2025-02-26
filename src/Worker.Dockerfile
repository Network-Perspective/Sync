# prepare runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# run publish
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG VERSION=1.0.0
WORKDIR "/src"
COPY . .
RUN dotnet publish "SingleContainer/Worker/NetworkPerspective.Sync.Worker/NetworkPerspective.Sync.Worker.csproj" -c Release -o /app/publish /p:Version=$VERSION

# copy artefacts to final image
FROM base AS final
WORKDIR /app
RUN useradd -m connector -u 1000
COPY --from=build --chown=connector:connector /app/publish .
USER connector
ENTRYPOINT ["dotnet", "NetworkPerspective.Sync.Worker.dll"]