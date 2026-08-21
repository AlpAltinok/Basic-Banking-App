using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;

namespace BankaApp.IntegrationTests;

public class WalletTransferFlowTests : IClassFixture<BankaAppFactory>
{
    private readonly HttpClient _client;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public WalletTransferFlowTests(BankaAppFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Health_Should_Return_Ok()
    {
        var response = await _client.GetAsync("/api/health");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Wallet_Without_Token_Should_Return_Unauthorized()
    {
        var response = await _client.GetAsync("/api/wallet");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Full_Flow_Register_Deposit_Transfer_Should_Succeed()
    {
        var alice = await RegisterAsync("alice.flow@example.com", "Alice");
        var bob = await RegisterAsync("bob.flow@example.com", "Bob");

        var deposit = await AuthorizedPostAsync(alice.AccessToken, "/api/wallet/deposit", new
        {
            amount = 1000m,
            description = "Initial funding"
        });
        deposit.StatusCode.Should().Be(HttpStatusCode.OK);

        var transfer = await AuthorizedPostAsync(alice.AccessToken, "/api/transfers", new
        {
            toEmail = "bob.flow@example.com",
            amount = 250m,
            description = "Rent share",
            idempotencyKey = "flow-key-001"
        });
        transfer.StatusCode.Should().Be(HttpStatusCode.OK);

        var transferBody = await transfer.Content.ReadFromJsonAsync<TransferDto>(JsonOptions);
        transferBody!.Amount.Should().Be(250m);
        transferBody.SenderBalanceAfter.Should().Be(750m);

        var aliceWallet = await GetWalletAsync(alice.AccessToken);
        var bobWallet = await GetWalletAsync(bob.AccessToken);
        aliceWallet.Balance.Should().Be(750m);
        bobWallet.Balance.Should().Be(250m);

        var replay = await AuthorizedPostAsync(alice.AccessToken, "/api/transfers", new
        {
            toEmail = "bob.flow@example.com",
            amount = 250m,
            idempotencyKey = "flow-key-001"
        });
        replay.StatusCode.Should().Be(HttpStatusCode.OK);
        (await GetWalletAsync(alice.AccessToken)).Balance.Should().Be(750m);
        (await GetWalletAsync(bob.AccessToken)).Balance.Should().Be(250m);
    }

    [Fact]
    public async Task Withdraw_With_Insufficient_Balance_Should_Return_BadRequest()
    {
        var user = await RegisterAsync("poor.user@example.com", "Poor");

        var response = await AuthorizedPostAsync(user.AccessToken, "/api/wallet/withdraw", new
        {
            amount = 50m
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var payload = await response.Content.ReadFromJsonAsync<ErrorDto>(JsonOptions);
        payload!.ErrorCode.Should().Be("INSUFFICIENT_BALANCE");
    }

    [Fact]
    public async Task Login_With_Wrong_Password_Should_Return_BadRequest()
    {
        await RegisterAsync("login.user@example.com", "Login User");

        var response = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            email = "login.user@example.com",
            password = "WrongPassword!"
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var payload = await response.Content.ReadFromJsonAsync<ErrorDto>(JsonOptions);
        payload!.ErrorCode.Should().Be("INVALID_CREDENTIALS");
    }

    private async Task<AuthDto> RegisterAsync(string email, string fullName)
    {
        var response = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            email,
            fullName,
            password = "Password123!"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<AuthDto>(JsonOptions);
        body.Should().NotBeNull();
        body!.AccessToken.Should().NotBeNullOrWhiteSpace();
        return body;
    }

    private async Task<WalletDto> GetWalletAsync(string token)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/wallet");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await _client.SendAsync(request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<WalletDto>(JsonOptions);
        body.Should().NotBeNull();
        return body!;
    }

    private async Task<HttpResponseMessage> AuthorizedPostAsync(string token, string url, object body)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Content = JsonContent.Create(body);
        return await _client.SendAsync(request);
    }

    private sealed class AuthDto
    {
        public string AccessToken { get; set; } = string.Empty;
    }

    private sealed class WalletDto
    {
        public decimal Balance { get; set; }
    }

    private sealed class TransferDto
    {
        public decimal Amount { get; set; }
        public decimal SenderBalanceAfter { get; set; }
    }

    private sealed class ErrorDto
    {
        public string ErrorCode { get; set; } = string.Empty;
    }
}
