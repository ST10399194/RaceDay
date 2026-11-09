
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace RaceDay.Api.Tests;

public class EnrollmentTests(RaceDayWebApplicationFactory factory)
    : BaseApiTest(factory)
{
    // PARTICIPANT ENROLMENT TESTS

    [Fact]
    public async Task Participant_Enrol_ReturnsOk()
    {
        var organiser = await GetOrganiserAsync();
        var eventId = await TestData.CreateEventAsync(organiser);
        var categoryId = await TestData.CreateCategoryAsync(organiser, eventId);
        var participant = await GetParticipantAsync();

        var response = await participant.PostAsJsonAsync(
            ApiRoutes.Enrolments,
            TestData.EnrolmentBody(eventId, categoryId));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Participant_Enrol_UnknownEvent_ReturnsNotFound()
    {
        var organiser = await GetOrganiserAsync();
        var eventId = await TestData.CreateEventAsync(organiser);
        var categoryId = await TestData.CreateCategoryAsync(organiser, eventId);
        var participant = await GetParticipantAsync();

        // Use a valid category but an event ID that does not exist.
        var response = await participant.PostAsJsonAsync(
            ApiRoutes.Enrolments,
            TestData.EnrolmentBody(999999, categoryId));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // VIEW ENROLMENTS TESTS

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

        var response = await organiser.GetAsync(
            ApiRoutes.EventEnrolments(eventId));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // UPDATE ENROLMENT TESTS

    [Fact]
    public async Task Organiser_UpdateEnrolmentStatus_ReturnsOk()
    {
        var organiser = await GetOrganiserAsync();
        var eventId = await TestData.CreateEventAsync(organiser);
        var categoryId = await TestData.CreateCategoryAsync(organiser, eventId);
        var participant = await GetParticipantAsync();

        var enrolmentId = await TestData.EnrolAsync(
            participant, eventId, categoryId);

        var response = await organiser.PutAsJsonAsync(
            ApiRoutes.EnrolmentStatus(enrolmentId),
            new { status = "Confirmed" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Participant_UpdateEnrolmentStatus_ReturnsForbidden()
    {
        var organiser = await GetOrganiserAsync();
        var eventId = await TestData.CreateEventAsync(organiser);
        var categoryId = await TestData.CreateCategoryAsync(organiser, eventId);
        var participant = await GetParticipantAsync();

        var enrolmentId = await TestData.EnrolAsync(
            participant, eventId, categoryId);

        var response = await participant.PutAsJsonAsync(
            ApiRoutes.EnrolmentStatus(enrolmentId),
            new { status = "Confirmed" });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
