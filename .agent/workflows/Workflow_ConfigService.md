---
description: Guide for AI developers working on ConfigService
---

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

## 5. AI Configuration Management
The service exposes a specialized API for AI agents to manage configurations globally.

### Authentication
All AI endpoints require the `X-System-Key` header. This key is generated on startup and displayed on the dashboard.

### API Endpoints
- `GET /api/ai/apps` - List all applications.
- `POST /api/ai/apps` - Create an application. Body: `{ "name": "MyApp" }`
- `GET /api/ai/envs/{appId}` - List environments for an application.
- `POST /api/ai/envs` - Create an environment. Body: `{ "appName": "MyApp", "envName": "Prod" }`
- `GET /api/ai/config/{envId}` - List configurations for an environment.
- `POST /api/ai/config` - Upsert a configuration.
- `DELETE /api/ai/config/{id}` - Delete a configuration.

### Example Usage
**Create App:**
```bash
curl -X POST http://localhost:5001/api/ai/apps \
     -H "X-System-Key: <SYSTEM_KEY>" \
     -H "Content-Type: application/json" \
     -d '{ "name": "NewApp" }'
```

**Create Env:**
```bash
curl -X POST http://localhost:5001/api/ai/envs \
     -H "X-System-Key: <SYSTEM_KEY>" \
     -H "Content-Type: application/json" \
     -d '{ "appName": "NewApp", "envName": "Dev" }'
```
**Upsert Config:**
```bash
curl -X POST http://localhost:5001/api/ai/config \
     -H "X-System-Key: <SYSTEM_KEY>" \
     -H "Content-Type: application/json" \
     -d '{
           "envId": 1,
           "key": "NewConfigKey",
           "value": "NewValue"
         }'
```
