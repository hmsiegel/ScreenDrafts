using Microsoft.Extensions.Hosting;

using ScreenDrafts.Common.Application.Storage;

namespace ScreenDrafts.Common.Infrastructure.Storage;

/// <summary>
/// Development/Testing fallback: writes into wwwroot, which the API already
/// serves with UseStaticFiles() and docker-compose bind-mounts on the host.
/// </summary>
internal sealed class LocalFileStorage(IHostEnvironment environment) : IFileStorage
{
  private readonly string _root = Path.GetFullPath(
    Path.Combine(environment.ContentRootPath, "wwwroot")
  );

  public async Task UploadAsync(
    string key,
    Stream content,
    string contentType,
    string cacheControl,
    CancellationToken cancellationToken = default
  )
  {
    var path = Path.GetFullPath(Path.Combine(_root, key));

    if (!path.StartsWith(_root + Path.DirectorySeparatorChar, StringComparison.Ordinal))
    {
      throw new ArgumentException("Key escapes the storage root.", nameof(key));
    }

    Directory.CreateDirectory(Path.GetDirectoryName(path)!);

    await using var file = File.Create(path);
    await content.CopyToAsync(file, cancellationToken);
  }
}
