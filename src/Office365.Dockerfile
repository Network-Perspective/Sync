# prepare runtime image
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# run publish
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR "/src"
COPY . .
RUN dotnet publish "NetworkPerspective.Sync.Office365/NetworkPerspective.Sync.Office365.csproj" -c Release -o /app/publish

# copy artefacts to final image
FROM base AS final
WORKDIR /app
RUN useradd -m connector -u 1000
COPY --from=build --chown=connector:connector /app/publish .
USER connector
ENTRYPOINT ["dotnet", "NetworkPerspective.Sync.Office365.dll"]
