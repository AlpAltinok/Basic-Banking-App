using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace BankaApp.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// JWT içindeki kullanıcı Id'sini okur (sub claim).
    /// </summary>
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var value = user.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? user.FindFirstValue(JwtRegisteredClaimNames.Sub)
                    ?? user.FindFirstValue("sub");

        if (value is null || !Guid.TryParse(value, out var userId))
        {
            throw new UnauthorizedAccessException("Geçerli kullanıcı kimliği bulunamadı.");
        }

        return userId;
    }
}
