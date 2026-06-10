using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;

namespace KBM.Client.Services;

public sealed class AuthApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };
    private readonly HttpClient _http;

    public AuthApiClient(string baseUrl = "http://localhost:5262")
    {
        _http = new HttpClient { BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/") };
    }

    public async Task<SetupStatusResult?> GetSetupStatusAsync()
    {
        var response = await _http.GetAsync("api/setup/status");
        if (!response.IsSuccessStatusCode) return null;
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<SetupStatusResult>(json, JsonOptions);
    }

    public async Task<bool> CompleteSetupAsync(SetupCompleteRequest request)
    {
        var response = await _http.PostAsJsonAsync("api/setup/complete", request);
        return response.IsSuccessStatusCode;
    }

    public async Task<LoginOutcome?> LoginAsync(string username, string password, long? companyId = null)
    {
        var response = await _http.PostAsJsonAsync("api/auth/login", new { username, password, companyId });
        if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            return LoginOutcome.Disabled();
        if (!response.IsSuccessStatusCode) return null;
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<LoginOutcome>(json, JsonOptions);
    }
}

public record SetupStatusResult(bool Required, bool Completed, string? Message);

public record SetupCompleteRequest(
    string CompanyCode,
    string BusinessName,
    string? VatNumber,
    string AdminUsername,
    string AdminEmail,
    string AdminPassword,
    string AdminFirstName,
    string AdminLastName);

public record LoginOutcome(
    bool RequiresCompanySelection,
    LoginSession? Session,
    IReadOnlyList<CompanyOption>? Companies)
{
    public static LoginOutcome Disabled() => new(false, null, null) { IsDisabled = true };
    public bool IsDisabled { get; init; }
}

public record CompanyOption(long Id, string Code, string BusinessName);

public record LoginSession(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    long UserId,
    string Username,
    string DisplayName,
    long CompanyId,
    string CompanyName,
    IReadOnlyList<string>? Roles);
