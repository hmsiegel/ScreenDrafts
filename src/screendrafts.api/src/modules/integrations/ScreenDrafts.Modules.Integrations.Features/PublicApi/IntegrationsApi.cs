using ScreenDrafts.Modules.Integrations.Features.People.GetPersonProfile;

namespace ScreenDrafts.Modules.Integrations.Features.PublicApi;

internal class IntegrationsApi(ISender sender) : IIntegrationsApi
{
  private readonly ISender _sender = sender;

  public async Task<Result<SearchMediaApiResponse>> SearchMoviesAsync(
    string query,
    int page = 1,
    CancellationToken cancellationToken = default
  )
  {
    var result = await _sender.Send(
      new SearchForMovieCommand { Query = query, Page = page },
      cancellationToken
    );

    if (result.IsFailure)
    {
      return Result.Failure<SearchMediaApiResponse>(result.Errors);
    }

    var mapped = result
      .Value.Results.Select(r => new MediaSearchApiResult
      {
        TmdbId = r.TmdbId,
        Title = r.Title,
        Year = r.Year,
        Overview = r.Overview,
        Poster = r.PosterUrl,
        MediaType = r.MediaType,
      })
      .ToList()
      .AsReadOnly();

    return Result.Success(
      new SearchMediaApiResponse
      {
        Results = mapped,
        TotalCount = result.Value.TotalResults,
        TotalPages = result.Value.TotalPages,
        Page = result.Value.Page,
      }
    );
  }

  public async Task<Result<SearchGamesApiResponse>> SearchGamesAsync(
    string query,
    int page = 1,
    CancellationToken cancellationToken = default
  )
  {
    var result = await _sender.Send(
      new SearchGamesCommand { Query = query, Page = page },
      cancellationToken
    );

    if (result.IsFailure)
    {
      return Result.Failure<SearchGamesApiResponse>(result.Errors);
    }

    var mapped = result
      .Value.Results.Select(g => new GameSearchApiResult
      {
        IgdbId = g.IgdbId,
        Title = g.Title,
        Year = g.Year,
        Poster = g.PosterUrl,
      })
      .ToList()
      .AsReadOnly();

    return Result.Success(new SearchGamesApiResponse { Results = mapped });
  }

  public async Task<Result<SearchYouTubeApiResponse>> SearchYouTubeAsync(
    string query,
    int page = 1,
    CancellationToken cancellationToken = default
  )
  {
    var result = await _sender.Send(
      new SearchYouTubeCommand { Query = query, Page = page },
      cancellationToken
    );

    if (result.IsFailure)
    {
      return Result.Failure<SearchYouTubeApiResponse>(result.Errors);
    }

    var mapped = result
      .Value.Results.Select(v => new YouTubeSearchApiResult
      {
        VideoId = v.VideoId,
        Title = v.Title,
        ChannelTitle = v.ChannelTitle,
        ThumbnailUrl = v.ThumbnailUrl != null ? new Uri(v.ThumbnailUrl) : null,
      })
      .ToList()
      .AsReadOnly();

    return Result.Success(new SearchYouTubeApiResponse { Results = mapped });
  }

  public async Task<Result<PersonFilmographyApiResponse>> GetPersonFilmographyAsync(
    string imdbId,
    CancellationToken cancellationToken = default
  )
  {
    var result = await _sender.Send(
      new GetPersonFilmographyCommand { ImdbId = imdbId },
      cancellationToken
    );

    if (result.IsFailure)
    {
      return Result.Failure<PersonFilmographyApiResponse>(result.Errors);
    }

    var mapped = result
      .Value.Credits.Select(c => new PersonFilmographyCreditApiResult
      {
        TmdbId = c.TmdbId,
        Title = c.Title,
        Year = c.Year,
        PosterPath = c.PosterUrl,
        MediaType = c.MediaType,
        CreditRole = c.CreditRole,
      })
      .ToList()
      .AsReadOnly();

    return Result.Success(
      new PersonFilmographyApiResponse
      {
        PersonName = result.Value.PersonName,
        PersonPhotoPath = result.Value.PersonPhotoUrl,
        Credits = mapped,
      }
    );
  }

  public async Task<Result<PersonProfileApiResponse>> GetPersonProfileAsync(
    int tmdbId,
    string? imdbId,
    CancellationToken cancellationToken = default
  )
  {
    var result = await _sender.Send(
      new GetPersonProfileCommand { TmdbId = tmdbId, ImdbId = imdbId },
      cancellationToken
    );

    if (result.IsFailure)
    {
      return Result.Failure<PersonProfileApiResponse>(result.Errors);
    }

    return Result.Success(
      new PersonProfileApiResponse
      {
        TmdbId = result.Value.TmdbId,
        Name = result.Value.Name,
        PhotoPath = result.Value.PhotoUrl,
      }
    );
  }

  public async Task<Result<OnlineMediaCreditsApiResponse>> GetOnlineMediaCreditsAsync(
    OnlineMediaCreditsApiRequest request,
    CancellationToken cancellationToken = default
  )
  {
    ArgumentNullException.ThrowIfNull(request);

    var result = await _sender.Send(
      new GetOnlineMediaCommand
      {
        MediaType = MediaType.FromValue(request.MediaType),
        TmdbId = request.TmdbId,
        ImdbId = request.ImdbId,
        TvSeriesTmdbId = request.TvSeriesTmdbId,
        SeasonNumber = request.SeasonNumber,
        EpisodeNumber = request.EpisodeNumber,
      },
      cancellationToken
    );

    if (result.IsFailure)
    {
      return Result.Failure<OnlineMediaCreditsApiResponse>(result.Errors);
    }

    var media = result.Value;

    return Result.Success(
      new OnlineMediaCreditsApiResponse
      {
        Directors = [.. media.Directors.Select(d => new OnlineCreditApiResult { Name = d.Name, ImdbId = d.ImdbId, TmdbId = d.TmdbId })],
        Actors = [.. media.Actors.Select(a => new OnlineCreditApiResult { Name = a.Name, ImdbId = a.ImdbId, TmdbId = a.TmdbId })],
        Writers = [.. media.Writers.Select(w => new OnlineCreditApiResult { Name = w.Name, ImdbId = w.ImdbId, TmdbId = w.TmdbId })],
        Producers = [.. media.Producers.Select(p => new OnlineCreditApiResult { Name = p.Name, ImdbId = p.ImdbId, TmdbId = p.TmdbId })],
        ProductionCompanies = [.. media.ProductionCompanies.Select(c => new OnlineCreditApiResult { Name = c.Name, ImdbId = c.ImdbId, TmdbId = c.TmdbId })],
      }
    );
  }

  public async Task<Result<ListPodcastEpisodesApiResponse>> ListPodcastEpisodesAsync(
    PodcastPlatform platform,
    DateOnly releasedOnOrAfter,
    CancellationToken cancellationToken = default
  )
  {
    var result = await _sender.Send(
      new ListPodcastEpisodesCommand { Platform = platform, ReleasedOnOrAfter = releasedOnOrAfter },
      cancellationToken
    );

    if (result.IsFailure)
    {
      return Result.Failure<ListPodcastEpisodesApiResponse>(result.Errors);
    }

    var mapped = result
      .Value.Episodes.Select(e => new PodcastEpisodeApiResult
      {
        Title = e.Title,
        ReleaseDate = e.ReleaseDate,
        Url = e.Url,
      })
      .ToList()
      .AsReadOnly();

    return Result.Success(new ListPodcastEpisodesApiResponse { Results = mapped });
  }
}
