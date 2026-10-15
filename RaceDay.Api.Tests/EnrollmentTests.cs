using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace RaceDay.Api.Tests;

public class EnrollmentTests(RaceDayWebApplicationFactory factory) : BaseApiTest(factory)
{
    private static async Task<HttpResponseMessage> EnrolParticipantAsync(HttpClient participant, int eventId, int categoryId) =>
        await participant.PostAsJsonAsync(ApiRoutes.Enrolments, TestData.EnrolmentBody(eventId, categoryId));

    [Fact]
    public async Task Participant_Enrol_ReturnsSuccess()
    {
        var organiser = await GetOrganiserAsync();
        var eventId = await TestData.CreateEventAsync(organiser);
        var categoryId = await TestData.CreateCategoryAsync(organiser, eventId);
        var participant = await GetParticipantAsync();

        var response = await EnrolParticipantAsync(participant, eventId, categoryId);

        bool isSuccess = response.StatusCode == HttpStatusCode.Created || response.StatusCode == HttpStatusCode.OK;
        Assert.True(isSuccess, $"Expected 200 or 201, but returned {(int)response.StatusCode}");
    }

    [Fact]
    public async Task Participant_Enrol_UnknownEvent_IsRejected()
    {
        var participant = await GetParticipantAsync();
        var response = await EnrolParticipantAsync(participant, 999999, 1);

        Assert.Contains(response.StatusCode, [HttpStatusCode.BadRequest, HttpStatusCode.Conflict, HttpStatusCode.UnprocessableEntity, HttpStatusCode.NotFound]);
    }

    [Fact]
    public async Task Participant_Enrol_CategoryBelongingToAnotherEvent_IsRejected()
    {
        var organiser = await GetOrganiserAsync();
        var eventA = await TestData.CreateEventAsync(organiser, "Event A");
        var eventB = await TestData.CreateEventAsync(organiser, "Event B");
        var categoryOfB = await TestData.CreateCategoryAsync(organiser, eventB);
        var participant = await GetParticipantAsync();

        var response = await EnrolParticipantAsync(participant, eventA, categoryOfB);

        Assert.Contains(response.StatusCode, [HttpStatusCode.BadRequest, HttpStatusCode.Conflict, HttpStatusCode.UnprocessableEntity, HttpStatusCode.NotFound]);
    }

    [Fact]
    public async Task Participant_EnrolTwiceInSameEvent_IsRejected()
    {
        var organiser = await GetOrganiserAsync();
        var eventId = await TestData.CreateEventAsync(organiser);
        var categoryId = await TestData.CreateCategoryAsync(organiser, eventId);
        var participant = await GetParticipantAsync();
        await TestData.EnrolAsync(participant, eventId, categoryId);

        var second = await EnrolParticipantAsync(participant, eventId, categoryId);

        Assert.Contains(second.StatusCode, [HttpStatusCode.BadRequest, HttpStatusCode.Conflict]);
    }

    [Fact]
    public async Task Participant_ViewOwnEnrolments_ReturnsOk()
    {
        var organiser = await GetOrganiserAsync();
        var eventId = await TestData.CreateEventAsync(organiser);
        var categoryId = await TestData.CreateCategoryAsync(organiser, eventId);
        var participant = await GetParticipantAsync();
        await TestData.EnrolAsync(participant, eventId, categoryId);

        var response = await participant.GetAsync(ApiRoutes.MyEnrolments);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Organiser_ViewEnrolmentsForOwnEvent_ReturnsOk()
    {
        var organiser = await GetOrganiserAsync();
        var eventId = await TestData.CreateEventAsync(organiser);
        var categoryId = await TestData.CreateCategoryAsync(organiser, eventId);
        var participant = await GetParticipantAsync();
        await TestData.EnrolAsync(participant, eventId, categoryId);

        var response = await organiser.GetAsync(ApiRoutes.EventEnrolments(eventId));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Organiser_UpdateEnrolmentStatus_ForOwnEvent_Succeeds()
    {
        var organiser = await GetOrganiserAsync();
        var eventId = await TestData.CreateEventAsync(organiser);
        var categoryId = await TestData.CreateCategoryAsync(organiser, eventId);
        var participant = await GetParticipantAsync();
        var enrolmentId = await TestData.EnrolAsync(participant, eventId, categoryId);

        var response = await organiser.PutAsJsonAsync(
            ApiRoutes.EnrolmentStatus(enrolmentId), new { status = "Confirmed" });

        Assert.True(response.StatusCode is HttpStatusCode.OK or HttpStatusCode.NoContent,
            $"Expected 200/204 but got {(int)response.StatusCode}");
    }

    [Fact]
    public async Task Participant_UpdateEnrolmentStatus_Returns403()
    {
        var organiser = await GetOrganiserAsync();
        var eventId = await TestData.CreateEventAsync(organiser);
        var categoryId = await TestData.CreateCategoryAsync(organiser, eventId);
        var participant = await GetParticipantAsync();
        var enrolmentId = await TestData.EnrolAsync(participant, eventId, categoryId);

        var response = await participant.PutAsJsonAsync(
            ApiRoutes.EnrolmentStatus(enrolmentId), new { status = "Confirmed" });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}