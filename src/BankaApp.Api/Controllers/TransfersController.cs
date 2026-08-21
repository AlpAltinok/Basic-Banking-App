using BankaApp.Api.Extensions;
using BankaApp.Application.DTOs.Transfer;
using BankaApp.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankaApp.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class TransfersController : ControllerBase
{
    private readonly ITransferService _transferService;

    public TransfersController(ITransferService transferService)
    {
        _transferService = transferService;
    }

    /// <summary>
    /// İki kullanıcı arasında para transferi (atomik).
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(TransferResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TransferResponse>> Transfer(
        [FromBody] TransferRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _transferService.TransferAsync(User.GetUserId(), request, cancellationToken);
        return Ok(result);
    }
}
