using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace RaceDay.Api.Tests;

/// <summary>
/// EVERY route used by the tests lives here. If one of your controllers uses a
/// different URL (for example "enrollments" instead of "enrolments"), change it
/// once in this class and all tests follow.
/// </summary>
public static class ApiRoutes
{
    public const string Register = "/api/auth/register";
    public const string Login = "/api/auth/login";
    public const string Logout = "/api/auth/logout";

    public const string Profile = "/api/profile/me";

    public const string Events = "/api/events";
    public const string MyEvents = "/api/events/mine";
    public static string Event(int id) => $"/api/events/{id}";

    public static string EventCategories(int eventId) => $"/api/events/{eventId}/categories";
    public static string Category(int id) => $"/api/categories/{id}";

    public const string Enrolments = "/api/enrolments";
    public const string MyEnrolments = "/api/enrolments/mine";
    public static string EventEnrolments(int eventId) => $"/api/enrolments/event/{eventId}";
    public static string EnrolmentStatus(int id) => $"/api/enrolments/{id}/status";

    public const string Results = "/api/results";
    public const string MyResults = "/api/results/mine";
    public static string EventResults(int eventId) => $"/api/results/event/{eventId}";
}

/// <summary>
/// Request bodies and set-up helpers. Property names are camelCase because that is
/// how ASP.NET Core serialises/deserialises JSON by default. If your DTOs in
/// RaceDay.Contracts use different property names, adjust the bodies below.
/// </summary>
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

    // ---- set-up helpers (each asserts, so a broken set-up fails loudly and clearly) ----

    public static async Task<int> CreateEventAsync(HttpClient organiser, string name = "Soweto 10K")
    {
        var response = await organiser.PostAsJsonAsync(ApiRoutes.Events, EventBody(name));
        Assert.True(response.StatusCode == System.Net.HttpStatusCode.Created,
            $"Set-up failed: creating an event returned {(int)response.StatusCode}");
        return await ReadIdAsync(response, "eventId", "eventID", "id");
    }

    public static async Task<int> CreateCategoryAsync(HttpClient organiser, int eventId, string name = "Senior")
    {
        var response = await organiser.PostAsJsonAsync(ApiRoutes.EventCategories(eventId), CategoryBody(name));
        Assert.True(response.StatusCode == System.Net.HttpStatusCode.Created,
            $"Set-up failed: creating a category returned {(int)response.StatusCode}");
        return await ReadIdAsync(response, "categoryId", "categoryID", "id");
    }

    public static async Task<int> EnrolAsync(HttpClient participant, int eventId, int categoryId)
    {
        var response = await participant.PostAsJsonAsync(ApiRoutes.Enrolments, EnrolmentBody(eventId, categoryId));
        Assert.True(response.StatusCode == System.Net.HttpStatusCode.Created,
            $"Set-up failed: enrolling returned {(int)response.StatusCode}");
        return await ReadIdAsync(response, "enrolmentId", "enrolmentID", "enrollmentId", "enrollmentID", "id");
    }

    /// <summary>Reads a numeric id from a JSON response, trying the given names first.</summary>
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

        // Fallback: first numeric property whose name ends in "id".
        foreach (var prop in root.EnumerateObject())
            if (prop.Name.EndsWith("id", StringComparison.OrdinalIgnoreCase)
                && prop.Value.ValueKind == JsonValueKind.Number)
                return prop.Value.GetInt32();

        throw new InvalidOperationException($"Could not find an id in the response body: {json}");
    }
}