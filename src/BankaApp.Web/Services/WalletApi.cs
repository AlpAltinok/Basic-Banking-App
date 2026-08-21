using System.Net.Http.Json;
using System.Text.Json;
using BankaApp.Web.Models;
using Microsoft.JSInterop;

namespace BankaApp.Web.Services;

public class AuthSession
{
    private const string TokenKey = "bankaapp.token";
    private const string EmailKey = "bankaapp.email";
    private const string NameKey = "bankaapp.name";

    private readonly IJSRuntime _js;
    private bool _loaded;

    public AuthSession(IJSRuntime js) => _js = js;

    public string? AccessToken { get; private set; }
    public string? Email { get; private set; }
    public string? FullName { get; private set; }
    public bool IsAuthenticated => !string.IsNullOrWhiteSpace(AccessToken);

    public event Action? Changed;

    public async Task EnsureLoadedAsync()
    {
        if (_loaded) return;
        try
        {
            AccessToken = await _js.InvokeAsync<string?>("localStorage.getItem", TokenKey);
            Email = await _js.InvokeAsync<string?>("localStorage.getItem", EmailKey);
            FullName = await _js.InvokeAsync<string?>("localStorage.getItem", NameKey);
        }
        catch (JSException)
        {
            // JS not ready yet (rare); treat as logged out until next load.
        }
        catch (InvalidOperationException)
        {
            // Interop not available during early render; retry next call.
            return;
        }

        _loaded = true;
    }

    public async Task SetAsync(AuthResponse auth)
    {
        AccessToken = auth.AccessToken;
        Email = auth.Email;
        FullName = auth.FullName;
        await _js.InvokeVoidAsync("localStorage.setItem", TokenKey, auth.AccessToken);
        await _js.InvokeVoidAsync("localStorage.setItem", EmailKey, auth.Email);
        await _js.InvokeVoidAsync("localStorage.setItem", NameKey, auth.FullName);
        Changed?.Invoke();
    }

    public async Task ClearAsync()
    {
        AccessToken = null;
        Email = null;
        FullName = null;
        await _js.InvokeVoidAsync("localStorage.removeItem", TokenKey);
        await _js.InvokeVoidAsync("localStorage.removeItem", EmailKey);
        await _js.InvokeVoidAsync("localStorage.removeItem", NameKey);
        Changed?.Invoke();
    }
}

public class AuthHeaderHandler : DelegatingHandler
{
    private readonly AuthSession _session;

    public AuthHeaderHandler(AuthSession session) => _session = session;

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        await _session.EnsureLoadedAsync();

        // Always use the live in-memory token (updated on login/logout).
        if (!string.IsNullOrWhiteSpace(_session.AccessToken))
        {
            request.Headers.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _session.AccessToken);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
public class WalletApi
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _http;

    public WalletApi(HttpClient http) => _http = http;

    public Task<AuthResponse> RegisterAsync(string email, string fullName, string password)
        => PostAsync<AuthResponse>("api/auth/register", new { email, fullName, password });

    public Task<AuthResponse> LoginAsync(string email, string password)
        => PostAsync<AuthResponse>("api/auth/login", new { email, password });

    public Task<WalletResponse> GetWalletAsync()
        => GetAsync<WalletResponse>("api/wallet");

    public Task<WalletResponse> DepositAsync(decimal amount, string? description = null)
        => PostAsync<WalletResponse>("api/wallet/deposit", new { amount, description });

    public Task<WalletResponse> WithdrawAsync(decimal amount, string? description = null)
        => PostAsync<WalletResponse>("api/wallet/withdraw", new { amount, description });

    public Task<List<TransactionResponse>> GetTransactionsAsync(int take = 20)
        => GetAsync<List<TransactionResponse>>($"api/wallet/transactions?take={take}");

    public Task<TransferResponse> TransferAsync(
        string toEmail,
        decimal amount,
        string? description = null,
        string? idempotencyKey = null)
        => PostAsync<TransferResponse>("api/transfers", new { toEmail, amount, description, idempotencyKey });

    private async Task<T> GetAsync<T>(string url)
    {
        var response = await _http.GetAsync(url);
        return await ReadAsync<T>(response);
    }

    private async Task<T> PostAsync<T>(string url, object body)
    {
        var response = await _http.PostAsJsonAsync(url, body);
        return await ReadAsync<T>(response);
    }

    private static async Task<T> ReadAsync<T>(HttpResponseMessage response)
    {
        var json = await response.Content.ReadAsStringAsync();
        if (response.IsSuccessStatusCode)
        {
            return JsonSerializer.Deserialize<T>(json, JsonOptions)
                   ?? throw new InvalidOperationException("Empty response.");
        }

        var error = JsonSerializer.Deserialize<ApiError>(json, JsonOptions);
        throw new ApiException(
            error?.ErrorCode ?? "HTTP_ERROR",
            error?.Message ?? $"Request failed ({(int)response.StatusCode}).",
            response.StatusCode);
    }
}

public class ApiException : Exception
{
    public string ErrorCode { get; }
    public System.Net.HttpStatusCode StatusCode { get; }

    public ApiException(string errorCode, string message, System.Net.HttpStatusCode statusCode)
        : base(message)
    {
        ErrorCode = errorCode;
        StatusCode = statusCode;
    }
}
