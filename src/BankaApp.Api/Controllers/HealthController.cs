using Microsoft.AspNetCore.Mvc;

namespace BankaApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
        => Ok(new
        {
            status = "Healthy",
            service = "BankaApp Digital Wallet API",
            utc = DateTime.UtcNow
        });
}
