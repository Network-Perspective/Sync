# Deploy-OfficeConnector.ps1

## Overview

This script automates the creation of Azure Entra ID (formerly Azure Active Directory) applications required for syncing Office 365 data with Network Perspective. It registers two applications:

- **Network Perspective Office 365 Sync**: For syncing Mail and Calendar metadata.
- **Network Perspective Office 365 Teams Sync**: For syncing Teams metadata.

The script also guides you through the necessary manual steps to complete the setup, including generating secrets and granting admin consent.

## Prerequisites

Before running the script, ensure you have:

- **Azure CLI** installed and logged in with sufficient permissions to create Azure AD applications.
- **PowerShell** environment to run the script.
- Permissions to create and manage Azure AD applications and to grant admin consent.

## Usage

Run the script using the following command:

    ./Deploy-OfficeConnector.ps1

### Parameters

The script accepts the following optional parameters:

- `-appNameBasic` (string): The display name for the basic Office 365 sync application. Default is `"Network Perspective Office 365 Sync"`.
- `-appNameWithTeams` (string): The display name for the Teams sync application. Default is `"Network Perspective Office 365 Teams Sync"`.

**Example:**

    ./Deploy-OfficeConnector.ps1 -appNameBasic "My Basic App" -appNameWithTeams "My Teams App"

## Script Steps

1. **Register Entra ID Applications**: The script registers two applications in Azure Entra ID.
   - Checks if applications with the specified names already exist.
   - Offers the option to delete and recreate existing applications or use them as is.
2. **Add Permissions**: The script adds the required permissions to each application.
   - For the basic app:
     - `User.Read.All`
     - `Calendars.ReadBasic.All`
     - `Mail.ReadBasic.All`
     - `GroupMember.Read.All`
   - For the Teams app (in addition to the above):
     - `Team.ReadBasic.All`
     - `Channel.ReadBasic.All`
     - `ChannelMember.Read.All`
     - `ChannelMessage.Read.All`
     - `Chat.Read.All`
3. **Output Instructions**: After registering the applications and adding permissions, the script outputs manual steps to:
   - Generate client secrets for each application.
   - Save the application IDs and secrets in a Key Vault.
   - Grant admin consent for the applications.

## Manual Steps

### 1. Generate Client Secrets

For each application, generate a client secret using the Azure CLI.

#### Basic App

    az ad app credential reset --id <appIdBasic> --display-name "app-secret"

#### Teams App

    az ad app credential reset --id <appIdWithTeams> --display-name "app-secret"

Replace `<appIdBasic>` and `<appIdWithTeams>` with the application IDs provided by the script.

### 2. Save Credentials in Key Vault

Save the following values in your Key Vault:

- `microsoft-client-basic-id`: `<appIdBasic>`
- `microsoft-client-basic-secret`: The secret generated for the basic app.
- `microsoft-client-with-teams-id`: `<appIdWithTeams>`
- `microsoft-client-with-teams-secret`: The secret generated for the Teams app.

### 3. Grant Admin Consent

Grant admin consent for each application by opening the following URLs in a web browser:

#### Basic App (Mail & Calendar Metadata Sync)

    https://login.microsoftonline.com/common/adminconsent?client_id=<appIdBasic>

#### Teams App (Teams Metadata Sync)

    https://login.microsoftonline.com/common/adminconsent?client_id=<appIdWithTeams>

Sign in with an account that has global administrator permissions to grant the necessary consents.

## Troubleshooting

- **Insufficient Permissions**: Ensure you have the necessary permissions to create Azure AD applications and grant admin consent.
- **Azure CLI Errors**: Make sure you have the latest version of Azure CLI installed and that you're logged into the correct Azure account.
- **Application Already Exists**: If the application already exists and you choose not to delete it, ensure it has the correct permissions and settings as specified.

## Notes

- The script uses the Azure CLI to interact with Azure AD. Ensure that Azure CLI is installed and accessible in your PowerShell environment.
- Application IDs and secrets are sensitive information. Handle them securely and avoid exposing them in public repositories or logs.
- The script requires an interactive session to confirm deletion of existing applications if found.

## Support

For any issues or questions, please contact [contact@networkperspective.io](mailto:contact@networkperspective.io).

---

**Example Execution:**

    ./Deploy-OfficeConnector.ps1

Upon running the script, follow the prompts and complete the manual steps as instructed.
