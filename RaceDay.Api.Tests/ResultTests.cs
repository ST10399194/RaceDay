
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace RaceDay.Api.Tests;

public class ResultTests(RaceDayWebApplicationFactory factory)
    : BaseApiTest(factory)
{
    // Creates an organiser, participant, event, category and enrolment.
    private async Task<(HttpClient Organiser, HttpClient Participant,
        int EventId, int EnrolmentId)> ArrangeAsync()
    {
        var organiser = await GetOrganiserAsync();
        var participant = await GetParticipantAsync();

        var eventId = await TestData.CreateEventAsync(organiser);
        var categoryId = await TestData.CreateCategoryAsync(organiser, eventId);

        var enrolmentId = await TestData.EnrolAsync(
            participant, eventId, categoryId);

        return (organiser, participant, eventId, enrolmentId);
    }

    // Sends a request to capture a participant's result.
    private static Task<HttpResponseMessage> CaptureResultAsync(
        HttpClient organiser, int enrolmentId, int position = 1)
    {
        return organiser.PostAsJsonAsync(
            ApiRoutes.Results,
            TestData.ResultBody(enrolmentId, position));
    }

    // CAPTURE RESULT TESTS
    [Fact]
    public async Task Organiser_CaptureResult_ReturnsSuccess()
    {
        // Arrange
        var (organiser, _, _, enrolmentId) = await ArrangeAsync();

        // Act
        var response = await CaptureResultAsync(organiser, enrolmentId);

        // Read the API response for debugging.
        var responseBody = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.True(
            response.StatusCode == HttpStatusCode.OK ||
            response.StatusCode == HttpStatusCode.Created,
            $"Expected 200 or 201 but received {(int)response.StatusCode} " +
            $"({response.StatusCode}). Response: {responseBody}");
    }
    [Fact]
    public async Task Organiser_CaptureResult_UnknownEnrolment_ReturnsNotFound()
    {
        var organiser = await GetOrganiserAsync();

        var response = await CaptureResultAsync(organiser, 999999);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Organiser_CaptureResultTwice_IsRejected()
    {
        var (organiser, _, _, enrolmentId) = await ArrangeAsync();

        await CaptureResultAsync(organiser, enrolmentId);

        var response = await CaptureResultAsync(
            organiser, enrolmentId, position: 2);

        Assert.True(
            response.StatusCode == HttpStatusCode.BadRequest ||
            response.StatusCode == HttpStatusCode.Conflict,
            $"Expected 400 or 409 but received {(int)response.StatusCode}");
    }

    // VIEW RESULT TESTS

    [Fact]
    public async Task Participant_ViewOwnResults_ReturnsOk()
    {
        var (organiser, participant, _, enrolmentId) = await ArrangeAsync();

        await CaptureResultAsync(organiser, enrolmentId);

        var response = await participant.GetAsync(ApiRoutes.MyResults);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Organiser_ViewOwnEventResults_ReturnsOk()
    {
        var (organiser, _, eventId, enrolmentId) = await ArrangeAsync();

        await CaptureResultAsync(organiser, enrolmentId);

        var response = await organiser.GetAsync(
            ApiRoutes.EventResults(eventId));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]


    public async Task Organiser_ViewAnotherOrganisersResults_IsForbidden()
    {
        // Arrange: first Organiser creates an event.
        var (organiser, _, eventId, _) = await ArrangeAsync();

        // A second Organiser logs in using a different account.
        var otherOrganiser = await GetOrganiserAsync();

        // Act: second Organiser tries to view the first Organiser's results.
        var response = await otherOrganiser.GetAsync(
            ApiRoutes.EventResults(eventId));

        // Read the response to help identify the problem.
        var responseBody = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.True(
            response.StatusCode == HttpStatusCode.Forbidden,
            $"Expected 403 Forbidden, but received {(int)response.StatusCode} " +
            $"({response.StatusCode}). Response: {responseBody}");
    }

}



