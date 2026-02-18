# SalesForceSync POC

A .NET Web API that synchronizes contacts from Salesforce into a local SQL Server database using the Salesforce REST API with OAuth 2.0 Client Credentials authentication.

## Tech Stack

- .NET 8 Web API
- Entity Framework Core
- SQL Server (LocalDB)
- Salesforce REST API
- OAuth 2.0 (Client Credentials Flow)

## Features

- Authenticates with Salesforce using Client Credentials OAuth flow
- Fetches contacts from Salesforce via REST API
- Syncs contacts to a local SQL Server database
- Upsert logic: creates new contacts or updates existing ones based on Salesforce ID
- REST API endpoints to trigger sync and view synced contacts

## Swagger UI

When running in development mode, interactive API documentation is available at:

```
http://localhost:5199/swagger
```

You can test all endpoints directly from the browser.

## API Endpoints

| Method | Endpoint             | Description                                   |
| ------ | -------------------- | --------------------------------------------- |
| POST   | `/api/sync/contacts` | Triggers a sync of contacts from Salesforce   |
| GET    | `/api/sync/contacts` | Returns all synced contacts from the database |

## Setup

### Prerequisites

- .NET 8 SDK
- SQL Server LocalDB
- Salesforce Developer Account with a Connected App (External Client App)

### Configuration

1. Clone the repository
2. Set up user secrets for Salesforce credentials:

```bash
dotnet user-secrets init
dotnet user-secrets set "Salesforce:ClientId" "your-consumer-key"
dotnet user-secrets set "Salesforce:ClientSecret" "your-consumer-secret"
```

3. Update `appsettings.json` with your Salesforce org URL:

```json
"Salesforce": {
    "LoginUrl": "https://your-org.develop.my.salesforce.com"
}
```

4. Run database migrations:

```bash
dotnet ef database update
```

5. Run the application:

```bash
dotnet run
```

## Project Structure

```
SalesForceSync/
├── Controllers/
│   └── SyncController.cs        # API endpoints
├── Data/
│   └── AppDbContext.cs           # Entity Framework DB context
├── Models/
│   └── Contact.cs               # Contact data model
├── Services/
│   ├── SalesforceAuthService.cs  # OAuth authentication
│   └── SalesforceContactService.cs  # Contact sync logic
├── Program.cs                    # App configuration
└── appsettings.json              # App settings
```

## How It Works

1. **Authentication**: The app authenticates with Salesforce using OAuth 2.0 Client Credentials flow
2. **Fetch**: Queries the Salesforce REST API for all Contact records
3. **Sync**: For each contact, checks if it already exists in the local database by Salesforce ID
4. **Upsert**: Creates new contacts or updates existing ones to keep data in sync
