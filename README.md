# weather-platform

Weather platform containing an API, Azure Function integration with OpenWeather, and IaC for Azure infrastructure deployment.

## Initial plan

### Data Sync

- **Azure Function** with a schedule trigger that reads current weather data from the OpenWeather API.

### Storage

- **Azure SQL Server / Azure SQL Database** to store weather data provided by the sync function.

### API

- **Azure App Service** with public network access enabled.
- Authentication via Bearer token using an App Registration (client ID + client secret).

### CI/CD Pipeline

- **GitHub Actions** CI pipeline: builds the solution and runs tests.
- On success, triggers an **Azure DevOps** pipeline responsible for deploying infrastructure and application code.

### Data Access

- **Entity Framework Core** with a repository handling all database operations.
- Database migrations applied automatically at deploy time.
