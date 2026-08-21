using BankaApp.Api.Extensions;
using BankaApp.Application.DTOs.Wallet;
using BankaApp.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankaApp.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class WalletController : ControllerBase
{
    private readonly IWalletService _walletService;

    public WalletController(IWalletService walletService)
    {
        _walletService = walletService;
    }

    /// <summary>
    /// Giriş yapan kullanıcının cüzdan bakiyesi.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(WalletResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<WalletResponse>> GetMyWallet(CancellationToken cancellationToken)
    {
        var result = await _walletService.GetMyWalletAsync(User.GetUserId(), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Para yatırma (deposit).
    /// </summary>
    [HttpPost("deposit")]
    [ProducesResponseType(typeof(WalletResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<WalletResponse>> Deposit(
        [FromBody] MoneyOperationRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _walletService.DepositAsync(User.GetUserId(), request, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Para çekme (withdraw). Yetersiz bakiyede 400 döner.
    /// </summary>
    [HttpPost("withdraw")]
    [ProducesResponseType(typeof(WalletResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<WalletResponse>> Withdraw(
        [FromBody] MoneyOperationRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _walletService.WithdrawAsync(User.GetUserId(), request, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Son hareketler (ledger).
    /// </summary>
    [HttpGet("transactions")]
    [ProducesResponseType(typeof(IReadOnlyList<TransactionResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<TransactionResponse>>> GetTransactions(
        [FromQuery] int take = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _walletService.GetMyTransactionsAsync(User.GetUserId(), take, cancellationToken);
        return Ok(result);
    }
}
