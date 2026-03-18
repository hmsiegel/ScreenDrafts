using System.Net.Http.Json;
using System.Text.Json.Serialization;

using MassTransit;

namespace ScreenDrafts.Seeding.Movies.Imdb;

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
      $"movie/{tmdbId}?append_to_response=credits,videos", cancellationToken);

    if (response is null)
    {
      return null;
    }

    var trailer = response.Videos.Results
      .Where(v => v.Site == "YouTube" && v.Type == "Trailer")
      .OrderByDescending(v => v.Official)
      .FirstOrDefault();

    var trailerUrl = trailer is not null
      ? new Uri($"https://www.youtube.com/watch?v={trailer.Key}")
      : null;


    var allPeople = response.Credits.Cast
      .Select(c => (c.Id, c.Name, IsCrew: false, c.KnownForDepartment, Job: (string?)null, Department: (string?)null))
      .Concat(response.Credits.Crew
        .Select(c => (c.Id, c.Name, IsCrew: true, KnownForDepartment: (string?)null, Job: (string?)c.Job, Department: (string?)c.Department)))
      .Distinct()
      .ToList();

    var uniquePersonIds = allPeople.Select(p => p.Id).Distinct().ToList();

    var imdbIdMap = new Dictionary<int, string?>();
    var batches = uniquePersonIds.Chunk(20);

    foreach ( var b in batches)
    {
      var tasks = b.Select(async id =>
      {
        var imdbId = await GetPersonImdbIdAsync(id, cancellationToken);
        return (TmdbPersonId: id, ImdbId: imdbId);
      });

      var results = await Task.WhenAll(tasks);
      foreach (var (TmdbPersonId, ImdbId) in results)
      {
        imdbIdMap[TmdbPersonId] = ImdbId;
      }

      await Task.Delay(500, cancellationToken);
    }

    return new TmdbMovieDetails
    {
      Id = response.Id,
      Title = response.Title,
      Overview = response.Overview ?? string.Empty,
      PosterPath = response.PosterPath,
      ReleaseDate = response.ReleaseDate,
      TrailerUrl = trailerUrl,
      Genres = response.Genres
        .Select(g => new TmdbGenre { Id = g.Id, Name = g.Name })
        .ToList()
        .AsReadOnly(),
      Credits = new TmdbCredits
      {
        Cast = response.Credits.Cast
          .Select(c => new TmdbCastMember
          {
            TmdbId = c.Id,
            Name = c.Name,
            KnownForDepartment = c.KnownForDepartment,
            ImdbId = imdbIdMap.GetValueOrDefault(c.Id)
          })
          .ToList()
          .AsReadOnly(),
        Crew = response.Credits.Crew
          .Select(c => new TmdbCrewMember
          {
            TmdbId = c.Id,
            Name = c.Name,
            Job = c.Job,
            Department = c.Department,
            ImdbId = imdbIdMap.GetValueOrDefault(c.Id)
          })
          .ToList()
          .AsReadOnly()
      }
    };
  }

  public async Task<string?> GetPersonImdbIdAsync(int tmdbPersonId, CancellationToken cancellationToken = default)
  {
    var response = await _httpClient.GetFromJsonAsync<TmdbPersonExternalIdsResponse>(
      $"person/{tmdbPersonId}/external_ids", cancellationToken);
    return response?.ImdbId;
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
      [property: JsonPropertyName("genres")] IReadOnlyList<TmdbGenreApiResponse> Genres,
      [property: JsonPropertyName("videos")] TmdbVideosApiResponse Videos,
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

  private sealed record TmdbPersonExternalIdsResponse(
      [property: JsonPropertyName("imdb_id")] string? ImdbId);

  private sealed record TmdbGenreApiResponse(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("name")] string Name);

  private sealed record TmdbVideosApiResponse(
    [property: JsonPropertyName("results")] IReadOnlyList<TmdbVideoItem> Results);
  private sealed record TmdbVideoItem(
    [property: JsonPropertyName("key")] string Key,
    [property: JsonPropertyName("site")] string Site,
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("official")] bool Official);
}
