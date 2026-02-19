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

| Method | Endpoint                    | Description                                       |
| ------ | --------------------------- | ------------------------------------------------- |
| POST   | `/api/sync/contacts`        | Triggers a sync of contacts from Salesforce       |
| GET    | `/api/sync/contacts`        | Returns contacts with search & pagination         |
| GET    | `/api/sync/contacts/{id}`   | Returns a single contact by ID                    |
| POST   | `/api/sync/contacts/create` | Creates a contact in both Salesforce and local DB |
| DELETE | `/api/sync/contacts/{id}`   | Deletes a contact from the local database         |
| GET    | `/api/sync/history`         | Returns sync history logs                         |

### Query Parameters for GET /api/sync/contacts

| Parameter | Default | Description                                |
| --------- | ------- | ------------------------------------------ |
| search    | null    | Filter contacts by name, email, or company |
| page      | 1       | Page number for pagination                 |
| pageSize  | 10      | Number of results per page                 |

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

## Dashboard

A built-in web dashboard is available at `/index.html` when running the application.

Features:

- View all synced contacts with search and pagination
- Trigger manual sync from Salesforce
- Create new contacts (syncs to both Salesforce and local DB)
- Delete contacts
- View sync history with status tracking
- Real-time stats (total contacts, last sync time, syncs today)

## Testing

Run the unit tests:

```bash
cd SalesForceSync.Tests
dotnet test
```

Tests cover:

- GET contacts (returns all, search filtering, pagination)
- GET single contact (valid ID, invalid ID returns 404)
- DELETE contact (valid ID, invalid ID returns 404)

## How It Works

1. **Authentication**: The app authenticates with Salesforce using OAuth 2.0 Client Credentials flow
2. **Fetch**: Queries the Salesforce REST API for all Contact records
3. **Sync**: For each contact, checks if it already exists in the local database by Salesforce ID
4. **Upsert**: Creates new contacts or updates existing ones to keep data in sync
