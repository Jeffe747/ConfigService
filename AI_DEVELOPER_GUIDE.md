# AI Developer Guide - ConfigService

> **STATUS: ACTIVE**
> **CRITICAL**: This file MUST be kept up-to-date.

## 1. Project Overview
**ConfigService** is a .NET Web API that manages application configurations across environments. It provides a REST API for configuration substitution and a web dashboard for management.

## 2. Key Architecture

### Tech Stack
- **Backend**: .NET 10.0 Web API
- **Frontend**: Vanilla JS, HTML, CSS (in `wwwroot`)
- **Database**: SQLite (default) or MSSQL (configurable). EF Core is used for data access.

### Core Logic
- **Substitution**: The service takes a JSON template and replaces values based on the stored configuration for the given App/Env.
- **Persistence**: 
    - `ConfigContext` manages `Applications`, `Environments`, and `ConfigItems`.
    - Migration logic runs on startup (`Program.cs`).

### API Endpoints
- `GET /api/apps` - List apps
- `POST /config/{appName}/{envName}` - Substitute JSON template. Requires `X-App-Key` header.

## 3. Workflows

### Adding a New Feature
1. **Model**: Update `Models/` if needed.
2. **Migration**: Run `dotnet ef migrations add <Name>` and update database.
3. **Controller**: Add endpoints in `Controllers/`.
4. **Frontend**: Update `wwwroot/app.js` and `index.html`.

### Configuration Substitution
To use the substitution service:
1. **Get API Key**: Create an App in the dashboard and copy the API Key.
2. **Send Request**:
   ```bash
   curl -X POST http://localhost:5000/config/MyApp/Prod \
        -H "X-App-Key: <YOUR_KEY>" \
        -H "Content-Type: application/json" \
        -d '{ "ConnectionStrings": { "Default": "ReplaceMe" } }'
   ```

## 4. Development Rules
1. **Migrations**: Always check for pending migrations.
2. **Frontend**: Keep it simple (Vanilla JS). No build steps required.
3. **AI Friendly**: Ensure endpoints return clear JSON errors.
