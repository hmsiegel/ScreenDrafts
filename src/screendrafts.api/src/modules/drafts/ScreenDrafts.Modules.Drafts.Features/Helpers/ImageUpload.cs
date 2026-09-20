namespace ScreenDrafts.Modules.Drafts.Features.Helpers;

internal static class ImageUpload
{
  public const string DraftersFolder = "drafters";
  public const string DraftsFolder = "drafts";

  // Filenames are unique per upload, so the CDN and browsers may cache forever.
  public const string CacheControl = "public, max-age=31536000, immutable";

  private static readonly Dictionary<string, string> _extensions = new(
    StringComparer.OrdinalIgnoreCase
  )
  {
    ["image/jpeg"] = "jpg",
    ["image/png"] = "png",
    ["image/webp"] = "webp",
  };

  public static string? GetExtension(string contentType) =>
    _extensions.GetValueOrDefault(contentType);

  // "p_abc123-1a2b3c4d.jpg"
  public static string BuildFileName(string publicId, string extension) =>
    $"{publicId}-{Convert.ToHexStringLower(RandomNumberGenerator.GetBytes(4))}.{extension}";
}
