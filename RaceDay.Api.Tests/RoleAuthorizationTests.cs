using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace RaceDay.Api.Tests;

public class RoleAuthorizationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public RoleAuthorizationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;

        TestDatabaseSeeder.SeedMockData(_factory);
    }

    [Fact]
    public async Task Participant_CreatesEvent_Returns403Forbidden()
    {
        var client = _factory.CreateClient();
        var participantClient = await TestAuthHelper.GetAuthenticatedClientAsync(client, "runner@raceday.com", "Password123!");
        var badEvent = new { EventName = "Restricted Race Entry", EventDate = DateTime.UtcNow.AddDays(7) };

        var response = await participantClient.PostAsJsonAsync("/api/events", badEvent);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task AnonymousUser_AccessesProfile_Returns401Unauthorized()
    {
        var publicClient = _factory.CreateClient();

        var response = await publicClient.GetAsync("/api/profile/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Organiser_CreatesEvent_Returns201Created()
    {
        var client = _factory.CreateClient();
        var organiserClient = await TestAuthHelper.GetAuthenticatedClientAsync(client, "admin@raceday.com", "Password123!");
        var goodEvent = new { OrganiserID = 1, CategoryID = 1, EventName = "Soweto 10K", EventDate = DateTime.UtcNow.AddDays(14) };

        var response = await organiserClient.PostAsJsonAsync("/api/events", goodEvent);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Participant_EntersEvent_Returns201Created()
    {
        var client = _factory.CreateClient();
        var participantClient = await TestAuthHelper.GetAuthenticatedClientAsync(client, "runner@raceday.com", "Password123!");
        var enrolment = new { ParticipantID = 1, EventID = 1, RegistrationDate = DateTime.UtcNow };

        var response = await participantClient.PostAsJsonAsync("/api/enrolments", enrolment);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }
}