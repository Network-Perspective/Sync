# Custom integration

This document describes json file format that is accepted by Network Perspective API for implementing custom integrations. 

## List of employees 

This api call shall contain list of employees and teams or groups they belong to.  It should be exported monthly or more often. 

Example payload to the [API endpoint](https://app.networkperspective.io/api/docs/#!/SyncHashed/SyncHashed_SyncEntities)
```json
{
  "serviceToken": "your_api_token",
  "entites": [
    {
      "changeDate": "2023-05-30TT00:00:00.000Z",
      "ids": {
        "employee_id": "39954af95558",
        "email": "a440e59f68fa"
      },
      "groups": [
        "a9f6b2c1",
        "d3e4f5a6",
        "b7c8d9e0",
        "f0a1b2c3"
      ],
      "props": {
        "team_id": "a9f6b2c1",
        "employment_date": "1998-10-01"
      },
      "relationships": [
        {
          "relationshipName": "Supervisor",
          "targetIds": {
            "employee_id": "fbc3e7b8e8d7",
            "email": "a440e59f68fa"
          }
        }
      ]
    },
    ...
  ]
}
```
Fields of a single employee record:
* changeDate 
  * date when the row info gathered UTC ISO date format
* ids  
  * list of employee unique identifiers - it is recommended to use at least two identifiers one of which is email address.
  * employee_id
    * hashed employee unique identifier  
    * it might be an email if other identifier is not available
  * email
    * hashed employee lowercase email
* groups
  * list of ids of all groups employee belongs to
* props
  * team_id
    * id of employee team
    * used to distinguish intra-team vs cross-team collaboration
  * employment_date
    * date of employment yyyy-MM-dd
    * recommended but optional. Please skip the field (instead of sending empty value) if information is not available.
    * the date might be rounded to 1st day of month to preserve privacy
* relationships   
  * list of employee relationships - supervisor relationship is required
  * supervisor relationship is described by:
    * relationshipName: "Supervisor"
    * targetIds
      * at least one id of a supervisor employee (ref id field)

All fields except dates (row_date & employment_date) shall be hashed with HMAC algorithm and customer key before sent to Network Perspective. 


## List of groups

This api call shall contain unhashed names of groups & teams within the company that will map to reports visible in Network Perspective UI. 

Example payload to the [API endpoint](https://app.networkperspective.io/api/docs/#!/SyncHashed/SyncHashed_SyncGroups)
```json
{
  "serviceToken": "your_api_token",
  "groups": [
    {
      "id": "a9f6b2c1",
      "name": "Devops Team",
      "category": "Team",
      "parentId": "e2d3f4a5"
    },
    {
      "id": "e2d3f4a5",
      "name": "Engineering Department",
      "category": "Department",
    },
    ...
  ]
}

```

Fields in a group record:
* id 
  * identifier of a group or team employee might belong to
  * these are the same identifiers as in "groups" & "props/team_id" for list of employees request described above
* name 
  * name of the group or team
  * this becomes a title of the report 
* category
  * group category (e.g. Team, Department, Agile Squad, Project, etc.) 
  * reports are organized in categories for better browsing experience. Also if necessary each category might use slightly different report template, use a bit different wording, etc.
* parent_id
  * reference to an id field above
  * used to organize groups into tree like hierarchy 

Fields id & parent_id shall be hashed with HMAC algorithm and customer key before sent to Network Perspective. It can be done with a powershell script provided. 

 
## Users & permissions
We might want to automate synchronization of application users and their permissions. This is optional as users can be also created and assigned permissions via admin UI. However, if there are more than dozens of users to be managed it is a good practice to automate this process in the long run.

Example payload to the [API endpoint](https://app.networkperspective.io/api/docs/#!/SyncHashed/SyncHashed_SyncUsers)
```json
{
  "serviceToken": "your_api_token",
  "users": [
    {
      "email": "john.doe@example.com",
      "groupAccess": [
        "a9f6b2c1"
      ],
      "ids": {
        "email": "john.doe@example.com"
      },
      "props": {
        "name": "John Doe"
      }
    },
  ]
}
```

Fields in the users record:

* email
  * email address (login) of a user that should have access to the application
  * here email address is NOT hashed as it will be used in application login process
* groupAccess
  * list of ids of groups (reports) the user shall have access to
  * Each group_id in the field group_ids shall be hashed individually yielding list of hashed identifiers.
* ids
  * email
    * hashed employee lowercase email
* props
  * optional additional information that is displayed in the application admin panel next to the user that could help the admin manage user permissions manually 
  * "name"
    * employye first and last name


## Signalling batch start / end 
Custom connector should signal start and completion of synchronization. Signalling makes it possible to handle the synchronization in an transactional / atomic way. Either all data in the batch is processed or whole batch is discarded. This is especially important when implementing a connector that synchronized interactions or other data that have to be split into multiple requests. Anyways start / end signalling is a **requirement** and internal model will not be updated if batch start and completion isn't properly signalled.

### Before sending data (SyncStart)
Before sending actual data start of synchronization should be signalled. This is similar to "begin transaction" in sql.

Example payload to the [API endpoint](https://app.networkperspective.io/api/docs/#!/SyncHashed/SyncHashed_ReportStart)
```json
{
  "serviceToken": "your_api_token",
  "syncPeriodStart": "2023-09-20T00:00:00.000Z",
  "syncPeriodEnd": "2023-10-20T00:00:00.000Z",
}
```

### After successful sync (SyncCompleted)
When whole batch was sent successfully, connector should signal success. This is simillar to "commit transaction" in sql. All data that arrived since the SyncStart was signalled will be used to update internal model.

Example payload to the [API endpoint](https://app.networkperspective.io/api/docs/#!/SyncHashed/SyncHashed_ReportCompleted)

```json
{
  "serviceToken": "string",
  "syncPeriodStart": "2023-09-20T00:00:00.000Z",
  "syncPeriodEnd": "2023-10-20T00:00:00.000Z",
  "success": true,
  "message": "OK"
}
```

### After error during sync (SyncError)
If connector encounters any error that prevents it to complete the batch it should signal an error. This is simillar to "rollback transaction" in sql. All data coming from the batch will be discared.

Example payload to the [API endpoint](https://app.networkperspective.io/api/docs/#!/SyncHashed/SyncHashed_ReportCompleted)

```json
{
  "serviceToken": "string",
  "syncPeriodStart": "2023-09-20T00:00:00.000Z",
  "syncPeriodEnd": "2023-10-20T00:00:00.000Z",
  "success": false,
  "message": "Reason why sync failed"
}
```

### Gotchas
Connector might fail in unrecoverable way meaning it was unable to send SyncError signal. This is still fine, as the system assumes only one batch transaction per connector can be running at the same time. Hence next time SyncStart is signalled it will not only start a new batch but also discard any uncompleted batch for the perticular connector (identified with the token).
