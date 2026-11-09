
using System.Net.Http.Headers;
using System.Net.Http.Json;
using static RaceDay.Contracts.Dtos;

namespace RaceDay.Api.Tests;

public static class TestAuthHelper
{
    public const string Organiser = "Organiser";
    public const string Participant = "Participant";

    // Registers a new user.
    public static Task<HttpResponseMessage> RegisterAsync(
        HttpClient client,
        string email,
        string role,
        string password = TestData.Password)
    {
        return client.PostAsJsonAsync(
            ApiRoutes.Register,
            TestData.RegisterBody(email, role, password));
    }

    // Logs in an existing user.
    public static Task<HttpResponseMessage> LoginAsync(
        HttpClient client,
        string email,
        string password = TestData.Password)
    {
        return client.PostAsJsonAsync(
            ApiRoutes.Login,
            TestData.LoginBody(email, password));
    }

    // Registers and logs in a user, then returns an authenticated client.
    public static async Task<HttpClient> CreateAuthenticatedClientAsync(
        RaceDayWebApplicationFactory factory,
        string role)
    {
        var client = factory.CreateApiClient();
        var email = TestData.UniqueEmail(role.ToLowerInvariant());

        // Register the user.
        var register = await RegisterAsync(client, email, role);

        if (!register.IsSuccessStatusCode)
        {
            var error = await register.Content.ReadAsStringAsync();
            throw new Exception(
                $"Registration failed for {role}: {(int)register.StatusCode} - {error}");
        }

        // Log in the user.
        var login = await LoginAsync(client, email);

        if (!login.IsSuccessStatusCode)
        {
            var error = await login.Content.ReadAsStringAsync();
            throw new Exception(
                $"Login failed for {role}: {(int)login.StatusCode} - {error}");
        }

        // Read the JWT token returned by the API.
        var authResult = await login.Content.ReadFromJsonAsync<AuthResponse>();

        if (authResult == null || string.IsNullOrWhiteSpace(authResult.Token))
        {
            throw new Exception("Login succeeded but no authentication token was returned.");
        }

        // Add the token to future requests.
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", authResult.Token);

        return client;
    }

    // Creates an authenticated Organiser.
    public static Task<HttpClient> CreateOrganiserClientAsync(
        RaceDayWebApplicationFactory factory)
    {
        return CreateAuthenticatedClientAsync(factory, Organiser);
    }

    // Creates an authenticated Participant.
    public static Task<HttpClient> CreateParticipantClientAsync(
        RaceDayWebApplicationFactory factory)
    {
        return CreateAuthenticatedClientAsync(factory, Participant);
    }
}
