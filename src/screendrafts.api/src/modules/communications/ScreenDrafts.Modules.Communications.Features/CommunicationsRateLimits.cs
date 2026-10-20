namespace ScreenDrafts.Modules.Communications.Features;

internal static class CommunicationsRateLimits
{
  /// <summary>
  /// Name of the ASP.NET rate-limiter policy applied to the contact endpoint. The policy
  /// itself is registered in Program.cs — keep the string "contact" in sync there.
  /// </summary>
  public const string Contact = "contact";
}
