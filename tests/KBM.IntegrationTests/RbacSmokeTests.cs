using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace KBM.IntegrationTests;

public class RbacSmokeTests
{
    private static readonly JsonSerializerOptions Json = new() { PropertyNameCaseInsensitive = true };

    [Fact]
    public async Task Admin_Can_List_Users_After_Login()
    {
        await using var factory = new KbmWebApplicationFactory();
        var client = factory.CreateClient();

        await SetupAdminAsync(client);

        var login = await LoginAsync(client, "rbacadmin", "Admin123!");
        Assert.NotNull(login.Session);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login.Session!.AccessToken);

        var response = await client.GetAsync("/api/users");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<UsersPageDto>(Json);
        Assert.NotNull(body);
        Assert.True(body!.Total >= 1);
        Assert.Contains(body.Items, u => u.Username == "rbacadmin");
    }

    [Fact]
    public async Task Users_Endpoint_Requires_Auth()
    {
        await using var factory = new KbmWebApplicationFactory();
        var client = factory.CreateClient();
        var response = await client.GetAsync("/api/users");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private static async Task SetupAdminAsync(HttpClient client)
    {
        await client.PostAsJsonAsync("/api/setup/complete", new
        {
            companyCode = "RBAC",
            businessName = "RBAC Test Srl",
            adminUsername = "rbacadmin",
            adminEmail = "rbac@test.local",
            adminPassword = "Admin123!",
            adminFirstName = "Rbac",
            adminLastName = "Admin"
        });
    }

    private static async Task<LoginResultDto> LoginAsync(HttpClient client, string user, string pass)
    {
        var response = await client.PostAsJsonAsync("/api/auth/login", new { username = user, password = pass });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<LoginResultDto>(Json))!;
    }

    private record LoginResultDto(bool RequiresCompanySelection, LoginSessionDto? Session);
    private record LoginSessionDto(string AccessToken, string RefreshToken, string Username);
    private record UsersPageDto(IReadOnlyList<UserDto> Items, int Total);
    private record UserDto(string Username, string Status);
}
