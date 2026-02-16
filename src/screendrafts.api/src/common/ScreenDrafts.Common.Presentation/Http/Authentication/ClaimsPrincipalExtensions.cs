using ScreenDrafts.Common.Abstractions.Authentication;

namespace ScreenDrafts.Common.Presentation.Http.Authentication;

public static class ClaimsPrincipalExtensions
{
  public static Guid GetUserId(this ClaimsPrincipal? principal)
  {
    string? userId = principal?.FindFirst(CustomClaims.Sub)?.Value;

    return Guid.TryParse(userId, out Guid parsedUserId) ?
        parsedUserId :
        throw new ScreenDraftsException("User identifier is unavailable");
  }

  public static string GetIdentityId(this ClaimsPrincipal? principal)
  {
    return principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
           throw new ScreenDraftsException("User identity is unavailable");
  }

  public static HashSet<string> GetPermissions(this ClaimsPrincipal? principal)
  {
    IEnumerable<Claim> permissionClaims = principal?.FindAll(CustomClaims.Permission) ??
                                          throw new ScreenDraftsException("Permissions are unavailable");

    return [.. permissionClaims.Select(c => c.Value)];
  }

  public static string GetPublicId(this ClaimsPrincipal? principal)
  {
    return principal?.FindFirst(CustomClaims.PublicId)?.Value ??
           throw new ScreenDraftsException("User public identifier is unavailable");
  }
}
