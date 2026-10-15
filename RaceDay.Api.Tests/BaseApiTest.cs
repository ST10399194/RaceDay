using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace RaceDay.Api.Tests;

public abstract class BaseApiTest(RaceDayWebApplicationFactory factory) : IClassFixture<RaceDayWebApplicationFactory>
{
    protected readonly RaceDayWebApplicationFactory Factory = factory;

    /// <summary>
    /// Generates a pristine, authorized client for an Organizer.
    /// </summary>
    protected Task<HttpClient> GetOrganiserAsync() =>
    TestAuthHelper.CreateOrganiserClientAsync(Factory);

    /// <summary>
    /// Generates a pristine, authorized client for a Participant.
    /// </summary>
    protected Task<HttpClient> GetParticipantAsync() =>
    TestAuthHelper.CreateParticipantClientAsync(Factory);
}