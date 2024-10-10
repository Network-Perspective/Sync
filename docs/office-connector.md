# Office 365 Connector

## Introduction and Goals
Office 365 Connector is a headless application that is the bridge between Office 365 Collaboration Tools and Network Perspective System. It periodically fetches Employees and Employees interactions data from available API and transfers the data to Network Perspective System. The data should be considered confidential and should be treated with special care. The operation of Office 365 Connector shall not interfere with other Office 365 Collaboration Tools consumers.

## Requirements Overview
### What is Office 365 Connector?

The main purpose of Office 365 Connector is to retrieve data collected in Office 365 Collaboration Tools (for example emails, calendar, chat history, and formal structure), and transfer them to the Network Perspective system while ensuring confidentiality and minimizing data leak risk.

### Main features

* Periodically retrieve data from Office 365 Collaboration Tools
* Converting data in such a way that it is not possible to restore from them the original state, but at the same time that the data would be valuable for the analysis an improvement of teams' work habits.
* Transferring retrieved data to Network Perspective system

### Quality Goals
| No  | Quality   | Motivation  |
|---|---|---|
| 1  | Confidentiality  | Employees data is treated as confidential in several dimensions: GDPR directive, Security by Design, Company know how  |
| 2  | Testability  | The architecture should allow easy testing all main building blocks  |
| 3  | Efficiency  | Big data operations require optimizing processing algorithms  |
| 4  | Interoperability  | The application should provide a simple API, compatible with Network Perspective System.  |
| 5  |  Resiliency | The application depends on external services and it needs to be prepared for scenarios like the unavailability of other services.  |

### Architecture Constraints
#### Technical Constraints
| No  | Constraint   | Background  |
|---|---|---|
| 1  | Implementation in C#  | The application should be written using one of .NET new frameworks that have ongoing Microsoft support, especially for fixing security bugs.  |
| 2  | Deployable to major clouds | As Azure is our main cloud provider, the application should be able to run in Azure Cloud, however on private cloud deployement may target other cloud providers. |
| 3  | Auditable  | The interested developer or architect should be able to check out the sources, compile and run the application without problems compiling or installing dependencies. All external dependencies should be available via the package manager of the operation system or at least through an installer.  |
| 4 | OS agnostic | The application should be compatible with Linux and Windows operating System |

