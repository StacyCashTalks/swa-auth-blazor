# swa-auth-blazor
Holds the code example used for Stacy Cashmore's "Authenticating in Azure Static Web Apps" talk 

## Information about talk

During the talk Stacy starts with a [basic Todo application](./Start) that has no authentication, nor storage - so when the page is refreshed all the Todos are lost 😢

First of all standard authentication is added to the [application](./StandardAuth), along with CosmosDB storage, to allow users to authenticate and save their todo list

Once that is working, the [application](./CustomAuth) is changed to swap the standard authentication to custom authentication using an Auth0 account.

## Setting up the environment

To run the application locally you will need to have on your machine:

- [NodeJs](https://nodejs.org/en/)
- The [Static Web App CLI](https://azure.github.io/static-web-apps-cli/)
- [Azure Function Core Tools](https://learn.microsoft.com/en-us/azure/azure-functions/functions-run-local?tabs=v4%2Cwindows%2Ccsharp%2Cportal%2Cbash)
- [.NET 6](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)
- [CosmosDB Emulator](https://learn.microsoft.com/en-us/azure/cosmos-db/local-emulator?tabs=ssl-netstd21)

## Running the application locally

- For the Standard and Custom auth versions of the application ensure that the CosmosDB Emulator is running
- In a terminal window, go to the folder of the version you would like to run
- Run the following command:

``` ps
swa start run-all
```

This command will start the Blazor WASM application, the Azure Functions and the Static Web App CLI itself. To connect to the running application navigate to

```ps
http://localhost:4280
```

To use the storage of the StandardAuth and CustomAuth solutions login and give add the `authorsed` role to the test user.

When reading/saving todos the CosmosDB database and container will automatically be created by code.

If you encounter please open an issue, PRs are also welcome 😊
