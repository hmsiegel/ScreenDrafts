namespace ScreenDrafts.Common.Application.Storage;

/// <summary>
/// Writes public, CDN-served files. Keys are folder-prefixed
/// (e.g. "drafters/p_abc-1a2b3c4d.jpg") and identical across dev and prod.
/// </summary>
public interface IFileStorage
{
  Task UploadAsync(
    string key,
    Stream content,
    string contentType,
    string cacheControl,
    CancellationToken cancellationToken = default
  );
}
