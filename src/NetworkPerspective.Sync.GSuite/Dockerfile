FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app

ENV ASPNETCORE_URLS "http://*:80"

EXPOSE 80

COPY . .

ENTRYPOINT ["dotnet", "NetworkPerspective.Sync.GSuite.dll"]