using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace KBM.IntegrationTests;

public class AuthSmokeTests
{
    private static readonly JsonSerializerOptions Json = new() { PropertyNameCaseInsensitive = true };

    [Fact]
    public async Task Setup_Status_InitiallyRequired()
    {
        await using var factory = new KbmWebApplicationFactory();
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/setup/status");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<SetupStatusDto>(Json);
        Assert.NotNull(body);
        Assert.True(body.Required);
        Assert.False(body.Completed);
    }

    [Fact]
    public async Task Setup_Complete_Then_Login_Succeeds()
    {
        await using var factory = new KbmWebApplicationFactory();
        var client = factory.CreateClient();

        var setupResponse = await client.PostAsJsonAsync("/api/setup/complete", new
        {
            companyCode = "TEST",
            businessName = "Test Srl",
            vatNumber = "12345678901",
            adminUsername = "admin",
            adminEmail = "admin@test.local",
            adminPassword = "Admin123!",
            adminFirstName = "Test",
            adminLastName = "User"
        });
        Assert.Equal(HttpStatusCode.OK, setupResponse.StatusCode);

        var status = await client.GetFromJsonAsync<SetupStatusDto>("/api/setup/status", Json);
        Assert.NotNull(status);
        Assert.False(status.Required);
        Assert.True(status.Completed);

        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new
        {
            username = "admin",
            password = "Admin123!"
        });
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        var login = await loginResponse.Content.ReadFromJsonAsync<LoginResultDto>(Json);
        Assert.NotNull(login);
        Assert.False(login.RequiresCompanySelection);
        Assert.NotNull(login.Session);
        Assert.False(string.IsNullOrEmpty(login.Session.AccessToken));
        Assert.Equal("admin", login.Session.Username);
    }

    [Fact]
    public async Task Login_WithWrongPassword_ReturnsUnauthorized()
    {
        await using var factory = new KbmWebApplicationFactory();
        var client = factory.CreateClient();

        await client.PostAsJsonAsync("/api/setup/complete", new
        {
            companyCode = "T2",
            businessName = "T2 Srl",
            adminUsername = "user2",
            adminEmail = "u2@test.local",
            adminPassword = "Admin123!",
            adminFirstName = "U",
            adminLastName = "2"
        });

        var response = await client.PostAsJsonAsync("/api/auth/login", new
        {
            username = "user2",
            password = "wrong"
        });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private record SetupStatusDto(bool Required, bool Completed, string? Message);
    private record LoginResultDto(bool RequiresCompanySelection, LoginSessionDto? Session);
    private record LoginSessionDto(string AccessToken, string RefreshToken, string Username);
}
