namespace ScreenDrafts.Modules.Drafts.Infrastructure.Converters;

internal static class UriConverters
{
  public static ValueConverter<Uri?, string?> UriConverter =>
    new(
      v => v == null ? null : v.AbsoluteUri,
      v => v == null ? null : new Uri(v));
}
