namespace RaceDay.Api.Tests;

public abstract class BaseApiTest(RaceDayWebApplicationFactory factory) : IClassFixture<RaceDayWebApplicationFactory>
{
    protected readonly RaceDayWebApplicationFactory Factory = factory;

    protected Task<HttpClient> GetOrganiserAsync() =>
        TestAuthHelper.CreateOrganiserClientAsync(Factory);

    protected Task<HttpClient> GetParticipantAsync() =>
        TestAuthHelper.CreateParticipantClientAsync(Factory);
}