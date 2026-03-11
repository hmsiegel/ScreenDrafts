namespace ScreenDrafts.Modules.Integrations.Infrastructure.Imdb;

internal sealed class TmdbService(
  HttpClient httpClient,
  IOptions<TmdbSettings> settings)
  : ITmdbService
{
  private readonly HttpClient _httpClient = httpClient;
  private readonly TmdbSettings _settings = settings.Value;

  public Uri? BuildPosterUrl(string? posterPath, string size = "w500")
  {
    if (string.IsNullOrWhiteSpace(posterPath))
    {
      return null;
    }

    return new Uri($"{_settings.BaseImageAddress}{size}{posterPath}");
  }

  public Uri? BuildTrailerUrl(string? trailerPath, string size = "w500")
  {
    if (string.IsNullOrWhiteSpace(trailerPath))
    {
      return new Uri(_settings.TrailerPlaceholder);
    }

    return new Uri($"{_settings.BaseImageAddress}{size}{trailerPath}");
  }

  public async Task<TmdbMovieSearchResult?> FindMovieByImdbIdAsync(string imdbId, CancellationToken cancellationToken = default)
  {
    var response = await _httpClient.GetFromJsonAsync<TmdbFindApiResponse>(
      $"find/{imdbId}?external_source=imdb_id", cancellationToken);

    var movie = response?.MovieResults.Count > 0 ? response.MovieResults[0] : null;
    if (movie is null)
    {
      return null;
    }

    return new TmdbMovieSearchResult
    {
      Id = movie.Id,
      Title = movie.Title,
      Overview = movie.Overview ?? string.Empty,
      ReleaseDate = movie.ReleaseDate,
      PosterPath = movie.PosterPath
    };
  }

  public async Task<string?> GetImdbIdAsync(int tmdbId, CancellationToken cancellationToken = default)
  {
    var response = await _httpClient.GetFromJsonAsync<TmdbExternalIdsApiResponse>(
      $"movie/{tmdbId}/external_ids", cancellationToken);

    return response?.ImdbId;
  }

  public async Task<TmdbMovieDetails?> GetMovieDetailsAsync(int tmdbId, CancellationToken cancellationToken = default)
  {
    var response = await _httpClient.GetFromJsonAsync<TmdbMovieDetailApiResponse>(
      $"movie/{tmdbId}?append_to_response=credits", cancellationToken);

    if (response is null)
    {
      return null;
    }

    return new TmdbMovieDetails
    {
      Id = response.Id,
      Title = response.Title,
      Overview = response.Overview ?? string.Empty,
      PosterPath = response.PosterPath,
      ReleaseDate = response.ReleaseDate,
      Credits = new TmdbCredits
      {
        Cast = response.Credits.Cast
          .Select(c => new TmdbCastMember
          {
            Name = c.Name,
            KnownForDepartment = c.KnownForDepartment
          })
          .ToList()
          .AsReadOnly(),
        Crew = response.Credits.Crew
          .Select(c => new TmdbCrewMember
          {
            Name = c.Name,
            Job = c.Job,
            Department = c.Department
          })
          .ToList()
          .AsReadOnly()
      }
    };
  }

  public async Task<IReadOnlyList<TmdbMovieSearchResult>> SearchMoviesAsync(string query, CancellationToken cancellationToken = default)
  {
    var response = await _httpClient.GetFromJsonAsync<TmdbSearchResponse>(
      $"search/movie?query={Uri.EscapeDataString(query)}&include_adult=true", cancellationToken);

    return response?.Results
      .Select(r => new TmdbMovieSearchResult
      {
        Id = r.Id,
        Title = r.Title,
        Overview = r.Overview!,
        ReleaseDate = r.ReleaseDate,
        PosterPath = r.PosterPath
      })
      .ToList()
      .AsReadOnly()
      ?? (IReadOnlyList<TmdbMovieSearchResult>)[];
  }

  private sealed record TmdbSearchResponse(
    [property: JsonPropertyName("results")] IReadOnlyList<TmdbSearchItem> Results);

  private sealed record TmdbSearchItem(
        [property: JsonPropertyName("id")] int Id,
        [property: JsonPropertyName("title")] string Title,
        [property: JsonPropertyName("overview")] string? Overview,
        [property: JsonPropertyName("poster_path")] string? PosterPath,
        [property: JsonPropertyName("release_date")] string? ReleaseDate);

  private sealed record TmdbFindApiResponse(
      [property: JsonPropertyName("movie_results")] IReadOnlyList<TmdbSearchItem> MovieResults);

  private sealed record TmdbMovieDetailApiResponse(
      [property: JsonPropertyName("id")] int Id,
      [property: JsonPropertyName("title")] string Title,
      [property: JsonPropertyName("overview")] string? Overview,
      [property: JsonPropertyName("poster_path")] string? PosterPath,
      [property: JsonPropertyName("release_date")] string? ReleaseDate,
      [property: JsonPropertyName("credits")] TmdbCreditsApiResponse Credits);

  private sealed record TmdbCreditsApiResponse(
      [property: JsonPropertyName("cast")] IReadOnlyList<TmdbCastApiResponse> Cast,
      [property: JsonPropertyName("crew")] IReadOnlyList<TmdbCrewApiResponse> Crew);

  private sealed record TmdbCastApiResponse(
      [property: JsonPropertyName("id")] int Id,
      [property: JsonPropertyName("name")] string Name,
      [property: JsonPropertyName("known_for_department")] string? KnownForDepartment);

  private sealed record TmdbCrewApiResponse(
      [property: JsonPropertyName("id")] int Id,
      [property: JsonPropertyName("name")] string Name,
      [property: JsonPropertyName("job")] string Job,
      [property: JsonPropertyName("department")] string Department);

  private sealed record TmdbExternalIdsApiResponse(
      [property: JsonPropertyName("imdb_id")] string? ImdbId);
}
