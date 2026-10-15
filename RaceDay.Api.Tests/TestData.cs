using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace RaceDay.Api.Tests;

public static class TestData
{
    public const string Password = "Password123!";

    public static string UniqueEmail(string prefix) => $"{prefix}_{Guid.NewGuid():N}@raceday.test";

    public static object RegisterBody(string email, string role, string password = Password) => new
    {
        firstName = "Test",
        lastName = "User",
        email,
        password,
        role
    };

    public static object LoginBody(string email, string password = Password) => new { email, password };

    public static object EventBody(string name = "Soweto 10K") => new
    {
        name,
        description = "Community road race",
        eventDate = DateTime.UtcNow.AddDays(30),
        location = "Soweto Stadium",
        distanceKm = 10.0,
        eventType = "Road"
    };

    public static object CategoryBody(string name = "Senior") => new { name };

    public static object EnrolmentBody(int eventId, int categoryId) => new { eventId, categoryId };

    public static object ResultBody(int enrolmentId, int position = 1) => new
    {
        enrolmentId,
        finishTime = "00:48:30",
        position
    };

    // ---- Setup Helpers ----

    public static async Task<int> CreateEventAsync(HttpClient organiser, string name = "Soweto 10K") =>
        await PostAndGetIdAsync(organiser, ApiRoutes.Events, EventBody(name), "eventId", "eventID", "id");

    public static async Task<int> CreateCategoryAsync(HttpClient organiser, int eventId, string name = "Senior") =>
        await PostAndGetIdAsync(organiser, ApiRoutes.EventCategories(eventId), CategoryBody(name), "categoryId", "categoryID", "id");

    public static async Task<int> EnrolAsync(HttpClient participant, int eventId, int categoryId) =>
        await PostAndGetIdAsync(participant, ApiRoutes.Enrolments, EnrolmentBody(eventId, categoryId), "enrolmentId", "enrolmentID", "enrollmentId", "enrollmentID", "id");

    private static async Task<int> PostAndGetIdAsync(HttpClient client, string route, object body, params string[] idNames)
    {
        var response = await client.PostAsJsonAsync(route, body);

        var code = response.StatusCode;
        bool isSuccess = code == HttpStatusCode.Created || code == HttpStatusCode.OK;
        Assert.True(isSuccess, $"Set-up failed for {route}: expected 200 or 201, but returned {(int)code}");

        return await ReadIdAsync(response, idNames);
    }

    public static async Task<int> ReadIdAsync(HttpResponseMessage response, params string[] candidateNames)
    {
        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        foreach (var name in candidateNames)
            foreach (var prop in root.EnumerateObject())
                if (string.Equals(prop.Name, name, StringComparison.OrdinalIgnoreCase)
                    && prop.Value.ValueKind == JsonValueKind.Number)
                    return prop.Value.GetInt32();

        foreach (var prop in root.EnumerateObject())
            if (prop.Name.EndsWith("id", StringComparison.OrdinalIgnoreCase)
                && prop.Value.ValueKind == JsonValueKind.Number)
                return prop.Value.GetInt32();

        throw new InvalidOperationException($"Could not find an id in the response body: {json}");
    }
}