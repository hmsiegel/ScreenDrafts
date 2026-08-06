using ScreenDrafts.Modules.Integrations.Features.People.GetPersonFilmography;

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
}
