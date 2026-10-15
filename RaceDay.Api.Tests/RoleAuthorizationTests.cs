using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace RaceDay.Api.Tests;

/// <summary>
/// Proves that access is blocked when it should be:
///   401 = not logged in, 403 = logged in with the wrong role,
///   403/404 = right role but not the owner of the resource.
/// </summary>
public class RoleAuthorizationTests(RaceDayWebApplicationFactory factory) : BaseApiTest(factory)
{
    private static readonly HttpStatusCode[] ForbiddenOrNotFound =
        [HttpStatusCode.Forbidden, HttpStatusCode.NotFound];

    // ------------------------- 401: not logged in -------------------------

    [Theory]
    [InlineData("/api/profile/me")]
    [InlineData("/api/events")]
    [InlineData("/api/events/mine")]
    [InlineData("/api/enrolments/mine")]
    [InlineData("/api/results/mine")]
    public async Task ProtectedGet_WithoutLogin_Returns401(string url)
    {
        var client = Factory.CreateApiClient();

        var response = await client.GetAsync(url);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateEvent_WithoutLogin_Returns401()
    {
        var client = Factory.CreateApiClient();

        var response = await client.PostAsJsonAsync(ApiRoutes.Events, TestData.EventBody());

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Enrol_WithoutLogin_Returns401()
    {
        var client = Factory.CreateApiClient();

        var response = await client.PostAsJsonAsync(ApiRoutes.Enrolments, TestData.EnrolmentBody(1, 1));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // ------------------- 403: Participant on Organiser-only -------------------

    [Fact]
    public async Task Participant_CreateEvent_Returns403()
    {
        var participant = await GetParticipantAsync();

        var response = await participant.PostAsJsonAsync(ApiRoutes.Events, TestData.EventBody());

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Participant_UpdateEvent_Returns403()
    {
        var organiser = await GetOrganiserAsync();
        var eventId = await TestData.CreateEventAsync(organiser);
        var participant = await GetParticipantAsync();

        var response = await participant.PutAsJsonAsync(ApiRoutes.Event(eventId), TestData.EventBody("Nope"));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Participant_DeleteEvent_Returns403()
    {
        var organiser = await GetOrganiserAsync();
        var eventId = await TestData.CreateEventAsync(organiser);
        var participant = await GetParticipantAsync();

        var response = await participant.DeleteAsync(ApiRoutes.Event(eventId));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Participant_CreateCategory_Returns403()
    {
        var organiser = await GetOrganiserAsync();
        var eventId = await TestData.CreateEventAsync(organiser);
        var participant = await GetParticipantAsync();

        var response = await participant.PostAsJsonAsync(
            ApiRoutes.EventCategories(eventId), TestData.CategoryBody("Sneaky"));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Participant_ViewEventEnrolmentList_Returns403()
    {
        var organiser = await GetOrganiserAsync();
        var eventId = await TestData.CreateEventAsync(organiser);
        var participant = await GetParticipantAsync();

        var response = await participant.GetAsync(ApiRoutes.EventEnrolments(eventId));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Participant_CaptureResult_Returns403()
    {
        var organiser = await GetOrganiserAsync();
        var eventId = await TestData.CreateEventAsync(organiser);
        var categoryId = await TestData.CreateCategoryAsync(organiser, eventId);
        var participant = await GetParticipantAsync();
        var enrolmentId = await TestData.EnrolAsync(participant, eventId, categoryId);

        var response = await participant.PostAsJsonAsync(ApiRoutes.Results, TestData.ResultBody(enrolmentId));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    // ------------------- 403: Organiser on Participant-only -------------------

    [Fact]
    public async Task Organiser_Enrol_Returns403()
    {
        var organiser = await GetOrganiserAsync();
        var eventId = await TestData.CreateEventAsync(organiser);
        var categoryId = await TestData.CreateCategoryAsync(organiser, eventId);

        var response = await organiser.PostAsJsonAsync(
            ApiRoutes.Enrolments, TestData.EnrolmentBody(eventId, categoryId));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    // ---------------- Ownership: right role, wrong Organiser ----------------

    [Fact]
    public async Task Organiser_UpdateAnotherOrganisersEvent_IsBlocked()
    {
        var owner = await GetOrganiserAsync();
        var eventId = await TestData.CreateEventAsync(owner);
        var intruder = await GetOrganiserAsync();

        var response = await intruder.PutAsJsonAsync(ApiRoutes.Event(eventId), TestData.EventBody("Hijacked"));

        Assert.Contains(response.StatusCode, ForbiddenOrNotFound);
    }

    [Fact]
    public async Task Organiser_DeleteAnotherOrganisersEvent_IsBlocked()
    {
        var owner = await GetOrganiserAsync();
        var eventId = await TestData.CreateEventAsync(owner);
        var intruder = await GetOrganiserAsync();

        var response = await intruder.DeleteAsync(ApiRoutes.Event(eventId));

        Assert.Contains(response.StatusCode, ForbiddenOrNotFound);
    }

    [Fact]
    public async Task Organiser_CreateCategoryOnAnotherOrganisersEvent_IsBlocked()
    {
        var owner = await GetOrganiserAsync();
        var eventId = await TestData.CreateEventAsync(owner);
        var intruder = await GetOrganiserAsync();

        var response = await intruder.PostAsJsonAsync(
            ApiRoutes.EventCategories(eventId), TestData.CategoryBody("Sneaky"));

        Assert.Contains(response.StatusCode, ForbiddenOrNotFound);
    }

    [Fact]
    public async Task Organiser_ViewEnrolmentsOfAnotherOrganisersEvent_IsBlocked()
    {
        var owner = await GetOrganiserAsync();
        var eventId = await TestData.CreateEventAsync(owner);
        var intruder = await GetOrganiserAsync();

        var response = await intruder.GetAsync(ApiRoutes.EventEnrolments(eventId));

        Assert.Contains(response.StatusCode, ForbiddenOrNotFound);
    }

    [Fact]
    public async Task Organiser_CaptureResultForAnotherOrganisersEvent_IsBlocked()
    {
        var owner = await GetOrganiserAsync();
        var eventId = await TestData.CreateEventAsync(owner);
        var categoryId = await TestData.CreateCategoryAsync(owner, eventId);

        var participant = await GetParticipantAsync();
        var enrolmentId = await TestData.EnrolAsync(participant, eventId, categoryId);
        var intruder = await GetOrganiserAsync();

        var response = await intruder.PostAsJsonAsync(ApiRoutes.Results, TestData.ResultBody(enrolmentId));

        Assert.Contains(response.StatusCode, ForbiddenOrNotFound);
    }
}