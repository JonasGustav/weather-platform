# Weather Platform

Weather platform that collects and exposes historical weather data for configured cities. An Azure Function fetches current weather from the OpenWeather API once per hour and stores the readings in a SQL database. An API provides access to the collected data — querying history, current conditions, and weather extremes (warmest, windiest, foggiest, etc.) across all tracked cities.

---

## Table of Contents

- [Repo Structure](#repo-structure)
- [Resources and How They Interact](#resources-and-how-they-interact)
- [Manual Setup Steps](#manual-setup-steps)
- [Local DevOps Agent](#local-devops-agent)
- [Security](#security)
- [Pipelines](#pipelines)
- [Local Development](#local-development)
- [API Endpoints](#api-endpoints)
- [Database Schema](#database-schema)
- [Limitations](#limitations)
- [Future Plans](#future-plans)

---

## Repo Structure

```text
Code/
  Common/               # Shared library: EF Core models, DbContext, repositories, migrations
  Api/                  # ASP.NET Core Web API
    Controllers/        # WeatherController with all endpoints
    Models/             # DTOs
  Function/             # Azure Functions
    Functions/          # SyncWeather (timer), SeedLocations (HTTP)
    Services/           # OpenWeatherService
    Helpers/            # CityConfigParser
  Tests/                # Unit tests
  Tools/TestSeed/       # Local test data seeding tool
Deployment/
  JG-WeatherHistory.bicep          # Main Bicep template
  Modules/                         # Modular Bicep files per resource type
  params/                          # Per-environment parameter files (d/t/p)
.azure/
  JG-WeatherHistory-ci.yml         # CI pipeline
  JG-WeatherHistory-cd-{env}.yml   # CD pipelines per environment
  templates/                       # Shared deploy job template
  variables/                       # Per-environment variable files
```

**Common** is a shared project referenced by both the API and Function. It contains EF Core models, AppDbContext, migrations, and db repository.

**SeedLocations** is an HTTP-triggered function that resolves city names to coordinates via the OpenWeather Geocoding API and inserts them into the `Locations` table. Cities are configured via the `SeedCities` app setting in the format `CityName,CountryCode|CityName,CountryCode`. Already-seeded locations are skipped based on coordinates.

**SyncWeather** is a timer-triggered function running every hour (`0 0 * * * *`). It fetches current weather for every location in the database and inserts a new weather record. Failures per city are logged individually and do not abort the overall sync.

---

## Resources and How They Interact

All resources per environment are deployed into a single resource group (`rg-JG-WeatherHistory-{env}`).

```text
OpenWeather API
     │
     ▼
Azure Function App
  - SyncWeather (timer, hourly)         
  - SeedLocations (HTTP POST, manual)
     │
     ▼
Azure SQL Database
     │
     ▼
Azure App Service (API)
  - Authenticated via Azure AD Bearer token
  - Reads weather data from SQL
```

- **Function App** and **API** both use system-assigned **managed identities** to authenticate to Key Vault and retrieve the SQL connection string. No credentials are stored in app settings.
- **Function App** runs on the same App Service Plan as the API (shared `B1` plan).
- **VNet** ensures both the Function App and API connect to resources over private subnet routes.
- **Application Insights** is connected to Log Analytics. Alert rules are configured for warning-level logs and missed sync runs (prod only).
- **Azure Storage** is used by the Function App runtime only, not for weather data.

---

## Manual Setup Steps

These steps are one-time and not automated by the pipelines:

1. **Azure AD App Registration**
   - Create an app registration in Entra ID.
   - Add an app role with value `Weather.Read`.
   - Note the Tenant ID and Client ID for the DevOps variable groups.

2. **DevOps Variable Groups**
   Create one secret variable group per environment (`jg-weatherhistory-secrets-{d/t/p}`) containing:
   `subscriptionId`, `sqlAdminLogin`, `sqlAdminPassword`, `agentIpAddress`, `aadTenantId`, `aadClientId`, `alertEmail`

3. **DevOps–GitHub Link**
   Connect the Azure DevOps project to the GitHub repository so the CI pipeline can be triggered on push.

4. **OpenWeather API Key**
   Register at openweathermap.org and obtain a API key. After the first infrastructure deploy, store it in Key Vault as `OpenWeatherApiKey`.

5. **Seed Locations**
   After infrastructure is deployed, trigger `SeedLocations` manually to populate the `Locations` table.

---

## Local DevOps Agent

A self-hosted agent is used while waiting for a Microsoft-hosted agent to be provisioned.

### Switching to a Microsoft-Hosted Agent

When a hosted agent is available:

1. Update the pipeline `pool` from the self-hosted pool name to `vmImage: ubuntu-latest` (or `windows-latest`).
2. The `agentIpAddress` variable can be removed or left empty.

---

## Security

### Networking

Both the Function App and API use VNet integration with dedicated subnets. SQL Server is configured with VNet rules to only allow connections from those two subnets. Key Vault is similarly restricted to the two subnets.

The API has public network access enabled. The Function App blocks all inbound traffic except Azure portal CORS. SCM endpoints are restricted to the deploy agent IP during deployment and blocked otherwise.

### Secrets

All runtime secrets (SQL connection string, OpenWeather API key) are stored in **Azure Key Vault**. The Function App and API retrieve them at startup using managed identity.

### Azure DevOps Variables

Secrets needed at deploy time (before or alongside infrastructure) are stored in DevOps **secret variable groups**, one per environment (`jg-weatherhistory-secrets-{d/t/p}`):

- `subscriptionId` — target Azure subscription
- `sqlAdminLogin` / `sqlAdminPassword` — SQL admin credentials used for EF migrations
- `agentIpAddress` — deploy agent IP for SCM access during deploy
- `aadTenantId` / `aadClientId` — Azure AD app registration details for API auth config
- `alertEmail` — notification email for Azure Monitor alert rules

### Azure AD (Entra)

The API is protected by Azure AD Bearer tokens validated by `Microsoft.Identity.Web`. An **App Registration** defines an app role `Weather.Read`. Callers must be assigned this role and obtain a token via the client credentials flow.

---

## Pipelines

Pipelines are defined in **Azure DevOps** only.

**CI pipeline** (`JG-WeatherHistory-ci.yml`) triggers on changes to `.azure/**`, `Deployment/**`, or `Code/**`. It builds the solution, runs tests, and publishes a build artifact used by the CD pipelines.

**CD pipelines** (`JG-WeatherHistory-cd-{d/t/p}.yml`) d/t trigger on a successful CI run. p only manual trigger. Each calls the shared deploy template which:

1. Creates/updates the resource group
2. Deploys Bicep infrastructure
3. Runs EF Core database migrations
4. Deploys the Function App and API from the CI artifact

**GitHub Actions** is not currently used. A potential future setup would be to run build and test in Actions and trigger the DevOps deploy pipeline via the DevOps REST API on success. This would mainly be for learning purposes, as the current setup already covers the same ground within DevOps.

---

## Local Development

### Prerequisites

- .NET 10 SDK
- Azure Functions Core Tools v4
- SQL Server (local or Azure) with migrations applied
- OpenWeather API key
- Azure CLI logged in (for `DefaultAzureCredential` to reach Key Vault).

### API (`Code/Api`)

Create `Code/Api/appsettings.Development.json` (gitignored):

```json
{
  "KeyVaultUri": "https://<your-keyvault>.vault.azure.net/",
  "AzureAd": {
    "Instance": "https://login.microsoftonline.com/",
    "TenantId": "<tenant-id>",
    "ClientId": "<client-id>",
    "Audience": "api://<client-id>"
  }
}
```

Run with:

```bash
dotnet run --project Code/Api
```

Swagger UI is available at `https://localhost:{port}/swagger`.

### Function App (`Code/Function`)

Create `Code/Function/local.settings.json` (gitignored):

```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
    "KeyVaultUri": "https://<your-keyvault>.vault.azure.net/",
    "SeedCities": "Stockholm,SE|Gothenburg,SE"
  }
}
```

Run with:

```bash
func start --project Code/Function
```

---

## API Endpoints

All endpoints require a Bearer token (`Authorization: Bearer <token>`). The token must include the `Weather.Read` app role.

| Method | Route                  | Description                                           |
|--------|------------------------|-------------------------------------------------------|
| GET    | /weather/current       | Latest reading per location matching city name        |
| GET    | /weather/history       | Paginated history for a city, with optional date range|
| GET    | /weather/warmest       | Highest temperature   |
| GET    | /weather/cloudiest     | Highest cloud coverage                                |
| GET    | /weather/highestuvi    | Highest UV index                                      |
| GET    | /weather/foggiest      | Lowest visibility                                     |
| GET    | /weather/windiest      | Highest wind speed                                    |
| GET    | /weather/mostrain      | Highest rain (1h)                                     |
| GET    | /weather/mostsnow      | Highest snow (1h)                                     |

Swagger UI is available at `https://api-jg-weatherhistory-p.azurewebsites.net/swagger/index.html`. Extreme endpoints (`warmest`, `foggiest`, etc.) default to the latest record per city when no date range is provided.

---

## Database Schema

### Locations

| Column | Type          | Notes                   |
|--------|---------------|-------------------------|
| Id     | int (PK)      | Auto-increment          |
| City   | nvarchar(100) | City name from geocoder |
| Lat    | decimal(8,6)  | Latitude                |
| Lon    | decimal(9,6)  | Longitude               |

### WeatherRecords

| Column     | Type         | Notes                        |
|------------|--------------|------------------------------|
| Id         | int (PK)     | Auto-increment               |
| LocationId | int (FK)     | References Locations.Id      |
| RecordedAt | datetime2    | UTC timestamp of the reading |
| Sunrise    | datetime2?   | UTC, nullable                |
| Sunset     | datetime2?   | UTC, nullable                |
| Temp       | decimal(5,2) | °C                           |
| FeelsLike  | decimal(5,2) | °C                           |
| Clouds     | int          | Cloudiness %                 |
| Uvi        | decimal(4,2) | UV index                     |
| Visibility | int          | Metres (max 10 000)          |
| WindSpeed  | decimal(5,2) | m/s                          |
| Rain1h     | decimal(6,2) | mm/h — nullable              |
| Snow1h     | decimal(6,2) | mm/h — nullable              |

---

## Limitations

### OpenWeather API

The free tier of the OpenWeather One Call API 3.0 allows a maximum of **1 000 calls per day**. With an hourly sync, each city consumes 24 calls/day, limiting the setup to **41 cities** total for all envs. Expanding beyond that or increasing sync frequency requires a paid plan.

### Multiple Cities with the Same Name

When querying the OpenWeather Geocoding API by city name, multiple results can be returned. The `SeedLocations` function uses `CityName,CountryCode` format. This has not been a problem with the limited cities used so far and sync and api has been setup with this in mind, but if the platform expands, this would need a closer look.

---

## Split Resource Groups / Repos

The current setup uses a single resource group per environment. Some potential future splits:

- **Common library as a NuGet package** — `WeatherPlatform.Common` (models, EF, repositories) published as a private NuGet/artifact feed package, giving the API and Function versioned, independent dependencies.
- **Split API and Function into separate repos/pipelines** — independent releases for the sync job and the API.
- **Shared infrastructure resource group** — Key Vault and monitoring etc. could live in a shared RG separate from the application workloads. This would also allow DevOps variable secrets to migrate into that shared Key Vault, reducing the number of variables managed in DevOps directly.

---

## Future Plans

- **More cities** — Requires awareness of the OpenWeather call limit and potential city name collision handling.
- **Restructure resource groups** — split shared infrastructure from applications.
- **Event-driven notifications** — alert on weather events such as a new temperature record or extreme conditions (e.g. via Logic App).
- **Common library as NuGet package** — publish `WeatherPlatform.Common` as a versioned private package rather than a project reference.
- **Split API and Function** — independent repositories and pipelines for the sync function and the API.
- **Tie-handling in extreme endpoints** — endpoints like `/warmest` and `/foggiest` currently return whichever record the database happens to return first when multiple records share the same extreme value. There is no tiebreaker. Could be addressed by returning all tied records instead of just one, or find some tiebreaker, for `warmest` maybe highest `feelsLike` value.
- **Database indexes** — currently only `IX_WeatherReadings_LocationId` exists. At current scale this has little to no impact on performance, but adding indexes for frequently looked up columns would be worth concidering/investigating before expanding to more cities or longer data retention.
