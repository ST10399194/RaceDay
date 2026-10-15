using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace RaceDay.Api.Tests;

public class ResultTests(RaceDayWebApplicationFactory factory) : IClassFixture<RaceDayWebApplicationFactory>
{
    private async Task<(HttpClient Organiser, HttpClient Participant, int EventId, int EnrolmentId)> ArrangeAsync()
    {
        // 1. Initialize authenticated role clients
        var organiser = await TestAuthHelper.CreateOrganiserClientAsync(factory);
        var participant = await TestAuthHelper.CreateParticipantClientAsync(factory);
        
        // 2. Build out the dependencies sequentially
        var eventId = await TestData.CreateEventAsync(organiser);
        var categoryId = await TestData.CreateCategoryAsync(organiser, eventId);
        
        // 3. Complete enrollment using the pre-authenticated participant client context
        var enrolmentId = await TestData.EnrolAsync(participant, eventId, categoryId);
        
        return (organiser, participant, eventId, enrolmentId);
    }

    [Fact]
    public async Task Organiser_CaptureResult_ForOwnEvent_Returns201()
    {
        var (organiser, _, _, enrolmentId) = await ArrangeAsync();

        var response = await organiser.PostAsJsonAsync(ApiRoutes.Results, TestData.ResultBody(enrolmentId));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Organiser_CaptureResult_UnknownEnrolment_IsRejected()
    {
        var organiser = await TestAuthHelper.CreateOrganiserClientAsync(factory);

        var response = await organiser.PostAsJsonAsync(ApiRoutes.Results, TestData.ResultBody(999999));

        Assert.Contains(response.StatusCode, [HttpStatusCode.NotFound, HttpStatusCode.BadRequest]);
    }

    [Fact]
    public async Task Organiser_CaptureResultTwiceForSameEnrolment_IsRejected()
    {
        var (organiser, _, _, enrolmentId) = await ArrangeAsync();
        await organiser.PostAsJsonAsync(ApiRoutes.Results, TestData.ResultBody(enrolmentId));

        var second = await organiser.PostAsJsonAsync(ApiRoutes.Results, TestData.ResultBody(enrolmentId, position: 2));

        Assert.Contains(second.StatusCode, [HttpStatusCode.BadRequest, HttpStatusCode.Conflict]);
    }

    [Fact]
    public async Task Participant_ViewOwnResults_ReturnsOk()
    {
        var (organiser, participant, _, enrolmentId) = await ArrangeAsync();
        await organiser.PostAsJsonAsync(ApiRoutes.Results, TestData.ResultBody(enrolmentId));

        var response = await participant.GetAsync(ApiRoutes.MyResults);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Organiser_ViewResultsForOwnEvent_ReturnsOk()
    {
        var (organiser, _, eventId, enrolmentId) = await ArrangeAsync();
        await organiser.PostAsJsonAsync(ApiRoutes.Results, TestData.ResultBody(enrolmentId));

        var response = await organiser.GetAsync(ApiRoutes.EventResults(eventId));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Organiser_ViewResultsForAnotherOrganisersEvent_IsBlocked()
    {
        var (_, _, eventId, _) = await ArrangeAsync();
        var intruder = await TestAuthHelper.CreateOrganiserClientAsync(factory);

        var response = await intruder.GetAsync(ApiRoutes.EventResults(eventId));

        Assert.Contains(response.StatusCode, [HttpStatusCode.Forbidden, HttpStatusCode.NotFound]);
    }
}