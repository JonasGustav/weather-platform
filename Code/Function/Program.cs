using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WeatherPlatform.Common.Data;
using WeatherPlatform.Common.Repositories;
using WeatherPlatform.Function.Services;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

var keyVaultUri = builder.Configuration["KeyVaultUri"]
    ?? throw new InvalidOperationException("KeyVaultUri app setting is required.");

var secretClient = new SecretClient(new Uri(keyVaultUri), new DefaultAzureCredential());
var sqlConnectionString = secretClient.GetSecret("SqlConnectionString").Value.Value;

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(sqlConnectionString));

builder.Services.AddScoped<ILocationRepository, LocationRepository>();

builder.Services.AddSingleton(secretClient);

builder.Services.AddHttpClient<IOpenWeatherService, OpenWeatherService>();

builder.Build().Run();
