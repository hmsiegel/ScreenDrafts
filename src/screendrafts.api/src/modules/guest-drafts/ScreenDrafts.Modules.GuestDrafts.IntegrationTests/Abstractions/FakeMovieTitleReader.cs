using System.Collections.Concurrent;

using ScreenDrafts.Modules.Movies.PublicApi;

namespace ScreenDrafts.Modules.GuestDrafts.IntegrationTests.Abstractions;

/// <summary>
/// In-memory replacement for <see cref="IMovieTitleReader"/> used by GuestDrafts
/// integration tests. Unlike Drafts (which keeps its own local read-replica "movies"
/// table synced via integration events and seeds it directly through DbContext),
/// GuestDrafts has no local movie cache -- PlayPickCommandHandler resolves movie
/// titles live via IMovieTitleReader. Seeding a real Movies-module row per test would
/// mean standing up that module's own aggregate just to get a title lookup, so this
/// fake lets a test register a MoviePublicId -> title mapping directly instead.
///
/// Registered as a singleton in GuestDraftsIntegrationTestWebAppFactory so every test
/// in the collection shares one instance.
/// </summary>
public sealed class FakeMovieTitleReader : IMovieTitleReader
{
  private readonly ConcurrentDictionary<string, string> _titlesByPublicId = new();
  private readonly ConcurrentDictionary<int, string> _publicIdsByTmdbId = new();

  /// <summary>
  /// Registers a fake movie and returns the MoviePublicId so the test can pass it
  /// straight into a command (e.g. PlayPickCommand.MoviePublicId).
  /// </summary>
  public string RegisterMovie(string publicId, string title, int? tmdbId = null)
  {
    _titlesByPublicId[publicId] = title;

    if (tmdbId.HasValue)
    {
      _publicIdsByTmdbId[tmdbId.Value] = publicId;
    }

    return publicId;
  }

  public void Reset()
  {
    _titlesByPublicId.Clear();
    _publicIdsByTmdbId.Clear();
  }

  public Task<IReadOnlyDictionary<string, string>> GetTitlesByPublicIdsAsync(
    IEnumerable<string> publicIds,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(publicIds);

    IReadOnlyDictionary<string, string> titles = publicIds
      .Where(_titlesByPublicId.ContainsKey)
      .ToDictionary(id => id, id => _titlesByPublicId[id]);

    return Task.FromResult(titles);
  }

  public Task<IReadOnlyList<string>> GetPublicIdsByTmdbIdsAsync(
    IReadOnlyList<int> tmdbIds,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(tmdbIds);

    IReadOnlyList<string> publicIds = tmdbIds
      .Where(_publicIdsByTmdbId.ContainsKey)
      .Select(id => _publicIdsByTmdbId[id])
      .ToList();

    return Task.FromResult(publicIds);
  }
}
