FROM mcr.microsoft.com/azure-cli AS az

# use azure-powershell as a base image
# as we'll neet to probably fetch some secrets from keyvault
FROM mcr.microsoft.com/powershell:latest AS base
RUN apt-get update && apt-get dist-upgrade -y
WORKDIR /app

# run publish
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR "/src"
COPY . .
RUN dotnet publish "Cli/NetworkPerspective.Sync.Cli/NetworkPerspective.Sync.Cli.csproj" -c Release --self-contained --os linux -o /app/publish -p:PublishSingleFile=true

# copy artefacts to final image
FROM base AS final
RUN apt-get update && apt-get dist-upgrade -y
COPY --from=build /app/publish /usr/local/bin/
# to be overridden
CMD [ "np-sync" ] 
