# weather-platform

Weather platform containing an API, Azure Function integration with OpenWeather, and IaC for Azure infrastructure deployment.

## Initial plan

### Data Sync

#### Location seeding — one-time setup job

- Use the **OpenWeather Geocoding API** to resolve city names to coordinates.
- Runs once at setup time to populate the `Location` table with `lat`, `lon`, and `city`.

#### Weather sync — recurring job

- Use the **OpenWeather One Call API 3.0** (Current and forecasts weather data endpoint).
- Azure Function with a schedule trigger syncs weather data for all locations at a configured interval.
- Each run inserts a new `WeatherRecords` record per location.

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

## Database Schema

### Locations

| Column     | Type            | Notes                        |
|------------|-----------------|------------------------------|
| Id         | int (PK)        | Auto-increment               |
| City       | nvarchar(100)   | City name                    |
| Lat        | decimal(8,6)    | Latitude (-90 to 90)         |
| Lon        | decimal(9,6)    | Longitude (-180 to 180)      |

### WeatherRecords

| Column      | Type            | Notes                                      |
|-------------|-----------------|--------------------------------------------|
| Id          | int (PK)        | Auto-increment                             |
| LocationId  | int (FK)        | References Location.Id                     |
| RecordedAt  | datetime2       | `current.dt` converted from Unix UTC       |
| Sunrise     | datetime2       | `current.sunrise` converted from Unix UTC  |
| Sunset      | datetime2       | `current.sunset` converted from Unix UTC   |
| Temp        | decimal(5,2)    | °C                                         |
| FeelsLike   | decimal(5,2)    | °C                                         |
| Clouds      | int             | Cloudiness %                               |
| Uvi         | decimal(4,2)    | UV index                                   |
| Visibility  | int             | Metres (max 10000)                         |
| WindSpeed   | decimal(5,2)    | m/s                                        |
| Rain1h      | decimal(6,2)    | mm/h — nullable                            |
| Snow1h      | decimal(6,2)    | mm/h — nullable                            |
