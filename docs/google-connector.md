# Google workspace connector

## Introduction and Goals
G-Suite Connector is a headless application that is the bridge between Google Collaboration Tools and Network Perspective System. It periodically fetches Employees and Employees interactions data from available API and transfers the data to Network Perspective System. The data should be considered confidential and should be treated with special care. Operation of G-Suite Connector shall not interfere with other Google Collaboration Tools consumers.

## Requirements Overview
### What is G-Suite Connector?

The main purpose of G-Suite Connector is to retrieve data collected in Google Collaboration Tools (for example emails, calendar, and formal structure), and transfer them to the Network Perspective system while ensuring confidentiality and minimizing data leak risk.

### Main features

* Periodically retrieve data from Google Collaboration Tools
* Converting data in such a way that it is not possible to restore from them the original state, but at the same time that the data would be valuable for the analysis of teams' work habits. 
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


### G-Suite-Connector

Fetches employees interactions meta-data from Google Collaboration Tools, i.e.:

Emails metadata - From, To, Cc, Bcc, Timestamp

Meetings metadata - Participants, Timestamp, Duration, Recurrence Pattern (e.g. Daily, Weekly)

Formal structure - UserId, Email addresses, ManagerId, Team, Department

**Pushes anonymous data to Network Perspective. Connector never processes content nor subject of emails, meeting titles, etc.** All personally identifiable data is hashed further in the connector's pipeline and never saved to any persistent storage or sent to Network Perspective.

### Network Perspective System

Exposes API for incoming data.

## Concepts
### Persistency
G-Suite Connector stores only configurations such as, for example, scheduled synchronization jobs. Data retrieved from Google Collaboration Tools are stored only in-memory for processing time.

### User Interface
G-Suite Connector is Headless and does not provide any Graphical User Interface. The only exposed interface is a simple REST API with endpoints that allow starting, stopping, and reading synchronization status (enabled / disabled / error) of the connector. 

### Security
For storing secrets such as for, example G-Suite Token, G-Suite Connector uses Azure Key Vault or HCP Vault

For authentication to the Azure Key Vault G-Suite Connector uses Azure built-in authentication mechanisms.

For authentication to Network Perspective System G-Suite Connector uses service tokens. G-Suite Connector is allowed to only push data to Network Perspective without permission to read data already stored in the system.

All communication with external systems is done via encrypted communication channels (TLS).

### Google Api Client
To communicate with Google Api G-Suite Connector uses public Google libraries:

* Google.Apis.Admin.Directory.directory_v1
* Google.Apis.Gmail.v1
* Google.Apis.Calendar.v3

G-Suite Connector needs to be privileged to use the following Google Scopes:

* https://www.googleapis.com/auth/admin.directory.user.readonly
* https://www.googleapis.com/auth/gmail.metadata
* https://www.googleapis.com/auth/calendar.readonly

For token rotation additional permissions are required as [described separetaly](create-google-api-keys.md).

Below tables presents what data are used from Google api

#### User Profiles 
ref.
[REST Resource: users  |  Admin console  |  Google for Developers](https://developers.google.com/admin-sdk/directory/reference/rest/v1/users#User)

| No  | Field   | Type  |
|---|---|---|
| 1	| user.primaryEmail| string |
| 2 | user.organizations[0].department | string|
| 3	| user.relations[“manager”] | string |
| 4	| user.emails[].address| string[]

#### Emails 
ref. [REST Resource: users.messages  |  Gmail  |  Google for Developers](https://developers.google.com/gmail/api/reference/rest/v1/users.messages#Message)

| No  | Field   | Type  |
|---|---|---|
| 1	| message.payload.headers[“from“] | string |
| 2	| message.payload.headers[“to”]| string|
| 3	| message.payload.headers[“cc”] | string |
| 4	| message.payload.headers[“bcc”] | string |
| 5	| message.internalDate | int64


#### Meetings 
ref. [REST Resource: users.messages  |  Gmail  |  Google for Developers) ](https://developers.google.com/calendar/api/v3/reference/events)

	
| No  | Field   | Type  |
|---|---|---|
| 1 | event.attendees[].email | string[] |
| 2 | event.start.datetime | DateTime |
| 3	| event.end.datetime| DateTime|
| 4	| event.status | {“confirmed”, “tentative”, “cancelled”} |
| 5	| event.recurrence | string

## Data Hashing
One of the major requirements is to provide anonymity for fetched data from Google Collaboration Tools. It is achieved by using hashing algorithms to convert sensitive information such as employee names to some meaningless string of characters. G-Suite Connector uses the HMAC256 algorithm to convert values to hashed values. Hashing key is stored in a Key Vault.

<img src="images/dataflow.png"  width="100%">

## Logging
The application uses standard .NET logging mechanisms. In addition, it uses 3rd party library for writing logs to the file system. The application uses different log levels to express different kind of operations. Logs cannot contain any data considered sensitive such as, for example, tokens, employee data, etc.

## Testability
The solution contains XUnit unit tests. The target code coverage is 100% but more important than the value of the metric itself is the coverage of functionalities.