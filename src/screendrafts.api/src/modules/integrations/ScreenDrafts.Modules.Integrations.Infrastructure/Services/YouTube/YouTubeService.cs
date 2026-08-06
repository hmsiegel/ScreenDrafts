namespace ScreenDrafts.Modules.Integrations.Infrastructure.Services.YouTube;

internal sealed class YouTubeService(
  HttpClient httpClient,
  IOptions<YouTubeSettings> youTubeSettings
) : IYouTubeService
{
  private readonly HttpClient _httpClient = httpClient;
  private readonly YouTubeSettings _youTubeSettings = youTubeSettings.Value;

  public async Task<YouTubeSearchPagedResult> SearchAsync(
    string query,
    int page = 1,
    CancellationToken cancellationToken = default
  )
  {
    // YouTube's search.list paginates via opaque pageToken, not a page
    // number — there's no way to jump directly to "page 3" the way
    // TMDb/OMDb allow. page > 1 isn't supported without a stored token from
    // the previous response; callers past page 1 will just get page 1 again
    // until this is revisited with real pageToken plumbing through the
    // caller chain.
    var url =
      $"search?part=snippet&type=video&maxResults=10&q={Uri.EscapeDataString(query)}&key={_youTubeSettings.Key}";

    var uri = new Uri(_httpClient.BaseAddress!, url);

    using var response = await _httpClient.GetAsync(uri, cancellationToken);

    if (!response.IsSuccessStatusCode)
    {
      return new YouTubeSearchPagedResult();
    }

    var payload = await response.Content.ReadFromJsonAsync<YouTubeSearchResponse>(
      cancellationToken: cancellationToken
    );

    if (payload?.Items is null)
    {
      return new YouTubeSearchPagedResult();
    }

    var results = payload
      .Items.Where(i => i.Id?.VideoId is not null)
      .Select(i => new YouTubeSearchResult
      {
        VideoId = i.Id!.VideoId!,
        Title = i.Snippet?.Title ?? string.Empty,
        ChannelTitle = i.Snippet?.ChannelTitle,
        ThumbnailUrl =
          new Uri(i.Snippet?.Thumbnails?.Medium?.Url!)
          ?? new Uri(i.Snippet?.Thumbnails?.Default?.Url!),
        PublishedAt = i.Snippet?.PublishedAt,
      })
      .ToList();

    return new YouTubeSearchPagedResult
    {
      Results = results,
      NextPageToken = payload.NextPageToken,
    };
  }

  public async Task<YouTubeVideoDetails?> GetVideoDetailsAsync(
    string videoId,
    CancellationToken cancellationToken = default
  )
  {
    var url =
      $"videos?part=snippet,contentDetails&id={Uri.EscapeDataString(videoId)}&key={_youTubeSettings.Key}";

    var uri = new Uri(_httpClient.BaseAddress!, url);

    using var response = await _httpClient.GetAsync(uri, cancellationToken);

    if (!response.IsSuccessStatusCode)
    {
      return null;
    }

    var payload = await response.Content.ReadFromJsonAsync<YouTubeVideosResponse>(
      cancellationToken: cancellationToken
    );

    var item = payload?.Items?.FirstOrDefault();

    if (item is null)
    {
      return null;
    }

    return new YouTubeVideoDetails
    {
      VideoId = item.Id ?? videoId,
      Title = item.Snippet?.Title ?? string.Empty,
      Description = item.Snippet?.Description,
      ChannelTitle = item.Snippet?.ChannelTitle,
      ThumbnailUrl = new Uri(item.Snippet?.Thumbnails?.Medium?.Url!),
      PublishedAt = item.Snippet?.PublishedAt,
      DurationSeconds = ParseIso8601Duration(item.ContentDetails?.Duration),
    };
  }

  /// <summary>
  /// YouTube returns duration as an ISO 8601 duration string (e.g. "PT4M13S"),
  /// not seconds. .NET's XmlConvert.ToTimeSpan parses this format directly.
  /// </summary>
  private static int ParseIso8601Duration(string? iso8601Duration)
  {
    if (string.IsNullOrWhiteSpace(iso8601Duration))
    {
      return 0;
    }

    try
    {
      return (int)System.Xml.XmlConvert.ToTimeSpan(iso8601Duration).TotalSeconds;
    }
    catch (FormatException)
    {
      return 0;
    }
  }

  // ── Raw API response shapes (private, mapped into the public records above) ──

  private sealed record YouTubeSearchResponse
  {
    [JsonPropertyName("nextPageToken")]
    public string? NextPageToken { get; init; }

    [JsonPropertyName("items")]
    public List<YouTubeSearchItem>? Items { get; init; }
  }

  private sealed record YouTubeSearchItem
  {
    [JsonPropertyName("id")]
    public YouTubeSearchItemId? Id { get; init; }

    [JsonPropertyName("snippet")]
    public YouTubeSnippet? Snippet { get; init; }
  }

  private sealed record YouTubeSearchItemId
  {
    [JsonPropertyName("videoId")]
    public string? VideoId { get; init; }
  }

  private sealed record YouTubeVideosResponse
  {
    [JsonPropertyName("items")]
    public List<YouTubeVideoItem>? Items { get; init; }
  }

  private sealed record YouTubeVideoItem
  {
    [JsonPropertyName("id")]
    public string? Id { get; init; }

    [JsonPropertyName("snippet")]
    public YouTubeSnippet? Snippet { get; init; }

    [JsonPropertyName("contentDetails")]
    public YouTubeContentDetails? ContentDetails { get; init; }
  }

  private sealed record YouTubeContentDetails
  {
    [JsonPropertyName("duration")]
    public string? Duration { get; init; }
  }

  private sealed record YouTubeSnippet
  {
    [JsonPropertyName("title")]
    public string? Title { get; init; }

    [JsonPropertyName("description")]
    public string? Description { get; init; }

    [JsonPropertyName("channelTitle")]
    public string? ChannelTitle { get; init; }

    [JsonPropertyName("publishedAt")]
    public string? PublishedAt { get; init; }

    [JsonPropertyName("thumbnails")]
    public YouTubeThumbnails? Thumbnails { get; init; }
  }

  private sealed record YouTubeThumbnails
  {
    [JsonPropertyName("default")]
    public YouTubeThumbnail? Default { get; init; }

    [JsonPropertyName("medium")]
    public YouTubeThumbnail? Medium { get; init; }
  }

  private sealed record YouTubeThumbnail
  {
    [JsonPropertyName("url")]
    public string? Url { get; init; }
  }
}
