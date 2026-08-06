using System.Text;

namespace ScreenDrafts.Modules.Integrations.Infrastructure.Services.Igdb;

internal sealed class IgdbService(HttpClient httpClient, IOptions<IgdbSettings> settings)
  : IIgdbService
{
  private readonly HttpClient _httpClient = httpClient;
  private readonly IgdbSettings _settings = settings.Value;

  private string? _accessToken;

  public async Task<IgdbGameDetails?> GetGameDetailsAsync(
    int igdbId,
    CancellationToken cancellationToken = default
  )
  {
    await EnsureAccessTokenAsync(cancellationToken);

    var body =
      $"fields name, summary, cover.url, first_release_date, genres.name; where id = {igdbId};";

    using var request = new HttpRequestMessage(HttpMethod.Post, _settings.Endpoint)
    {
      Content = new StringContent(body, Encoding.UTF8, "text/plain"),
      Headers =
      {
        { "Client-ID", _settings.ClientId },
        { "Authorization", $"Bearer {_accessToken}" },
      },
    };

    var response = await _httpClient.SendAsync(request, cancellationToken);
    response.EnsureSuccessStatusCode();

    var results = await response.Content.ReadFromJsonAsync<IReadOnlyList<IgdbGameApiResponse>>(
      cancellationToken: cancellationToken
    );

    var game = results is not null && results.Count > 0 ? results[0] : null;

    if (game is null)
    {
      return null;
    }

    string? coverUrl = null;
    if (game.Cover?.Url is not null)
    {
      coverUrl = game.Cover.Url.StartsWith("//", StringComparison.OrdinalIgnoreCase)
        ? $"https:{game.Cover.Url}"
        : game.Cover.Url;
    }

    coverUrl = coverUrl?.Replace("t_thumb", "t_cover_big", StringComparison.OrdinalIgnoreCase);

    return new IgdbGameDetails
    {
      Id = game.Id,
      Name = game.Name,
      Summary = game.Summary,
      CoverUrl = coverUrl is not null ? new Uri(coverUrl) : null,
      FirstReleaseDate = game.FirstReleaseDate,
      Genres = game.Genres?.Select(g => g.Name).ToList() ?? [],
    };
  }

  /// <summary>
  /// IGDB has no separate search endpoint — same /games endpoint as
  /// GetGameDetailsAsync, an Apicalypse "search" clause instead of "where
  /// id =". Query is quote-escaped since Apicalypse search values are
  /// double-quoted strings and an unescaped quote in the query would break
  /// the request body.
  /// </summary>
  public async Task<IReadOnlyList<IgdbGameDetails>> SearchGamesAsync(
    string query,
    int page = 1,
    CancellationToken cancellationToken = default
  )
  {
    await EnsureAccessTokenAsync(cancellationToken);

    const int pageSize = 10;
    var offset = (Math.Max(page, 1) - 1) * pageSize;
    var escapedQuery = query.Replace("\"", "\\\"", StringComparison.Ordinal);

    var body = $"""
      search "{escapedQuery}";
      fields name, cover.url, first_release_date;
      limit {pageSize};
      offset {offset};
      """;

    using var request = new HttpRequestMessage(HttpMethod.Post, _settings.Endpoint)
    {
      Content = new StringContent(body, Encoding.UTF8, "text/plain"),
      Headers =
      {
        { "Client-ID", _settings.ClientId },
        { "Authorization", $"Bearer {_accessToken}" },
      },
    };

    var response = await _httpClient.SendAsync(request, cancellationToken);

    if (!response.IsSuccessStatusCode)
    {
      return [];
    }

    var results = await response.Content.ReadFromJsonAsync<IReadOnlyList<IgdbGameApiResponse>>(
      cancellationToken: cancellationToken
    );

    if (results is null)
    {
      return [];
    }

    return
    [
      .. results.Select(g => new IgdbGameDetails
      {
        Id = g.Id,
        Name = g.Name,
        CoverUrl = BuildCoverUri(g.Cover?.Url),
        FirstReleaseDate = g.FirstReleaseDate,
      }),
    ];
  }

  // Private helper methods

  private static Uri? BuildCoverUri(string? rawCoverUrl)
  {
    if (rawCoverUrl is null)
    {
      return null;
    }

    var coverUrl = rawCoverUrl.StartsWith("//", StringComparison.OrdinalIgnoreCase)
      ? $"https:{rawCoverUrl}"
      : rawCoverUrl;

    coverUrl = coverUrl.Replace("t_thumb", "t_cover_big", StringComparison.OrdinalIgnoreCase);

    return new Uri(coverUrl);
  }

  private async Task EnsureAccessTokenAsync(CancellationToken cancellationToken)
  {
    if (_accessToken is not null)
    {
      return;
    }

    var tokenUrl =
      $"{_settings.TwitchTokenUrl}"
      + $"?client_id={_settings.ClientId}"
      + $"&client_secret={_settings.ClientSecret}"
      + $"&grant_type=client_credentials";

    using var tokenRequest = new HttpRequestMessage(HttpMethod.Post, tokenUrl);
    var tokenResponse = await _httpClient.SendAsync(tokenRequest, cancellationToken);
    tokenResponse.EnsureSuccessStatusCode();

    var tokenData = await tokenResponse.Content.ReadFromJsonAsync<TwitchTokenResponse>(
      cancellationToken: cancellationToken
    );

    _accessToken =
      tokenData?.AccessToken
      ?? throw new InvalidOperationException("Failed to retrieve access token from Twitch API.");
  }

  // Private API response models
  private sealed record IgdbGameApiResponse(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("summary")] string? Summary,
    [property: JsonPropertyName("cover")] IgdbCoverApiResponse? Cover,
    [property: JsonPropertyName("first_release_date")] long? FirstReleaseDate,
    [property: JsonPropertyName("genres")] IReadOnlyList<IgdbGenreApiResponse>? Genres
  );

  private sealed record IgdbCoverApiResponse([property: JsonPropertyName("url")] string Url);

  private sealed record IgdbGenreApiResponse([property: JsonPropertyName("name")] string Name);

  private sealed record TwitchTokenResponse(
    [property: JsonPropertyName("access_token")] string AccessToken
  );
}
