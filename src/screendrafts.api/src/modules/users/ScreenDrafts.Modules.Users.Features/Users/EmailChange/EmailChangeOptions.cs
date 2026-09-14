namespace ScreenDrafts.Modules.Users.Features.Users.EmailChange;

internal sealed class EmailChangeOptions
{
  public const string SectionName = "Users:EmailChange";
  public string ConfirmationBaseUrl { get; set; } = default!;
  public int TokenExpiryHours { get; set; } = 1;
}
