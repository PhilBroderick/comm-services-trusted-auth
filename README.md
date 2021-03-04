# Introduction

This is a very simple Azure function which acts as a trusted authentication service for 
[Azure Communication Services](https://azure.microsoft.com/en-gb/services/communication-services/), with the `Chat` scope.

This would be used for something like a basic anonymous chat app. It can be extended to only
provide tokens to authenticated users. Currently users are stored in an in memory list for ease of access,
but ideally this would be stored in a more persistent datastore.

## Running Locally

Clone the repository
```shell
git clone https://github.com/PhilBroderick/comm-services-trusted-auth
```

Create an Azure Communication Services resource
```shell
az communication create --name "<communicationName>" --location "Global" --data-location "United States" --resource-group "<resourceGroup>"
```

Retrieve the connection string
```shell
az communication list-key --name "<communicationName>" --resource-group "<resourceGroup>"
```

Add it is an environment variable called `COMM_SERVICES_CONNECTION_STRING`

The solution can then be run through an IDE like Visual Studio or Visual Studio Code.

Making a request to `http://localhost:7071/api/AuthenticateUser` will return a User object
with an Id, Access Token and Expiry timestamp. This same Id can be used to generate a new token.

This token can then be used when setting up a chat thread with Azure Communication Services.


