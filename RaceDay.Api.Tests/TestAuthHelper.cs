using System.Net.Http.Json;
using Xunit;
using static RaceDay.Contracts.Dtos;

namespace RaceDay.Api.Tests;

public static class TestAuthHelper
{
    public const string Organiser = "Organiser";
    public const string Participant = "Participant";

    public static Task<HttpResponseMessage> RegisterAsync(
        HttpClient client, string email, string role, string password = TestData.Password) =>
        client.PostAsJsonAsync(ApiRoutes.Register, TestData.RegisterBody(email, role, password));

    public static Task<HttpResponseMessage> LoginAsync(
        HttpClient client, string email, string password = TestData.Password) =>
        client.PostAsJsonAsync(ApiRoutes.Login, TestData.LoginBody(email, password));

    public static async Task<HttpClient> CreateAuthenticatedClientAsync(
        RaceDayWebApplicationFactory factory, string role)
    {
        var client = factory.CreateApiClient();
        var email = TestData.UniqueEmail(role.ToLowerInvariant());

        var register = await RegisterAsync(client, email, role);
        Assert.True(register.IsSuccessStatusCode,
            $"Set-up failed: registering a {role} returned {(int)register.StatusCode}");

        var login = await LoginAsync(client, email);
        Assert.True(login.IsSuccessStatusCode,
            $"Set-up failed: logging in a {role} returned {(int)login.StatusCode}");

        var authResult = await login.Content.ReadFromJsonAsync<AuthResponse>();

        Assert.NotNull(authResult);
        Assert.False(string.IsNullOrWhiteSpace(authResult.Token), "Login response was successful but contained an empty token.");

        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authResult.Token);

        return client;
    }

    public static Task<HttpClient> CreateOrganiserClientAsync(RaceDayWebApplicationFactory factory) =>
        CreateAuthenticatedClientAsync(factory, Organiser);

    public static Task<HttpClient> CreateParticipantClientAsync(RaceDayWebApplicationFactory factory) =>
        CreateAuthenticatedClientAsync(factory, Participant);
}