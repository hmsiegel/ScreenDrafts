namespace ScreenDrafts.Modules.Integrations.Infrastructure.Services.Tmdb;

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

  // Movies
  public async Task<TmdbSearchResult?> FindMovieByImdbIdAsync(string imdbId, CancellationToken cancellationToken = default)
  {
    var response = await _httpClient.GetFromJsonAsync<TmdbFindApiResponse>(
      $"find/{imdbId}?external_source=imdb_id", cancellationToken);

    var movie = response?.MovieResults.Count > 0 ? response.MovieResults[0] : null;
    if (movie is null)
    {
      return null;
    }

    return new TmdbSearchResult
    {
      Id = movie.Id,
      Title = movie.Title,
      Overview = movie.Overview ?? string.Empty,
      ReleaseDate = movie.ReleaseDate,
      PosterPath = movie.PosterPath
    };
  }

  public async Task<string?> GetMovieImdbIdAsync(int tmdbId, CancellationToken cancellationToken = default)
  {
    var response = await _httpClient.GetFromJsonAsync<TmdbExternalIdsApiResponse>(
      $"movie/{tmdbId}/external_ids", cancellationToken);

    return response?.ImdbId;
  }

  public async Task<TmdbMediaDetails?> GetMovieDetailsAsync(int tmdbId, CancellationToken cancellationToken = default)
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

    var credits = await BuildCreditsAsync(response.Credits, cancellationToken);

    return new TmdbMediaDetails
    {
      Id = response.Id,
      Title = response.Title,
      Overview = response.Overview ?? string.Empty,
      PosterPath = response.PosterPath,
      ReleaseDate = response.ReleaseDate,
      TrailerUrl = trailerUrl,
      Genres = response.Genres
        .Select(g => new TmdbGenre
        {
          Id = g.Id,
          Name = g.Name
        })
        .ToList()
        .AsReadOnly(),
      Credits = credits
    };
  }


  public async Task<IReadOnlyList<TmdbSearchResult>> SearchMoviesAsync(string query, CancellationToken cancellationToken = default)
  {
    var response = await _httpClient.GetFromJsonAsync<TmdbSearchResponse>(
      $"search/movie?query={Uri.EscapeDataString(query)}&include_adult=true", cancellationToken);

    return response?.Results
      .Select(r => new TmdbSearchResult
      {
        Id = r.Id,
        Title = r.Title,
        Overview = r.Overview!,
        ReleaseDate = r.ReleaseDate,
        PosterPath = r.PosterPath
      })
      .ToList()
      .AsReadOnly()
      ?? (IReadOnlyList<TmdbSearchResult>)[];
  }

  // TV Shows
  public async Task<TmdbMediaDetails?> GetTvShowDetailsAsync(
    int tmdbId,
    CancellationToken cancellationToken = default)
  {

    var response = await _httpClient.GetFromJsonAsync<TmdbTvDetailsApiResponse>(
      $"tv/{tmdbId}?append_to_response=credits,videos", cancellationToken);

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

    var credits = response.Credits is not null
      ? await BuildCreditsAsync(response.Credits, cancellationToken)
      : new TmdbCredits();

    return new TmdbMediaDetails
    {
      Id = response.Id,
      Title = response.Name,
      Overview = response.Overview ?? string.Empty,
      PosterPath = response.PosterPath,
      ReleaseDate = response.FirstAirDate,
      TrailerUrl = trailerUrl,
      Genres = response.Genres
        .Select(g => new TmdbGenre
        {
          Id = g.Id,
          Name = g.Name
        })
        .ToList()
        .AsReadOnly(),
      Credits = credits
    };
  }
  public async Task<string?> GetTvShowImdbIdAsync(int tmdbId, CancellationToken cancellationToken = default)
  {
    var response = await _httpClient.GetFromJsonAsync<TmdbExternalIdsApiResponse>(
      $"tv/{tmdbId}/external_ids", cancellationToken);

    return response?.ImdbId;
  }

  // TV Episodes
  public async Task<TmdbMediaDetails?> GetTvEpisodeDetailsAsync(
    int seriesTmdbId,
    int seasonNumber,
    int episodeNumber,
    CancellationToken cancellationToken = default)
  {
    var response = await _httpClient.GetFromJsonAsync<TmdbEpisodeDetailApiResponse>(
      $"tv/{seriesTmdbId}/season/{seasonNumber}/episode/{episodeNumber}?append_to_response=credits", cancellationToken);

    if (response is null)
    {
      return null;
    }

    // Episode credis use guest star and crew at the episode level.
    // Map guest_stars to cast and crew to crew for consistency with movie and TV show credits.
    var guestStarIds = response.Credits?.GuestStars.Select(g => g.Id).Distinct().ToList() ?? [];
    var crewIds = response.Credits?.Crew.Select(c => c.Id).Distinct().ToList() ?? [];
    var allIds = guestStarIds.Concat(crewIds).Distinct().ToList();

    var imdbIdMap = await FetchPersonImdbIdsAsync(allIds, cancellationToken);

    var cast = (response.Credits?.GuestStars ?? [])
      .Select(g => new TmdbCastMember
      {
        TmdbId = g.Id,
        Name = g.Name,
        KnownForDepartment = g.KnownForDepartment,
        ImdbId = imdbIdMap.TryGetValue(g.Id, out var imdbId) ? imdbId : null,
      })
      .ToList();

    var crew = (response.Credits?.Crew ?? [])
      .Select(c => new TmdbCrewMember
      {
        TmdbId = c.Id,
        Name = c.Name,
        Job = c.Job,
        Department = c.Department,
        ImdbId = imdbIdMap.TryGetValue(c.Id, out var imdbId) ? imdbId : null
      })
      .ToList();

    return new TmdbMediaDetails
    {
      Id = response.Id,
      Title = response.Name,
      Overview = response.Overview ?? string.Empty,
      PosterPath = response.StillPath,
      ReleaseDate = response.AirDate,
      TrailerUrl = null, // TMDb does not provide trailers at the episode level
      Genres = [],
      Credits = new TmdbCredits { Cast = cast.AsReadOnly(), Crew = crew.AsReadOnly() },
      TVSeriesTmdbId = seriesTmdbId,
      SeasonNumber = seasonNumber,
      EpisodeNumber = episodeNumber
    };
  }

  //  People
  public async Task<string?> GetPersonImdbIdAsync(int tmdbPersonId, CancellationToken cancellationToken = default)
  {
    var response = await _httpClient.GetFromJsonAsync<TmdbPersonExternalIdsResponse>(
      $"person/{tmdbPersonId}/external_ids", cancellationToken);
    return response?.ImdbId;
  }

  // Private Helpers
  private async Task<TmdbCredits> BuildCreditsAsync(TmdbCreditsApiResponse raw, CancellationToken cancellationToken)
  {
    var allIds = raw.Cast.Select(c => c.Id)
      .Concat(raw.Crew.Select(c => c.Id))
      .Distinct()
      .ToList();

    var imdbIdMap = await FetchPersonImdbIdsAsync(allIds, cancellationToken);

    return new TmdbCredits
    {
      Cast = raw.Cast
        .Select(c => new TmdbCastMember
        {
          TmdbId = c.Id,
          Name = c.Name,
          KnownForDepartment = c.KnownForDepartment,
          ImdbId = imdbIdMap.TryGetValue(c.Id, out var imdbId) ? imdbId : null
        })
        .ToList()
        .AsReadOnly(),
      Crew = raw.Crew
        .Select(c => new TmdbCrewMember
        {
          TmdbId = c.Id,
          Name = c.Name,
          Job = c.Job,
          Department = c.Department,
          ImdbId = imdbIdMap.TryGetValue(c.Id, out var imdbId) ? imdbId : null
        })
        .ToList()
        .AsReadOnly()
    };
  }

  private async Task<Dictionary<int, string>> FetchPersonImdbIdsAsync(IReadOnlyList<int> personIds, CancellationToken cancellationToken)
  {
    var result = new Dictionary<int, string>();

    foreach (var batch in personIds.Chunk(20))
    {
      var tasks = batch.Select(async id =>
      {
        var imdbId = await GetPersonImdbIdAsync(id, cancellationToken);
        return (Id: id, ImdbId: imdbId);
      });

      var results = await Task.WhenAll(tasks);
      foreach (var (id, imdbId) in results)
      {
        if (imdbId is not null)
        {
          result[id] = imdbId;
        }
      }

      await Task.Delay(500, cancellationToken); // TMDb rate limits to 40 requests per 10 seconds, so delay between batches
    }

    return result;
  }

  public async Task<TmdbFindResult?> FindByImdbIdAsync(string imdbId, CancellationToken cancellationToken = default)
  {
    var response = await _httpClient.GetFromJsonAsync<TmdbFindAllApiResponse>(
      $"find/{imdbId}?external_source=imdb_id", cancellationToken);

    if (response is null)
    {
      return new TmdbFindResult();
    }

    // Movie
    var movie = response.MovieResults.Count > 0 ? response.MovieResults[0] : null;
    TmdbSearchResult? movieResult = movie is not null
      ? new TmdbSearchResult
      {
        Id = movie.Id,
        Title = movie.Title,
        Overview = movie.Overview ?? string.Empty,
        ReleaseDate = movie.ReleaseDate,
        PosterPath = movie.PosterPath
      }
      : null;

    // TV Show
    var tv = response.TvResults.Count > 0 ? response.TvResults[0] : null;
    TmdbTvResult? tvResult = tv is not null
      ? new TmdbTvResult
      {
        Id = tv.Id,
        Name = tv.Name,
        Overview = tv.Overview,
        FirstAirDate = tv.FirstAirDate,
        PosterPath = tv.PosterPath
      }
      : null;

    // TV Episode
    var episode = response.TvEpisodeResults.Count > 0 ? response.TvEpisodeResults[0] : null;
    TmdbTvEpisodeResult? episodeResult = episode is not null
      ? new TmdbTvEpisodeResult
      {
        Id = episode.Id,
        Name = episode.Name,
        Overview = episode.Overview,
        AirDate = episode.AirDate,
        StillPath = episode.StillPath,
        ShowId = episode.ShowId,
        SeasonNumber = episode.SeasonNumber,
        EpisodeNumber = episode.EpisodeNumber
      }
      : null;

    return new TmdbFindResult
    {
      MovieResults = movieResult is not null ? new List<TmdbSearchResult> { movieResult }.AsReadOnly() : new List<TmdbSearchResult>().AsReadOnly(),
      TvResults = tvResult is not null ? new List<TmdbTvResult> { tvResult }.AsReadOnly() : new List<TmdbTvResult>().AsReadOnly(),
      TvEpisodeResults = episodeResult is not null ? new List<TmdbTvEpisodeResult> { episodeResult }.AsReadOnly() : new List<TmdbTvEpisodeResult>().AsReadOnly()
    };
  }

  // API Response Models

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

  private sealed record TmdbTvDetailsApiResponse(
      [property: JsonPropertyName("id")] int Id,
      [property: JsonPropertyName("name")] string Name,
      [property: JsonPropertyName("overview")] string? Overview,
      [property: JsonPropertyName("poster_path")] string? PosterPath,
      [property: JsonPropertyName("first_air_date")] string? FirstAirDate,
      [property: JsonPropertyName("genres")] IReadOnlyList<TmdbGenreApiResponse> Genres,
      [property: JsonPropertyName("videos")] TmdbVideosApiResponse Videos,
      [property: JsonPropertyName("credits")] TmdbCreditsApiResponse Credits);

  private sealed record TmdbEpisodeDetailApiResponse(
      [property: JsonPropertyName("id")] int Id,
      [property: JsonPropertyName("name")] string Name,
      [property: JsonPropertyName("overview")] string? Overview,
      [property: JsonPropertyName("still_path")] string? StillPath,
      [property: JsonPropertyName("air_date")] string? AirDate,
      [property: JsonPropertyName("season_number")] int SeasonNumber,
      [property: JsonPropertyName("episode_number")] int EpisodeNumber,
      [property: JsonPropertyName("credits")] TmdbEpisodeCreditsApiResponse? Credits);

  private sealed record TmdbEpisodeCreditsApiResponse(
      [property: JsonPropertyName("cast")] IReadOnlyList<TmdbCastApiResponse> Cast,
      [property: JsonPropertyName("guest_stars")] IReadOnlyList<TmdbGuestStarApiResponse> GuestStars,
      [property: JsonPropertyName("crew")] IReadOnlyList<TmdbCrewApiResponse> Crew);

  private sealed record TmdbGuestStarApiResponse(
      [property: JsonPropertyName("id")] int Id,
      [property: JsonPropertyName("name")] string Name,
      [property: JsonPropertyName("known_for_department")] string? KnownForDepartment);

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

  private sealed record TmdbFindAllApiResponse(
  [property: JsonPropertyName("movie_results")] IReadOnlyList<TmdbSearchItem> MovieResults,
  [property: JsonPropertyName("tv_results")] IReadOnlyList<TmdbTvApiResponse> TvResults,
  [property: JsonPropertyName("tv_episode_results")] IReadOnlyList<TmdbTvEpisodeApiResponse> TvEpisodeResults);

  private sealed record TmdbTvApiResponse(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("first_air_date")] string? FirstAirDate,
    [property: JsonPropertyName("poster_path")] string? PosterPath,
    [property: JsonPropertyName("overview")] string? Overview);

  private sealed record TmdbTvEpisodeApiResponse(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("air_date")] string? AirDate,
    [property: JsonPropertyName("still_path")] string? StillPath,
    [property: JsonPropertyName("overview")] string? Overview,
    [property: JsonPropertyName("show_id")] int ShowId,
    [property: JsonPropertyName("season_number")] int SeasonNumber,
    [property: JsonPropertyName("episode_number")] int EpisodeNumber);
}