#### Conventions
| No  | Constraint   | Background  |
|---|---|---|
| 1  | Coding conventions  | The project uses [Code Convention for C#](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions) The conventions are defined through .editorconfig file, and enforced by Continuous Integration Pipeline.  |
| 2  | Language | English. The project targets an international audience, so only English should be used thought the whole project.|

## System Scope and Context
This chapter describes the environment and context of Office Connector. Who uses the system and on which other system does Office Connector depend.

#### Administrator

Privileged user responsible for managing secrets such as Office 365 Access Token, Hashing Key

### Office 365 Connector

Fetches employees interactions meta-data from Office 365 Api, i.e.:

* Emails metadata - From, To, Cc, Bcc, Timestamp
* Meetings metadata - Participants, Timestamp, Duration, Recurrence Pattern (e.g. Daily, Weekly)
* Public channel metadata
  * Channel participants
  * Who and when sent a message
  * Who and when was mentioned in a message
  * Who and when reacted to a message
  * Formal structure - UserId, Email addresses, ManagerId, Team, Department

Pushes hashed data to Network Perspective. Connector never processes content nor the subject of emails, meeting titles, etc. All personally identifiable data is hashed further in the pipeline and never saved to any persistent storage.

**Pushes anonymous data to Network Perspective. Connector never processes content nor subject of emails, meeting titles, etc.** All personally identifiable data is hashed further in the connector's pipeline and never saved to any persistent storage or sent to Network Perspective.

### Network Perspective System

Exposes API for incoming data.

## Deployment
There are multiple deployment scenarios for the Office 365 Connector. Depending on the client’s architecture and security constraints The application may be deployed on the public cloud or on-premise. 

## Concepts
### Persistency
Office 365 Connector stores only configurations such as, for example, scheduled synchronization jobs. Data retrieved from Office 365 are stored only in-memory for processing time.

### User Interface
Office 365 Connector is Headless and does not provide any Graphical User Interface. The only exposed interface is a simple REST API with endpoints such as creating a network, starting, stopping, and reading the synchronization status (enabled / disabled / error) of the connector.

### Security
For storing secrets such as for, example Office 365 Token, Office 365 Connector uses Azure Key Vault or HCP Vault.

To authenticate to the Azure Key Vault Office 365 Connector uses Azure built-in authentication mechanisms.

To authenticate to Network Perspective System Office 365 Connector uses service tokens. Office 365 Connector is allowed to only push data to Network Perspective without permission to read data already stored in the system.

All communication with external systems is done via encrypted communication channels (TLS).

### Microsoft Graph Api Client
To communicate with Office 365 API Office 365 Connector uses public Microsoft libraries:

* Azure.Identity
* Microsoft.Graph

For Outlook integration the Office Connector needs to be privileged to use the following [Microsoft Graph Privileges](https://learn.microsoft.com/en-us/graph/permissions-reference#userreadall):

* User.Read.All
* Mail.ReadBasic.All
* Calendars.ReadBasic.All
* GroupMember.Read.All

For MS Teams integration:
* Team.ReadBasic.All
* Channel.ReadBasic.All
* ChannelMember.Read.All
* ChannelMessage.Read.All
* GroupMember.Read.All
* Chat.Read.All


## Data Hashing
One of the major requirements is to provide anonymity for fetched data from Office 365 API. It is achieved by using hashing algorithms to convert sensitive information such as employee names to some meaningless string of characters. Office 365 Connector uses the HMAC256 algorithm to convert values to hashed values. Hashing key is stored in Azure Key Vault.

<img src="images/dataflow.png"  width="100%">

## Detailed use of permissions
### User.Read.All
|   |   |   |   |   |
|---|---|---|---|---|
| Identifier  | df021288-bdef-4463-88db-98f22de89214 
| DisplayText | Read all users' full profiles
| Description |	Allows the app to read user profiles without a signed in user.
| AdminConsent | Required
| How it is used  | Office connector uses this permission to read basic information about users in the company such as user email, user id and "Department" attribute. This information is send in both hashed and unhashed streams. Hashed email is used to join data with other connectors such as slack or excel. Hashed used id is used to handle situation when user changes her email address. Hashed Department name is used to mark a person as a member of the department thus allowing to display reports aggregating all users from a department. The email, user id and "Department" are also send separately in unhashed stream to create a list of application users to allow application admin set permissions and roles for each specific user to access report concerning her department.

### Mail.ReadBasic.All
|   |   |   |   |   |
|---|---|---|---|---|
| Identifier  |  	693c5e45-0940-467d-9b8a-1022fb9d42ef
| DisplayText | Read basic mail in all mailboxes
| Description |	Allows the app to read basic mail properties in all mailboxes without a signed-in user. Includes all properties except body, previewBody, attachments and any extended properties.
| AdminConsent | Required
| How it is used  | Office connector uses this permission to read email **metadata**. All metadata excluding email timestamp is hashed, no unhashed information about user correspondence ever leaves the connector. The metadata includes hashed email addresses of the sender and all email recipients along with hashed email id. 

### Calendars.ReadBasic.All
|   |   |   |   |   |
|---|---|---|---|---|
| Identifier  | 8ba4a692-bc31-4128-9094-475872af8a53
| DisplayText | RRead basic details of calendars in all mailboxes
| Description |	Allows the app to read events of all calendars, except for properties such as body, attachments, and extensions, without a signed-in user.
| AdminConsent | Required
| How it is used  | Office connector uses this permission to read calendar events **metadata**. The metadata includes hashed email addresses of all the meeting participants along with hashed event id and a label indicating recurrence of the meeting (nor recurring, daily, weekly, monthly).

### GroupMember.Read.All
|   |   |   |   |   |
|---|---|---|---|---|
| Identifier  | 8ba4a692-bc31-4128-9094-475872af8a53
| DisplayText | Read basic details of calendars in all mailboxes
| Description |	Allows the app to read events of all calendars, except for properties such as body, attachments, and extensions, without a signed-in user.
| AdminConsent | Required
| How it is used  | Office connector can optionally use this to read users group membership to allow whitelist filtering based on Entra Id group name. The connector fetches whitelist definition from Network Perspective API and if it includes group name within the whitelist (or blacklist) it will limit processing of any data to people that belong to specified group. No information about who belongs to which Entra Id group ever leaves the connector.

### Team.ReadBasic.All
|   |   |   |   |   |
|---|---|---|---|---|
| Identifier  | 2280dda6-0bfd-44ee-a2f4-cb867cfc4c1e
| DisplayText | Get a list of all teams
| Description |	Get a list of all teams, without a signed-in user.
| AdminConsent | Required
| How it is used | Office connector uses this permission to iterate over all teams in the company to index messages appearing on MS Teams channels. The permission itself is used as an iterator.

### Channel.ReadBasic.All
|   |   |   |   |   |
|---|---|---|---|---|
| Identifier  | 59a6b24b-4225-4393-8165-ebaec5f55d7a
| DisplayText | Read the names and descriptions of all channels 	
| Description |	Read all channel names and channel descriptions, without a signed-in user.
| AdminConsent | Required
| How it is used | Office connector uses this permission to iterate over all channels within all teams to index messages channels. The permission itself is used as an iterator.

### ChannelMember.Read.All
|   |   |   |   |   |
|---|---|---|---|---|
| Identifier  | 3b55498e-47ec-484f-8136-9013221c06a9
| DisplayText | Read the members of all channels
| Description |	Read the members of all channels, without a signed-in user.
| AdminConsent | Required
| How it is used | Office connector uses this permission to list members of a MS Teams channel. A list of members is used within connector to generate hashed metadata about interactions of people participating within the channel. Details on how the interactions are calculated are described [separate document](interactions.md), however to summarize a new thread created within a channel create an interaction between the message sender and all members of the channel. The channel member emails and ids are never sent unhashed.

### ChannelMessage.Read.All
|   |   |   |   |   |
|---|---|---|---|---|
| Identifier  | 7b2449af-6ccd-4f4d-9f78-e550c193f0d1
| DisplayText | Read all channel messages
| Description |	Allows the app to read all channel messages in Microsoft Teams
| AdminConsent | Required
| How it is used | Office connector uses this permission to read messages from ms teams channels to generate interactions. Details on how the interactions are calculated are described [separate document](interactions.md), however to we shall emphasize that the content of messages is not processed and only hashed metadata is ever sent out of the connector.

### Chat.Read.All
|   |   |   |   |   |
|---|---|---|---|---|
| Identifier  | 7b2449af-6ccd-4f4d-9f78-e550c193f0d1
| DisplayText | Read all chat messages	
| Description |	Allows the app to read all 1-to-1 or group chat messages in Microsoft Teams.
| AdminConsent | Required
| How it is used | Office connector uses this permission to read messages from chat messages to generate interactions. Details on how the interactions are calculated are described [separate document](interactions.md), however to we shall emphasize that the content of messages is not processed and only hashed metadata is ever sent out of the connector.



## Logging
The application uses standard .NET logging mechanisms. In addition, it uses 3rd party library for writing logs to the file system. The application uses different log levels to express different kind of operations. Logs cannot contain any data considered sensitive such as, for example, tokens, employee data, etc.

## Testability
The solution contains XUnit unit tests. The target code coverage is 100% but more important than the value of the metric itself is the coverage of functionalities.