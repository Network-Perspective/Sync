# prepare runtime image
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# run publish
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR "/src/NetworkPerspective.Sync.Office365"
RUN dotnet publish "NetworkPerspective.Sync.Office365.csproj" -c Release -o /app/publish

# copy artefacts to final image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NetworkPerspective.Sync.Office365.dll"]
