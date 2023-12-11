# OrionSaaS Demo

## Setup authentication

Add users and their corresponding activation codes in the users section
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

## Setup new product

1) Add new product with edition and offering
2) Add features with the following keys: Calendar, ProjectPlanning, Collaboration, Reporting, ReportingAdvanced, Security
3) Add string attributes with following keys: CompanyName, PlanName
4) Add consumption token with key: CT1
5) Add element pool with key: EP1
6) Add floating feature with key: FF1
7) Add entitlement and activate if necessary

## Setup appsettings.json

Open appsettings.json and set following parameters in Zentitle section:
```
  "Zentitle": {
    "ClientId": "{Your api client ID}",
    "ClientSecret": "{Your API Client Secret}",
    "AuthServiceUrl": "{Zentitle Auth Url}",
    "TenantId": "{Your Tenant ID}}",
    "ZentitleUrl": "{Zentitle API URL}",
    "Entitlement": {
      "ProductId": "{Your Product ID}"
    }
  }
```

## Create Management API Client
We use NSwag to generate Management API client.

The Zentitle2 Management API documentation page contains the current openAPI specification file. 
Documentation link can be found in Zentittle 2 under Account > API Credentials in Management API Details box.

Download openAPI specification file and save it in the Zentitle/nswag folder under the name openapi.json

To generate client run following command from `Zentitle/nswag` directory
```
nswag run
```
It is reading the OpenAPI specification from API running locally (!) and creating client classes in `ZentitleClient.cs` file.

To install nswag run following command
```
dotnet tool install -g NSwag.ConsoleCore
```