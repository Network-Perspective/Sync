# prerequisities:
# .\install-swashbuckle-cli.ps1
# .\install-swagger-codegen.ps1

# refresh open-api.jsons 

Copy-Item ..\src\NetworkPerspective.Sync.GSuite\bin\Debug\net6.0\NetworkPerspective.Sync.GSuite.xml ..\src\NetworkPerspective.Sync.GSuite\bin\Debug\net6.0\dotnet-swagger.xml
dotnet swagger tofile --output gsuite.json ..\src\NetworkPerspective.Sync.GSuite\bin\Debug\net6.0\NetworkPerspective.Sync.GSuite.dll v1

Copy-Item ..\src\NetworkPerspective.Sync.Slack\bin\Debug\net6.0\NetworkPerspective.Sync.Slack.xml ..\src\NetworkPerspective.Sync.Slack\bin\Debug\net6.0\dotnet-swagger.xml
dotnet swagger tofile --output slack.json ..\src\NetworkPerspective.Sync.Slack\bin\Debug\net6.0\NetworkPerspective.Sync.Slack.dll v1

# generate clients

java -jar swagger-codegen-cli.jar generate -i .\gsuite.json -l typescript-fetch -o .\typescript\gsuite
java -jar swagger-codegen-cli.jar generate -i .\slack.json -l typescript-fetch -o .\typescript\slack