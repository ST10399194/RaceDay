
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace RaceDay.Api.Tests;

public static class TestData
{
    public const string Password = "Password123!";

    // Generates a different email for every test user.
    public static string UniqueEmail(string prefix) =>
        $"{prefix}_{Guid.NewGuid():N}@raceday.test";

    // Request bodies used when registering and logging in.
    public static object RegisterBody(string email, string role, string password = Password) => new
    {
        firstName = "Test",
        lastName = "User",
        email,
        password,
        role
    };

    public static object LoginBody(string email, string password = Password) =>
        new { email, password };

    // Request bodies for events, categories, enrolments and results.

    public static object EventBody(string name = "Soweto 10K") => new
    {
        name,
        description = "Community road race",
        date = DateTime.UtcNow.AddDays(30)
    };

    public static object CategoryBody(string name = "Senior") =>
        new { name };

    public static object EnrolmentBody(int eventId, int categoryId) =>
        new { eventId, categoryId };

    public static object ResultBody(int enrolmentId, int position = 1) => new
    {
        enrolmentId,
        finishTime = "00:48:30",
        position
    };

    // Creates an event and returns its database ID.
    public static Task<int> CreateEventAsync(
        HttpClient organiser,
        string name = "Soweto 10K") =>
        PostAndGetIdAsync(
            organiser,
            ApiRoutes.Events,
            EventBody(name),
            "eventId", "eventID", "id");

    // Creates a category for an event and returns its ID.
    public static Task<int> CreateCategoryAsync(
        HttpClient organiser,
        int eventId,
        string name = "Senior") =>
        PostAndGetIdAsync(
            organiser,
            ApiRoutes.EventCategories(eventId),
            CategoryBody(name),
            "categoryId", "categoryID", "id");

    // Enrols a participant and returns the enrolment ID.
    public static Task<int> EnrolAsync(
        HttpClient participant,
        int eventId,
        int categoryId) =>
        PostAndGetIdAsync(
            participant,
            ApiRoutes.Enrolments,
            EnrolmentBody(eventId, categoryId),
            "enrolmentId", "enrolmentID",
            "enrollmentId", "enrollmentID", "id");

    // Sends a POST request and extracts the new record's ID.
    private static async Task<int> PostAndGetIdAsync(
        HttpClient client,
        string route,
        object body,
        params string[] idNames)
    {
        var response = await client.PostAsJsonAsync(route, body);

        var responseBody = await response.Content.ReadAsStringAsync();

        Assert.True(
            response.StatusCode == HttpStatusCode.OK ||
            response.StatusCode == HttpStatusCode.Created,
            $"Setup failed for {route}. " +
            $"Status: {(int)response.StatusCode} ({response.StatusCode}). " +
            $"Response: {responseBody}");

        return await ReadIdAsync(response, idNames);
    }

    // Finds the ID in the API response.
    public static async Task<int> ReadIdAsync(
        HttpResponseMessage response,
        params string[] candidateNames)
    {
        var json = await response.Content.ReadAsStringAsync();

        if (string.IsNullOrWhiteSpace(json))
        {
            throw new InvalidOperationException(
                "The API returned an empty response. No ID could be read.");
        }

        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        if (root.ValueKind != JsonValueKind.Object)
        {
            throw new InvalidOperationException(
                $"Expected a JSON object but received: {json}");
        }

        // Check the expected ID property names first.
        foreach (var name in candidateNames)
        {
            foreach (var property in root.EnumerateObject())
            {
                if (string.Equals(
                        property.Name,
                        name,
                        StringComparison.OrdinalIgnoreCase)
                    && property.Value.ValueKind == JsonValueKind.Number
                    && property.Value.TryGetInt32(out int id))
                {
                    return id;
                }
            }
        }

        // If the property has a different name, look for any numeric ID.
        foreach (var property in root.EnumerateObject())
        {
            if (property.Name.EndsWith(
                    "id",
                    StringComparison.OrdinalIgnoreCase)
                && property.Value.ValueKind == JsonValueKind.Number
                && property.Value.TryGetInt32(out int id))
            {
                return id;
            }
        }

        throw new InvalidOperationException(
            $"Could not find an ID in the API response: {json}");
    }
}
