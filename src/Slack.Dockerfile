# prepare runtime image
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# build .net app
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore "NetworkPerspective.Sync.sln"
WORKDIR "/src/NetworkPerspective.Sync.Slack"
RUN dotnet build "NetworkPerspective.Sync.Slack.csproj" -c Release -o /app/build
WORKDIR "/src"
RUN dotnet test "NetworkPerspective.Sync.sln" -c Release --filter SkipInCi!=true

# run publish
FROM build AS publish
WORKDIR "/src/NetworkPerspective.Sync.Slack"
RUN dotnet publish "NetworkPerspective.Sync.Slack.csproj" -c Release -o /app/publish

# copy artefacts to final image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NetworkPerspective.Sync.Slack.dll"]
