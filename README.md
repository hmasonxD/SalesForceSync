# Salesforce Contact Sync Service

A .NET Web API that provides **bidirectional contact synchronization** between Salesforce and a local SQL Server database using the Salesforce REST API with OAuth 2.0 Client Credentials authentication. Features incremental sync, conflict detection with last-write-wins resolution, and a real-time web dashboard.

## Tech Stack

- .NET 8 Web API
- Entity Framework Core
- SQL Server (LocalDB)
- Salesforce REST API
- OAuth 2.0 (Client Credentials Flow)
- Swagger / OpenAPI
- MSTest + Moq (Unit Testing)

## Features

- **OAuth 2.0 Authentication**: Client Credentials flow with automatic token management
- **Bidirectional Sync**: Create, update, and sync contacts between Salesforce and local database
- **Incremental Sync**: Only fetches contacts modified since the last successful sync using `LastModifiedDate`
- **Conflict Detection & Resolution**: Last-write-wins strategy with full conflict logging — no data is silently lost
- **Background Sync Service**: Automatic sync on a configurable interval (default: 30 minutes)
- **Upsert Logic**: Creates new contacts or updates existing ones based on Salesforce ID
- **Search & Pagination**: Filter contacts by name, email, or company with paginated results
- **Sync History & Stats**: Track every sync operation with status, timing, and error messages
- **Web Dashboard**: Built-in UI for managing contacts, triggering syncs, and viewing history

## Swagger UI

When running in development mode, interactive API documentation is available at:

```
http://localhost:5199/swagger
```

You can test all endpoints directly from the browser.

## API Endpoints

| Method | Endpoint                    | Description                                           |
| ------ | --------------------------- | ----------------------------------------------------- |
| POST   | `/api/sync/contacts`        | Triggers a sync of contacts from Salesforce           |
| GET    | `/api/sync/contacts`        | Returns contacts with search & pagination             |
| GET    | `/api/sync/contacts/{id}`   | Returns a single contact by ID                        |
| POST   | `/api/sync/contacts/create` | Creates a contact in both Salesforce and local DB     |
| PUT    | `/api/sync/contacts/{id}`   | Updates a contact in both Salesforce and local DB     |
| DELETE | `/api/sync/contacts/{id}`   | Deletes a contact from the local database             |
| GET    | `/api/sync/history`         | Returns sync history logs                             |
| GET    | `/api/sync/stats`           | Returns sync stats (syncs today, last status, totals) |
| GET    | `/api/sync/conflicts`       | Returns conflict resolution logs                      |

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
│   └── SyncController.cs            # API endpoints (CRUD, sync, stats, conflicts)
├── Data/
│   ├── AppDbContext.cs               # Entity Framework DB context
│   └── AppDbContextFactory.cs       # Design-time DB context factory
├── Models/
│   ├── Contact.cs                   # Contact data model
│   ├── SyncLog.cs                   # Sync history tracking model
│   └── ConflictLog.cs               # Conflict resolution log model
├── Services/
│   ├── SalesforceAuthService.cs     # OAuth 2.0 authentication
│   ├── SalesforceContactService.cs  # Contact sync, create, update logic
│   └── SyncBackgroundService.cs     # Automatic background sync service
├── wwwroot/
│   └── index.html                   # Web dashboard
├── Program.cs                       # App configuration & DI setup
└── appsettings.json                 # App settings
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

## How It Works

1. **Authentication**: The app authenticates with Salesforce using OAuth 2.0 Client Credentials flow, sending the Client ID and Client Secret to Salesforce's token endpoint to receive an access token
2. **Incremental Fetch**: Queries the Salesforce REST API for contacts modified since the last successful sync using `LastModifiedDate`. On first run, fetches all contacts
3. **Conflict Detection**: For each contact, compares local and Salesforce `LastModifiedDate` timestamps. If data differs, logs both versions to the ConflictLog table before resolving
4. **Last-Write-Wins Resolution**: The version with the more recent `LastModifiedDate` wins. If Salesforce is newer, local data is overwritten. If local is newer, the Salesforce update is skipped
5. **Bidirectional Updates**: Contacts can be created and updated from the API, which pushes changes to Salesforce first, then saves locally. The system is eventually consistent — if a local save fails, the next sync cycle self-corrects
6. **Background Sync**: A hosted background service runs automatic syncs on a configurable interval (default: 30 minutes), using scoped DI to properly manage DbContext lifetime

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

## What I Learned

- **OAuth 2.0 Authentication**: Implementing Client Credentials flow and managing access tokens for server-to-server communication
- **Data Synchronization**: Designing incremental sync with `LastModifiedDate` filtering to minimize API calls
- **Conflict Resolution**: Building a last-write-wins strategy with conflict logging for data recovery and audit trails
- **DI Lifetime Management**: Understanding scoped vs singleton services — using `IServiceProvider.CreateScope()` in background services to properly manage DbContext lifetime
- **REST API Design**: Building a full CRUD API with search, pagination, and proper HTTP status codes
- **Entity Framework Core**: Migrations, upsert patterns, and LINQ queries for efficient database operations
- **Testing**: Writing unit tests with in-memory databases and Moq for service isolation

---
