using ScreenDrafts.Modules.Integrations.Domain.Services.Podcasts;

namespace ScreenDrafts.Modules.Integrations.Infrastructure.Services.Spotify;

internal sealed class SpotifyService(
  HttpClient httpClient,
  SpotifyTokenCache tokenCache,
  IOptions<SpotifySettings> settings
) : ISpotifyService
{
  private const int PageSize = 50;

  private readonly HttpClient _httpClient = httpClient;
  private readonly SpotifyTokenCache _tokenCache = tokenCache;
  private readonly SpotifySettings _settings = settings.Value;

  public async Task<IReadOnlyList<PodcastEpisode>> ListEpisodesAsync(
    DateOnly releasedOnOrAfter,
    CancellationToken cancellationToken = default
  )
  {
    var token = await GetAccessTokenAsync(cancellationToken);
    var episodes = new List<PodcastEpisode>();
    var offset = 0;
    bool hasMore;

    do
    {
      var page = await GetPageAsync(token, offset, cancellationToken);

      var pageEpisodes = (page.Items ?? [])
        .Select(ToEpisode)
        .OfType<PodcastEpisode>()
        .ToList();

      episodes.AddRange(pageEpisodes);
      offset += PageSize;
      hasMore = page.Next is not null && !ReachedCutoff(pageEpisodes, releasedOnOrAfter);
    } while (hasMore);

    return episodes;
  }

  private async Task<SpotifyEpisodesResponse> GetPageAsync(
    string token,
    int offset,
    CancellationToken cancellationToken
  )
  {
    var uri = new Uri(
      _httpClient.BaseAddress!,
      $"shows/{Uri.EscapeDataString(_settings.ShowId)}/episodes"
        + $"?market={Uri.EscapeDataString(_settings.Market)}&limit={PageSize}&offset={offset}"
    );

    using var request = new HttpRequestMessage(HttpMethod.Get, uri)
    {
      Headers = { Authorization = new AuthenticationHeaderValue("Bearer", token) },
    };

    using var response = await _httpClient.SendAsync(request, cancellationToken);

    if (response.StatusCode == HttpStatusCode.Unauthorized)
    {
      _tokenCache.Clear();
    }

    response.EnsureSuccessStatusCode();

    var payload = await response.Content.ReadFromJsonAsync<SpotifyEpisodesResponse>(
      cancellationToken: cancellationToken
    );

    return payload ?? new SpotifyEpisodesResponse();
  }

  private async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken)
  {
    if (_tokenCache.TryGet() is { } cached)
    {
      return cached;
    }

    var credentials = Convert.ToBase64String(
      Encoding.UTF8.GetBytes($"{_settings.ClientId}:{_settings.ClientSecret}")
    );

    using var request = new HttpRequestMessage(HttpMethod.Post, _settings.TokenUrl)
    {
      Content = new FormUrlEncodedContent(
        [new KeyValuePair<string, string>("grant_type", "client_credentials")]
      ),
      Headers = { Authorization = new AuthenticationHeaderValue("Basic", credentials) },
    };

    using var response = await _httpClient.SendAsync(request, cancellationToken);
    response.EnsureSuccessStatusCode();

    var payload = await response.Content.ReadFromJsonAsync<SpotifyTokenResponse>(
      cancellationToken: cancellationToken
    );

    if (payload?.AccessToken is null)
    {
      throw new HttpRequestException("Spotify returned no access token.");
    }

    _tokenCache.Set(payload.AccessToken, TimeSpan.FromSeconds(payload.ExpiresIn));

    return payload.AccessToken;
  }

  /// <summary>
  /// Spotify does not document the episode order. Stop paging early only when
  /// the page is newest-first and its oldest episode already predates the
  /// cutoff; otherwise page through everything.
  /// </summary>
  private static bool ReachedCutoff(List<PodcastEpisode> page, DateOnly cutoff) =>
    page.Count > 1 && page[0].ReleaseDate >= page[^1].ReleaseDate && page[^1].ReleaseDate < cutoff;

  private static PodcastEpisode? ToEpisode(SpotifyEpisodeItem? item)
  {
    if (item?.Name is null || item.ExternalUrls?.Spotify is null)
    {
      return null;
    }

    if (
      !DateOnly.TryParseExact(
        item.ReleaseDate,
        "yyyy-MM-dd",
        CultureInfo.InvariantCulture,
        DateTimeStyles.None,
        out var releaseDate
      )
    )
    {
      return null;
    }

    if (!Uri.TryCreate(item.ExternalUrls.Spotify, UriKind.Absolute, out var url))
    {
      return null;
    }

    return new PodcastEpisode(item.Name, releaseDate, new Uri(url.GetLeftPart(UriPartial.Path)));
  }

  // ── Raw API response shapes (private, mapped into PodcastEpisode above) ──

  private sealed record SpotifyTokenResponse
  {
    [JsonPropertyName("access_token")]
    public string? AccessToken { get; init; }

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; init; }
  }

  private sealed record SpotifyEpisodesResponse
  {
    [JsonPropertyName("items")]
    public List<SpotifyEpisodeItem?>? Items { get; init; }

    [JsonPropertyName("next")]
    public string? Next { get; init; }
  }

  private sealed record SpotifyEpisodeItem
  {
    [JsonPropertyName("name")]
    public string? Name { get; init; }

    [JsonPropertyName("release_date")]
    public string? ReleaseDate { get; init; }

    [JsonPropertyName("external_urls")]
    public SpotifyExternalUrls? ExternalUrls { get; init; }
  }

  private sealed record SpotifyExternalUrls
  {
    [JsonPropertyName("spotify")]
    public string? Spotify { get; init; }
  }
}
