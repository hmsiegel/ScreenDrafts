namespace ScreenDrafts.Common.Domain;

/// <summary>
/// Users registered before the real-email migration carry a fake address on this domain.
/// Nothing should ever be sent to one.
/// </summary>
public static class PlaceholderEmail
{
  public const string Domain = "screendrafts.fake";
  private const string PlaceholderTld = ".fake";

  public static bool IsPlaceholder(string? emailAddress) =>
    !string.IsNullOrWhiteSpace(emailAddress)
    && emailAddress.Trim().EndsWith(PlaceholderTld, StringComparison.OrdinalIgnoreCase);
}
