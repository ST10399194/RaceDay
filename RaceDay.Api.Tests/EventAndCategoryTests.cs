using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace RaceDay.Api.Tests;

public class EventAndCategoryTests(RaceDayWebApplicationFactory factory) : BaseApiTest(factory)
{
    // - Events --

    [Fact]
    public async Task Organiser_CreateEvent_Returns201()
    {
        var organiser = await GetOrganiserAsync();
        var response = await organiser.PostAsJsonAsync(ApiRoutes.Events, TestData.EventBody());
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Organiser_CreateEvent_MissingName_Returns400()
    {
        var organiser = await GetOrganiserAsync();
        var response = await organiser.PostAsJsonAsync(ApiRoutes.Events, TestData.EventBody(name: ""));
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData("Organiser")]
    [InlineData("Participant")]
    public async Task BothRoles_CanViewEventList(string role)
    {
        var organiser = await GetOrganiserAsync();
        await TestData.CreateEventAsync(organiser);

        var client = await TestAuthHelper.CreateAuthenticatedClientAsync(Factory, role);
        var response = await client.GetAsync(ApiRoutes.Events);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Participant_CanViewSingleEvent()
    {
        var organiser = await GetOrganiserAsync();
        var eventId = await TestData.CreateEventAsync(organiser);

        var participant = await GetParticipantAsync();
        var response = await participant.GetAsync(ApiRoutes.Event(eventId));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetEvent_UnknownId_Returns404()
    {
        var participant = await GetParticipantAsync();
        var response = await participant.GetAsync(ApiRoutes.Event(999999));
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Organiser_MyEvents_ReturnsOk()
    {
        var organiser = await GetOrganiserAsync();
        await TestData.CreateEventAsync(organiser);

        var response = await organiser.GetAsync(ApiRoutes.MyEvents);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Organiser_UpdateOwnEvent_Succeeds()
    {
        var organiser = await GetOrganiserAsync();
        var eventId = await TestData.CreateEventAsync(organiser);

        var response = await organiser.PutAsJsonAsync(ApiRoutes.Event(eventId), TestData.EventBody("Renamed 10K"));
        Assert.True(response.StatusCode is HttpStatusCode.OK or HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Organiser_DeleteOwnEvent_ThenEventIsGone()
    {
        var organiser = await GetOrganiserAsync();
        var eventId = await TestData.CreateEventAsync(organiser);

        var delete = await organiser.DeleteAsync(ApiRoutes.Event(eventId));
        Assert.True(delete.StatusCode is HttpStatusCode.OK or HttpStatusCode.NoContent);

        var get = await organiser.GetAsync(ApiRoutes.Event(eventId));
        Assert.Equal(HttpStatusCode.NotFound, get.StatusCode);
    }

    // - Categories --

    [Fact]
    public async Task Organiser_CreateCategoryOnOwnEvent_Returns201()
    {
        var organiser = await GetOrganiserAsync();
        var eventId = await TestData.CreateEventAsync(organiser);

        var response = await organiser.PostAsJsonAsync(ApiRoutes.EventCategories(eventId), TestData.CategoryBody("Under 20"));
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Participant_UpdateCategory_Returns403()
    {
        var organiser = await GetOrganiserAsync();
        var eventId = await TestData.CreateEventAsync(organiser);
        var categoryId = await TestData.CreateCategoryAsync(organiser, eventId);

        var participant = await GetParticipantAsync();
        var response = await participant.PutAsJsonAsync(ApiRoutes.Category(categoryId), TestData.CategoryBody("Nope"));
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}