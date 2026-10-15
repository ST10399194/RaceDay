using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace RaceDay.Api.Tests;

public class ResultTests(RaceDayWebApplicationFactory factory) : BaseApiTest(factory)
{
    private async Task<(HttpClient Organiser, HttpClient Participant, int EventId, int EnrolmentId)> ArrangeAsync()
    {
        var organiser = await GetOrganiserAsync();
        var participant = await GetParticipantAsync();
        var eventId = await TestData.CreateEventAsync(organiser);
        var categoryId = await TestData.CreateCategoryAsync(organiser, eventId);
        var enrolmentId = await TestData.EnrolAsync(participant, eventId, categoryId);

        return (organiser, participant, eventId, enrolmentId);
    }

    private static async Task<HttpResponseMessage> CaptureResultAsync(HttpClient organiser, int enrolmentId, int position = 1) =>
        await organiser.PostAsJsonAsync(ApiRoutes.Results, TestData.ResultBody(enrolmentId, position));

    [Fact]
    public async Task Organiser_CaptureResult_ForOwnEvent_ReturnsSuccess()
    {
        var (organiser, _, _, enrolmentId) = await ArrangeAsync();
        var response = await CaptureResultAsync(organiser, enrolmentId);

        bool isSuccess = response.StatusCode == HttpStatusCode.Created || response.StatusCode == HttpStatusCode.OK;
        Assert.True(isSuccess, $"Expected 200 or 201, but returned {(int)response.StatusCode}");
    }

    [Fact]
    public async Task Organiser_CaptureResult_UnknownEnrolment_IsRejected()
    {
        var organiser = await GetOrganiserAsync();
        var response = await CaptureResultAsync(organiser, 999999);

        Assert.Contains(response.StatusCode, [HttpStatusCode.NotFound, HttpStatusCode.BadRequest]);
    }

    [Fact]
    public async Task Organiser_CaptureResultTwiceForSameEnrolment_IsRejected()
    {
        var (organiser, _, _, enrolmentId) = await ArrangeAsync();
        await CaptureResultAsync(organiser, enrolmentId);

        var second = await CaptureResultAsync(organiser, enrolmentId, position: 2);

        Assert.Contains(second.StatusCode, [HttpStatusCode.BadRequest, HttpStatusCode.Conflict]);
    }

    [Fact]
    public async Task Participant_ViewOwnResults_ReturnsOk()
    {
        var (organiser, participant, _, enrolmentId) = await ArrangeAsync();
        await CaptureResultAsync(organiser, enrolmentId);

        var response = await participant.GetAsync(ApiRoutes.MyResults);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Organiser_ViewResultsForOwnEvent_ReturnsOk()
    {
        var (organiser, _, eventId, enrolmentId) = await ArrangeAsync();
        await CaptureResultAsync(organiser, enrolmentId);

        var response = await organiser.GetAsync(ApiRoutes.EventResults(eventId));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Organiser_ViewResultsForAnotherOrganisersEvent_IsBlocked()
    {
        var (_, _, eventId, _) = await ArrangeAsync();
        var intruder = await GetOrganiserAsync();

        var response = await intruder.GetAsync(ApiRoutes.EventResults(eventId));

        Assert.Contains(response.StatusCode, [HttpStatusCode.Forbidden, HttpStatusCode.NotFound]);
    }
}