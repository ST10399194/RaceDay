using System.Net.Http.Json;

namespace RaceDay.Api.Tests;

public static class TestAuthHelper
{
    public static async Task<HttpClient> GetAuthenticatedClientAsync(HttpClient baseClient, string email, string password)
    {
        // 1. Post  existing login endpoint
        var loginPayload = new { Email = email, Password = password };
        var response = await baseClient.PostAsJsonAsync("/api/auth/login", loginPayload);

        // Ensure credentials match database seeder records
        response.EnsureSuccessStatusCode();

        // Returns the client instance maintaining the server session data
        return baseClient;
    }
}