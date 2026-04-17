namespace ScreenDrafts.Modules.Audit.Infrastructure.Redaction;

internal static class BodyRedactionPolicy
{
  private static readonly string[] _redactedPrefixes =
   [
    "/users/register",
    "/users/login",
    "/users/reset-password",
    "/users/change-password",
    "/auth",
    "/connect/token"
   ];

   public static bool ShouldRedact(PathString path)
   {
     var pathValue = path.Value ?? string.Empty;
     return _redactedPrefixes
      .Any(prefix => 
        pathValue.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
   }
}
