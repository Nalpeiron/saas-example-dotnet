# OrionSaaS Demo

## Set up authentication

Add users and their corresponding activation codes in the `Users` section, which is shown below:

```
  "Users": [
    {
      "Email": "john@example.com",
      "Password": "123456!",
      "ActivationCode": "KHPZ-2364-M7H1-7TA9"
    },
    {
      "Email": "mat@example.com",
      "Password": "123456!",
      "ActivationCode": "ZQ7G-8779-H1XE-KLM1"
    }
  ],
```

## Set up new product

To set up a new product, do the following:

1. Add a new product with an edition and an offering.
1. Add features with the following keys: `Calendar`, `ProjectPlanning`, `Collaboration`, `Reporting`, `ReportingAdvanced`, and `Security`.
1. Add string attributes with following keys: `CompanyName` and `PlanName`.
1. Add usage count adv. feature with the `CT1` key.
1. Add element pool adv, feature with the `EP1` key.
1. Add an entitlement and activate, if necessary.

## Set up appsettings.json

In the `appsettings.json` file, the `Zentitle` section, set the following parameters:

```
  "Zentitle": {
    "ClientId": "<your-api-client-id>",
    "ClientSecret": "<your-api-client-secret>",
    "AuthServiceUrl": "<zentitle-auth-url>",
    "TenantId": "<your-tenant-id>",
    "ZentitleUrl": "<zentitle-api-url>",
    "Entitlement": {
      "ProductId": "<your-product-id>"
    }
  }
```

## Create Management API Client

We use NSwag to generate the Management API client. To generate the client, do the following:

1. If you haven't done so already, install `nswag` by running the following:

   ```
   dotnet tool install -g NSwag.ConsoleCore
   ```

1. In Zentitle 2, from the **Management API Details** box, go to **Account** > **API Credentials** and download the openAPI specification (OAS), saving it under `Zentitle/nswag` as `openapi.json`.

1. Navigate to the `Zentitle/nswag` directory.

1. Generate the client by running `nswag run`.

> [!note]
> `nswag` reads the OAS from the API running locally, which creates client classes in the `ZentitleClient.cs` file.

