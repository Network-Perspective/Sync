# Creating Google Workspace API keys
Follow these steps to set up your Google Workspace API keys:

## Create a new project
https://console.cloud.google.com/projectcreate

Name: "Network Perspective" (this exact name is reqiured)

## Enable APIs
Start by navigating to the API dashboard:
https://console.cloud.google.com/apis/dashboard

Next, enable the following APIs:
* Gmail API<br/> 
https://console.cloud.google.com/apis/library/gmail.googleapis.com
* Google Calendar API <br/>
https://console.cloud.google.com/apis/api/calendar-json.googleapis.com
* Admin SDK<br/>
https://console.cloud.google.com/apis/api/admin.googleapis.com

## Create Service Account Credentials
For a detailed guide on creating service account credentials, you can refer to the [Google Developer's documentation](
https://developers.google.com/workspace/guides/create-credentials#service-account)

* Create Account<br>
https://console.cloud.google.com/iam-admin/serviceaccounts/create

* Generate a private key
Ensure you save the JSON key when prompted. This will be used to authenticate your application.
<img src="google/google-private-key.png">

* Save generated file contents
Keep them confidential as they allow access to your Google workspace.
