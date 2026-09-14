namespace ScreenDrafts.Modules.Users.Infrastructure.Identity;

internal sealed class EmailBootstrapOptions
{
  public const string SectionName = "Users:EmailBootstrap";
  public string Secret { get; set; } = default!;
  public int DefaultExpiryHours { get; set; } = 72;
}
