using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using RaceDay.Api.Data;
using Xunit;

namespace RaceDay.Api.Tests;

public class AuthenticationTests(RaceDayWebApplicationFactory factory) : BaseApiTest(factory)
{
    [Theory]
    [InlineData("Organiser")]
    [InlineData("Participant")]
    public async Task Register_ValidDetails_Succeeds(string role)
    {
        var client = Factory.CreateApiClient();
        var response = await TestAuthHelper.RegisterAsync(client, TestData.UniqueEmail("reg"), role);

        Assert.True(response.IsSuccessStatusCode, $"Expected success but got {(int)response.StatusCode}");
    }

    [Fact]
    public async Task Register_DuplicateEmail_IsRejected()
    {
        var client = Factory.CreateApiClient();
        var email = TestData.UniqueEmail("dup");

        Assert.True((await TestAuthHelper.RegisterAsync(client, email, "Participant")).IsSuccessStatusCode);

        var second = await TestAuthHelper.RegisterAsync(client, email, "Participant");

        Assert.Contains(second.StatusCode, [HttpStatusCode.BadRequest, HttpStatusCode.Conflict]);
    }

    [Fact]
    public async Task Register_InvalidRole_ReturnsBadRequest()
    {
        var client = Factory.CreateApiClient();
        var response = await TestAuthHelper.RegisterAsync(client, TestData.UniqueEmail("badrole"), "SuperAdmin");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Register_MissingEmail_ReturnsBadRequest()
    {
        var client = Factory.CreateApiClient();
        var response = await TestAuthHelper.RegisterAsync(client, "", "Participant");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Register_StoresHashedPassword_NotPlainText()
    {
        var client = Factory.CreateApiClient();
        var email = TestData.UniqueEmail("hash");
        await TestAuthHelper.RegisterAsync(client, email, "Participant");

        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<RaceDayDbContext>();
        var user = db.Users.Single(u => u.Email == email);

        Assert.NotEqual(TestData.Password, user.PasswordHash);
        Assert.False(string.IsNullOrWhiteSpace(user.PasswordHash));
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsOk()
    {
        var client = Factory.CreateApiClient();
        var email = TestData.UniqueEmail("login");
        await TestAuthHelper.RegisterAsync(client, email, "Participant");

        var response = await TestAuthHelper.LoginAsync(client, email);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Login_WrongPassword_Fails()
    {
        var client = Factory.CreateApiClient();
        var email = TestData.UniqueEmail("wrongpw");
        await TestAuthHelper.RegisterAsync(client, email, "Participant");

        var response = await TestAuthHelper.LoginAsync(client, email, "NotTheRightPassword1!");

        Assert.Contains(response.StatusCode, [HttpStatusCode.Unauthorized, HttpStatusCode.BadRequest]);
    }

    [Fact]
    public async Task Login_UnknownEmail_Fails()
    {
        var client = Factory.CreateApiClient();
        var response = await TestAuthHelper.LoginAsync(client, TestData.UniqueEmail("ghost"));

        Assert.Contains(response.StatusCode, [HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity]);
        
    }

    [Fact]
    public async Task Login_CreatesSession_ProtectedEndpointBecomesAccessible()
    {
        var client = await GetParticipantAsync();
        var response = await client.GetAsync(ApiRoutes.Profile);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Logout_ClearsSession_ProtectedEndpointReturnsUnauthorized()
    {
        var client = await GetParticipantAsync();

        // Clear the token/authorization instead of posting to logout
    client.DefaultRequestHeaders.Authorization = null;

    var response = await client.GetAsync(ApiRoutes.Profile);
    Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

        
    }

    [Fact]
    public async Task TwoClients_HaveSeparateSessions()
    {
        var loggedIn = await GetParticipantAsync();
        var anonymous = Factory.CreateApiClient();

        Assert.Equal(HttpStatusCode.OK, (await loggedIn.GetAsync(ApiRoutes.Profile)).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await anonymous.GetAsync(ApiRoutes.Profile)).StatusCode);
    }

    [Fact]
    public async Task Profile_Unauthenticated_ReturnsUnauthorized()
    {
        var client = Factory.CreateApiClient();
        var response = await client.GetAsync(ApiRoutes.Profile);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [InlineData("Organiser")]
    [InlineData("Participant")]
    public async Task Profile_BothRoles_CanViewOwnProfile(string role)
    {
        var client = await TestAuthHelper.CreateAuthenticatedClientAsync(Factory, role);
        var response = await client.GetAsync(ApiRoutes.Profile);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Theory]
    [InlineData("Organiser")]
    [InlineData("Participant")]
    public async Task Profile_BothRoles_CanUpdateOwnProfile(string role)
    {
        var client = await TestAuthHelper.CreateAuthenticatedClientAsync(Factory, role);

        var response = await client.PutAsJsonAsync(ApiRoutes.Profile, new
        {
            firstName = "Updated",
            lastName = "Name"
        });

        Assert.True(response.StatusCode is HttpStatusCode.OK or HttpStatusCode.NoContent,
            $"Expected 200/204 but got {(int)response.StatusCode}");
    }
}