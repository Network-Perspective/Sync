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
Office 365 Connector stores only configurations such as, for example, scheduled synchronization jobs.

Data retrieved from Office 365 are stored only in-memory for processing time.

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

For Outlook integration the Office Connector needs to be privileged to use the following Microsoft Privileges:

* https://graph.microsoft.com/User.Read.All
* https://graph.microsoft.com/Mail.ReadBasic.All
* https://graph.microsoft.com/Calendars.Read

For MS Teams integration:
* https://graph.microsoft.com/Team.ReadBasic.All
* https://graph.microsoft.com/Channel.ReadBasic.All
* https://graph.microsoft.com/ChannelMember.Read.All
* https://graph.microsoft.com/ChannelMessage.Read.All

## Data Hashing
One of the major requirements is to provide anonymity for fetched data from Office 365 API. It is achieved by using hashing algorithms to convert sensitive information such as employee names to some meaningless string of characters. Office 365 Connector uses the HMAC256 algorithm to convert values to hashed values. Hashing key is stored in Azure Key Vault.

<img src="images/dataflow.png"  width="100%">

## Logging
The application uses standard .NET logging mechanisms. In addition, it uses 3rd party library for writing logs to the file system. The application uses different log levels to express different kind of operations. Logs cannot contain any data considered sensitive such as, for example, tokens, employee data, etc.

## Testability
The solution contains XUnit unit tests. The target code coverage is 100% but more important than the value of the metric itself is the coverage of functionalities.