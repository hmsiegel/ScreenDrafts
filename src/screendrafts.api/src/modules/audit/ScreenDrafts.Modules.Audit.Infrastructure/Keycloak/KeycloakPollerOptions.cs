namespace ScreenDrafts.Modules.Audit.Infrastructure.Keycloak;

internal sealed class KeycloakPollerOptions
{
  public const string SectionName = "Audit:KeycloakPoller";

  public int IntervalInSeconds { get; init; } = 60;
  public string AdminUrl { get; init; } = string.Empty;
  public string TokenUrl { get; init; } = string.Empty;
  public string ConfidentialClientId { get; init; } = string.Empty;
  public string ConfidentialClientSecret { get; init; } = string.Empty;
  public string Realm { get; init; } = string.Empty;
}
