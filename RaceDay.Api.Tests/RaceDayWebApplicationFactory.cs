using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RaceDay.Api.Data;

namespace RaceDay.Api.Tests;

/// <summary>
/// Boots the real API in memory but swaps SQL Server for an EF Core InMemory database.
/// Every factory instance gets its own uniquely named database, so test classes
/// cannot interfere with each other even when xUnit runs them in parallel.
/// </summary>
public class RaceDayWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = "RaceDayTests_" + Guid.NewGuid();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // Remove the SQL Server registration that Program.cs added.
            services.RemoveAll<DbContextOptions<RaceDayDbContext>>();

            // EF Core 9+ also registers an options-configuration service per context.
            var leftovers = services
                .Where(d => d.ServiceType.IsGenericType
                            && d.ServiceType.Name.StartsWith("IDbContextOptionsConfiguration")
                            && d.ServiceType.GetGenericArguments().Contains(typeof(RaceDayDbContext)))
                .ToList();
            foreach (var descriptor in leftovers)
                services.Remove(descriptor);

            services.AddDbContext<RaceDayDbContext>(options =>
                options.UseInMemoryDatabase(_databaseName));
        });
    }

    /// <summary>
    /// Creates a client with its own cookie jar (= its own session).
    /// The https base address means Secure session cookies are still sent
    /// and UseHttpsRedirection() does not redirect the test requests.
    /// </summary>
    public HttpClient CreateApiClient() =>
        CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            HandleCookies = true,
            AllowAutoRedirect = false
        });
}