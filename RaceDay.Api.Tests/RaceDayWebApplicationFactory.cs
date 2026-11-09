
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RaceDay.Api.Data;

namespace RaceDay.Api.Tests;

// Creates a test version of the RaceDay API.
public class RaceDayWebApplicationFactory : WebApplicationFactory<Program>
{
    // Gives each factory its own separate test database.
    private readonly string _databaseName =
        "RaceDayTests_" + Guid.NewGuid();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Use the Testing environment.
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // Remove the original database configuration.
            services.RemoveAll<DbContextOptions<RaceDayDbContext>>();

            // Remove existing EF Core database configuration.
            var leftovers = services
                .Where(d => d.ServiceType.IsGenericType &&
                    d.ServiceType.Name.StartsWith(
                        "IDbContextOptionsConfiguration") &&
                    d.ServiceType.GetGenericArguments()
                        .Contains(typeof(RaceDayDbContext)))
                .ToList();

            foreach (var descriptor in leftovers)
            {
                services.Remove(descriptor);
            }

            // Replace SQL Server with an InMemory database.
            services.AddDbContext<RaceDayDbContext>(options =>
                options.UseInMemoryDatabase(_databaseName));
        });
    }

    // Creates an HTTP client for sending API test requests.
    public HttpClient CreateApiClient()
    {
        return CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            HandleCookies = true,
            AllowAutoRedirect = false
        });
    }
}
