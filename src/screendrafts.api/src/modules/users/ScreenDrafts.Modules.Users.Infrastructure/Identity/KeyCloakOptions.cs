namespace ScreenDrafts.Modules.Users.Infrastructure.Identity;
internal sealed class KeyCloakOptions
{
  public string AdminUrl { get; set; } = default!;

  public string TokenUrl { get; set; } = default!;

  public string ConfidentialClientId { get; set; } = default!;

  public string ConfidentialClientSecret { get; set; } = default!;

  public string PublicClientId { get; set; } = default!;
}
