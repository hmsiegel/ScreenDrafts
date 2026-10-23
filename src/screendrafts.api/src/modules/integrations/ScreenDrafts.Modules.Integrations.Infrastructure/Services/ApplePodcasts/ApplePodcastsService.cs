using ScreenDrafts.Modules.Integrations.Domain.Services.Podcasts;

namespace ScreenDrafts.Modules.Integrations.Infrastructure.Services.ApplePodcasts;

internal sealed class ApplePodcastsService(
  HttpClient httpClient,
  IOptions<ApplePodcastsSettings> settings
) : IApplePodcastsService
{
  private const int MaxLimit = 200;
  private const string EpisodeKind = "podcast-episode";

  private readonly HttpClient _httpClient = httpClient;
  private readonly ApplePodcastsSettings _settings = settings.Value;

  public async Task<IReadOnlyList<PodcastEpisode>> ListEpisodesAsync(
    DateOnly releasedOnOrAfter,
    CancellationToken cancellationToken = default
  )
  {
    var uri = new Uri(
      _httpClient.BaseAddress!,
      $"lookup?id={Uri.EscapeDataString(_settings.CollectionId)}"
        + $"&country={Uri.EscapeDataString(_settings.Country)}"
        + $"&media=podcast&entity=podcastEpisode&limit={MaxLimit}"
    );

    using var response = await _httpClient.GetAsync(uri, cancellationToken);
    response.EnsureSuccessStatusCode();

    var payload = await response.Content.ReadFromJsonAsync<AppleLookupResponse>(
      cancellationToken: cancellationToken
    );

    return (payload?.Results ?? [])
      .Where(r => r.Kind == EpisodeKind)
      .Select(ToEpisode)
      .OfType<PodcastEpisode>()
      .Where(e => e.ReleaseDate >= releasedOnOrAfter)
      .ToList();
  }

  private static PodcastEpisode? ToEpisode(AppleLookupItem item)
  {
    if (item.TrackName is null || item.ReleaseDate is null)
    {
      return null;
    }

    var url = ToEpisodeUrl(item.TrackViewUrl);

    return url is null
      ? null
      : new PodcastEpisode(
        item.TrackName,
        DateOnly.FromDateTime(item.ReleaseDate.Value.UtcDateTime),
        url
      );
  }

  /// <summary>
  /// Apple appends tracking parameters (e.g. "uo=4"). Keep only the "i"
  /// parameter, which identifies the episode.
  /// </summary>
  private static Uri? ToEpisodeUrl(string? trackViewUrl)
  {
    if (!Uri.TryCreate(trackViewUrl, UriKind.Absolute, out var url))
    {
      return null;
    }

    var episodeParameter = url
      .Query.TrimStart('?')
      .Split('&')
      .FirstOrDefault(p => p.StartsWith("i=", StringComparison.Ordinal));

    var path = url.GetLeftPart(UriPartial.Path);

    return new Uri(episodeParameter is null ? path : $"{path}?{episodeParameter}");
  }

  // ── Raw API response shapes (private, mapped into PodcastEpisode above) ──

  private sealed record AppleLookupResponse
  {
    [JsonPropertyName("results")]
    public List<AppleLookupItem>? Results { get; init; }
  }

  private sealed record AppleLookupItem
  {
    [JsonPropertyName("kind")]
    public string? Kind { get; init; }

    [JsonPropertyName("trackName")]
    public string? TrackName { get; init; }

    [JsonPropertyName("releaseDate")]
    public DateTimeOffset? ReleaseDate { get; init; }

    [JsonPropertyName("trackViewUrl")]
    public string? TrackViewUrl { get; init; }
  }
}
