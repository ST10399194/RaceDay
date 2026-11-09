
namespace RaceDay.Api.Tests;

// Shared setup for all API test classes.
public abstract class BaseApiTest(
    RaceDayWebApplicationFactory factory)
    : IClassFixture<RaceDayWebApplicationFactory>
{
    // Provides access to the test API.
    protected readonly RaceDayWebApplicationFactory Factory = factory;

    // Creates an authenticated Organiser client.
    protected Task<HttpClient> GetOrganiserAsync()
    {
        return TestAuthHelper.CreateOrganiserClientAsync(Factory);
    }

    // Creates an authenticated Participant client.
    protected Task<HttpClient> GetParticipantAsync()
    {
        return TestAuthHelper.CreateParticipantClientAsync(Factory);
    }
}
