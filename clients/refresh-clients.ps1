# prerequisities:
# .\install-swashbuckle-cli.ps1
# .\install-swagger-codegen.ps1

# refresh open-api.jsons 

Copy-Item ..\src\GSuite\NetworkPerspective.Sync.GSuite\bin\Debug\net6.0\NetworkPerspective.Sync.GSuite.xml ..\src\Gsuite\NetworkPerspective.Sync.GSuite\bin\Debug\net6.0\dotnet-swagger.xml
dotnet swagger tofile --output gsuite.json ..\src\Gsuite\NetworkPerspective.Sync.GSuite\bin\Debug\net6.0\NetworkPerspective.Sync.GSuite.dll v1

Copy-Item ..\src\Slack\NetworkPerspective.Sync.Slack\bin\Debug\net6.0\NetworkPerspective.Sync.Slack.xml ..\src\Slack\NetworkPerspective.Sync.Slack\bin\Debug\net6.0\dotnet-swagger.xml
dotnet swagger tofile --output slack.json ..\src\Slack\NetworkPerspective.Sync.Slack\bin\Debug\net6.0\NetworkPerspective.Sync.Slack.dll v1

Copy-Item ..\src\Office365\NetworkPerspective.Sync.Office365\bin\Debug\net6.0\NetworkPerspective.Sync.Office365.xml ..\src\Office365\NetworkPerspective.Sync.Office365\bin\Debug\net6.0\dotnet-swagger.xml
dotnet swagger tofile --output office365.json ..\src\Office365\NetworkPerspective.Sync.Office365\bin\Debug\net6.0\NetworkPerspective.Sync.Office365.dll v1

Copy-Item ..\src\Excel\NetworkPerspective.Sync.Excel\bin\Debug\net6.0\NetworkPerspective.Sync.Excel.xml ..\src\Excel\NetworkPerspective.Sync.Excel\bin\Debug\net6.0\dotnet-swagger.xml
dotnet swagger tofile --output excel.json ..\src\Excel\NetworkPerspective.Sync.Excel\bin\Debug\net6.0\NetworkPerspective.Sync.Excel.dll v1

# generate clients

java -jar swagger-codegen-cli.jar generate -i .\gsuite.json -l typescript-fetch -o .\typescript\gsuite
java -jar swagger-codegen-cli.jar generate -i .\slack.json -l typescript-fetch -o .\typescript\slack
java -jar swagger-codegen-cli.jar generate -i .\office365.json -l typescript-fetch -o .\typescript\office365
java -jar swagger-codegen-cli.jar generate -i .\excel.json -l typescript-fetch -o .\typescript\excel