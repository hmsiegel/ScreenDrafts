namespace ScreenDrafts.Common.Application.Services;

public static class PublicIdGuards
{
  public static bool IsValid(string? publicId)
    => !string.IsNullOrWhiteSpace(publicId)
    && publicId.Contains('_', StringComparison.OrdinalIgnoreCase);

  public static bool IsValidWithPrefix(string? publicId, string prefix)
  {
    ArgumentNullException.ThrowIfNull(prefix);

    if (string.IsNullOrWhiteSpace(publicId))
    {
      return false;
    }

    if (!publicId.StartsWith(prefix + "_", StringComparison.OrdinalIgnoreCase))
    {
      return false;
    }

    if (!HasValidSuffix(publicId, prefix))
    {
      return false;
    }

    return publicId.Length > prefix.Length + 1;
  }

  private static bool HasValidSuffix(string publicId, string prefix)
  {
    var suffix = publicId[(prefix.Length + 1)..];

    if (suffix.Length < 8)
    {
      return false;
    }

    return suffix.All(char.IsLetterOrDigit);
  }
}
