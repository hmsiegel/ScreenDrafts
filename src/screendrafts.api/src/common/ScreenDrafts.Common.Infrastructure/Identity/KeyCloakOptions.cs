namespace ScreenDrafts.Common.Infrastructure.Identity;

[System.Diagnostics.CodeAnalysis.SuppressMessage(
  "Design",
  "CA1056:URI-like properties should not be strings",
  Justification = "Reviewed and accepted for configuration purposes."
)]
public sealed class KeyCloakOptions
{
  public string AdminUrl { get; set; } = default!;

  public string TokenUrl { get; set; } = default!;

  public string ConfidentialClientId { get; set; } = default!;

  public string ConfidentialClientSecret { get; set; } = default!;

  public string PublicClientId { get; set; } = default!;
}
