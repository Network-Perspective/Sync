# Sample Requests

This document describes the data formats sent to Network Perspective from the worker connector during synchronization. Sample files in this directory demonstrate the structure and content of the data streams.

Network Perspective processes two types of data streams:

1. **Unhashed Data Stream** - Contains plaintext data used for account creation
2. **Hashed Data Stream** - Contains hashed/pseudonymized data used for analytical processing


## Unhashed Data Stream

### [users-sample.json](./users-sample.json)

This file contains plaintext employee information used for synchronization of account list and assigning users to appropriate reports. This is the only data stream that contains unhashed, personally identifiable information.

**Key fields:**
- `connectorId`: Unique identifier for the data source connector
- `users`: Array of user objects containing:
  - `email`: Primary email address (unhashed)
  - `ids`: Dictionary of identifiers including:
    - `Email`: Email address (unhashed)
    - `Office365Id`: Microsoft 365 user ID (unhashed)
    - `Username`: User login name (unhashed)
  - `props`: Additional user properties:
    - `Name`: Full name of the employee (unhashed)
    - `Department`: Array of department names (unhashed)

## Hashed Data Stream

All data in this section is pseudonymized with identifiers hashed using an irreversible HMAC algorithm with a key that only resides in the customer's possession.

### [sync-started.json](./sync-started-sample.json)

Signals the start of a synchronization batch. This marks the beginning of data transfer for a specific time period.

**Key fields:**
- `connectorId`: Unique identifier for the data source connector
- `syncPeriodStart`: ISO 8601 timestamp for the beginning of the sync period
- `syncPeriodEnd`: ISO 8601 timestamp for the end of the sync period

### [groups.json](./groups-sample.json)

Contains organizational group information (departments, teams, etc.) with hashed identifiers.

**Key fields:**
- `connectorId`: Unique identifier for the data source connector
- `groups`: Array of group objects containing:
  - `id`: Hashed group identifier
  - `name`: Group name (department, team, etc.)
  - `category`: Group type classification

### [entites.json](./entites-sample.json)

Contains hashed employee data with their organizational groupings. Note: The filename has a typo ('entites' instead of 'entities') but this is maintained for compatibility.

**Key fields:**
- `connectorId`: Unique identifier for the data source connector
- `entites`: Array of entity objects containing:
  - `changeDate`: ISO 8601 timestamp when the entity data was last modified
  - `ids`: Dictionary of hashed identifiers including:
    - `Email`: Hashed email address
    - `Office365Id`: Hashed Microsoft 365 user ID
    - `Username`: Hashed user login name
  - `groups`: Array of hashed group identifiers that the entity belongs to
  - `relationships`: Array of relationship objects containing:
    - `relationshipName`: Name of the relationship (currently only `Supervisor` relationship is supported)
    - `targetIds`: Dictionary of hashed identifiers for the relationship target

### [interactions.json](./interactions-sample.json)

Contains hashed interaction data between employees representing communications like emails, meetings, and chats.

**Key fields:**
- `connectorId`: Unique identifier for the data source connector
- `interactions`: Array of interaction objects containing:
  - `interactionId`: Unique identifier for the interaction (helps identify duplicates)
  - `when`: ISO 8601 timestamp when the interaction occurred
  - `sourceIds`: Dictionary of hashed identifiers for the interaction source (sender)
  - `targetIds`: Dictionary of hashed identifiers for the interaction target (recipient)
  - `eventId`: Hashed identifier of the original event (email, meeting, chat message)
  - `channelId`: Hashed identifier of the communication channel (for chat messages)
  - `durationMinutes`: Duration of the interaction in minutes (for meetings)
  - `label`: Array of tags classifying the interaction type (Email, Chat, Meeting, etc.)

Note: External interactions use the literal string "external" instead of a hash value, representing people outside the organization.

### [sync-completed.json](./sync-completed-sample.json)

Signals the completion of a synchronization batch, including success/failure status.

**Key fields:**
- `connectorId`: Unique identifier for the data source connector
- `syncPeriodStart`: ISO 8601 timestamp matching the start sync message
- `syncPeriodEnd`: ISO 8601 timestamp matching the start sync message
- `success`: Boolean indicating if the sync completed successfully
- `message`: Additional information or error details if sync failed

If for any reason sync fails, the connector should send `sync-completed` with `success` set to `false` and `message` containing the error details. In such case all data sent before should be considered invalid and discarded. Similarly if no sync-completed message is received before a new sync-started message is received, the connector should consider the previous sync failed and discard all data sent before.

## Data Flow

The typical sequence of data transmission during synchronization:

1. Transmission of unhashed user data for account list synchronization
2. Synchronization batch that follow this pattern:
   - Send `sync-started`
   - Send `groups` data
   - Send `entities` data
   - Send `interactions` data
   - Send `sync-completed`

This ensures Network Perspective receives all necessary data while maintaining privacy through pseudonymization.

## Identifiers

Requests are using multiple identifiers to ensure data consistency (e.g. `Email` and `Office365Id`). Some of the identifiers are specific to the connector type (e.g. `Office365Id`) and others are generic (e.g. `Email`), this allows for linking data between different kinds of connectors (e.g. Slack and Office 365)
