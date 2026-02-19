# Config Service

Centralized configuration management service for distributed applications. Capabilities: App/Env management, config substitution, and AI integration.
**AI Optimized**: Secure API access for AI agents to retrieve and manage configurations.

## 📦 Installation

**One-line Install**:
```bash
curl -sL "https://raw.githubusercontent.com/Jeffe747/ConfigService/main/install.sh" | sudo bash
```

**Run Locally**:
```bash
git clone https://github.com/Jeffe747/ConfigService.git
cd ConfigService
dotnet run --project ConfigService
```

**Actions**: Starts service on port **5001**, initializes `config.db` (SQLite), and generates `GlobalAiApiKey`.

## 🔑 Security
*   **Auth**: `X-System-Key` header required for AI endpoints.
*   **Initial Key**: check server logs for "Generated Global AI API Key".
*   **Network**: Listens on port **5001**.

## 🚀 Usage

### 1. Dashboard
Access the web interface to manage apps and configurations visually:
-   **URL**: `http://localhost:5001/`

### 2. AI Workflow
Agents can programmatically manage configurations using the secure API.

**List Apps**:
```bash
curl -H "X-System-Key: <KEY>" http://localhost:5001/api/ai/apps
```

**Fetch Configs**:
```bash
curl -H "X-System-Key: <KEY>" http://localhost:5001/api/ai/config/<EnvID>
```

### 3. App Management (Public/Internal)
**Create App**:
```bash
curl -X POST http://localhost:5001/api/apps -H "Content-Type: application/json" -d '{ "name": "my-service" }'
```


## 🛠 Configuration
*   **Database**: Defaults to SQLite (`config.db`). Supports MSSQL via Environment Variable or `appsettings.json`.
*   **Port**: 5001 (default).

### Persistent Configuration (Systemd)

To persist environment variables (e.g., Database Connection Strings) across updates/reinstalls, use `systemctl edit`.

1.  Open the override file:
    ```bash
    sudo systemctl edit config-service
    ```

2.  Add your environment variables:
    ```ini
    [Service]
    Environment=ConnectionStrings__DefaultConnection="Server=YOUR_SERVER;Database=ConfigService;User Id=YOUR_USER;Password=YOUR_PASSWORD;Trusted_Connection=False;TrustServerCertificate=True;"
    Environment=DatabaseProvider=MSSQL
    ```

3.  Save and restart:
    ```bash
    sudo systemctl restart config-service
    ```

## 📄 License
MIT License. Developed by **Antigravity**, supervised by **Jeffe747**, with implementation support from **GitHub Copilot**.
