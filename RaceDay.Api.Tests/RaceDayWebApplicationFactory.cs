using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RaceDay.Api.Data;

namespace RaceDay.Api.Tests;

public class RaceDayWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = "RaceDayTests_" + Guid.NewGuid();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<RaceDayDbContext>>();

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

    public HttpClient CreateApiClient() =>
        CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            HandleCookies = true,
            AllowAutoRedirect = false
        });
}