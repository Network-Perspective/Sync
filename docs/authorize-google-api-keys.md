# Authenticate Google service account to access Google Tenant

From your Google Workspace domain’s [Admin console](https://admin.google.com), go to `Main menu > Security > Access and data control > API controls`

In the `Domain wide delegation` pane, select `Manage Domain Wide Delegation`. Click `Add new`.

In the `Client ID` field, enter your service account client id.

<img src="google/authorize-key.png">

In the `OAuth Scopes` field, enter a comma-delimited list of the scopes required for the connector.
```
https://www.googleapis.com/auth/admin.directory.user.readonly, https://www.googleapis.com/auth/calendar.readonly, https://www.googleapis.com/auth/gmail.metadata
```

Click `Authorize`.

