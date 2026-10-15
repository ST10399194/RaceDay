using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc.Testing;
using RaceDay.Api.Data; 
using RaceDay.Api.Models; 

namespace RaceDay.Api.Tests;

public static class TestDatabaseSeeder
{
    public static void SeedMockData(WebApplicationFactory<Program> factory)
    {
        // Access the internal Dependency Injection container of the running test server
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<RaceDayDbContext>();

        // Wipe any left-over data and recreate structural entities
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();

        // 1. Seed a Mock Organiser User
        var organiserUser = new User
        {
            Email = "admin@raceday.com",
            PasswordHash = "Password123!", 
            Role = "Organiser",
            FirstName = "Admin",
            LastName = "User"
        };

        // 2. Seed a Mock Participant User
        var participantUser = new User
        {
            Email = "runner@raceday.com",
            PasswordHash = "Password123!",
            Role = "Participant",
            FirstName = "Runner",
            LastName = "User"
        };

        context.Users.AddRange(organiserUser, participantUser);
        context.SaveChanges();
    }
}