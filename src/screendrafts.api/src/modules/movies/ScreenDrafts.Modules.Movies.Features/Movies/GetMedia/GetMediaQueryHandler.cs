using Serilog;

namespace ScreenDrafts.Modules.Movies.Features.Movies.GetMedia;

internal sealed class GetMediaQueryHandler(
  IDbConnectionFactory dbConnectionFactory,
  IDraftsApi draftsApi,
  IReportingApi reportingApi
) : IQueryHandler<GetMediaQuery, MediaResponse>
{
  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;
  private readonly IDraftsApi _draftsApi = draftsApi;
  private readonly IReportingApi _reportingApi = reportingApi;

  public async Task<Result<MediaResponse>> Handle(
    GetMediaQuery request,
    CancellationToken cancellationToken
  )
  {
    await using var connection = await _dbConnectionFactory.OpenConnectionAsync(cancellationToken);

    const string sql = $"""
      select
      	m.id                                                AS {nameof(MediaResponse.Id)},
        m.public_id                                         AS {nameof(MediaResponse.PublicId)},
      	m.imdb_id                                           AS {nameof(MediaResponse.ImdbId)},
        m.tmdb_id                                           AS {nameof(MediaResponse.TmdbId)},
        m.igdb_id                                           AS {nameof(MediaResponse.IgdbId)},
      	m.title                                             AS {nameof(MediaResponse.Title)},
      	m.year                                              AS {nameof(MediaResponse.Year)},
      	m.plot                                              AS {nameof(MediaResponse.Plot)},
      	m.image                                             AS {nameof(MediaResponse.Image)},
      	m.release_date                                      AS {nameof(MediaResponse.ReleaseDate)},
      	m.youtube_trailer_url                               AS {nameof(
        MediaResponse.YouTubeTrailer
      )},
        m.media_type                                        AS {nameof(MediaResponse.MediaType)},
        m.tv_series_tmdb_id                                 AS {nameof(
        MediaResponse.TvSeriesTmdbId
      )},
        m.season_number                                     AS {nameof(MediaResponse.SeasonNumber)},
        m.episode_number                                    AS {nameof(
        MediaResponse.EpisodeNumber
      )},
        g.id                                                AS {nameof(GenreResponse.Id)},
        g.Name                                              AS {nameof(GenreResponse.Name)},
        a.id                                                AS {nameof(ActorResponse.Id)},
        a.imdb_id                                           AS {nameof(ActorResponse.ImdbId)},
        a.name                                              AS {nameof(ActorResponse.Name)},
        d.id                                                AS {nameof(DirectorResponse.Id)},
        d.imdb_id                                           AS {nameof(DirectorResponse.ImdbId)},
        d.name                                              AS {nameof(DirectorResponse.Name)},
        w.id                                                AS {nameof(WriterResponse.Id)},
        w.imdb_id                                           AS {nameof(WriterResponse.ImdbId)},
        w.name                                              AS {nameof(WriterResponse.Name)},
        p.id                                                AS {nameof(ProducerResponse.Id)},
        p.imdb_id                                           AS {nameof(ProducerResponse.ImdbId)},
        p.name                                              AS {nameof(ProducerResponse.Name)},
        pc.id                                               AS {nameof(
        ProductionCompanyResponse.Id
      )},
        pc.imdb_id                                            AS {nameof(
        ProductionCompanyResponse.ImdbId
      )},
        pc.name                                              AS {nameof(
        ProductionCompanyResponse.Name
      )}
      from movies.media m
      left join movies.media_genres mg on mg.media_id = m.id
      left join movies.genres g on g.id = mg.genre_id
      left join movies.media_actors ma on ma.media_id = m.id
      left join movies.people a on a.id = ma.actor_id
      left join movies.media_directors md on md.media_id = m.id
      left join movies.people d on d.id = md.director_id 
      left join movies.media_writers mw on mw.media_id = m.id
      left join movies.people w on w.id = mw.writer_id
      left join movies.media_producers mp on mp.media_id = m.id
      left join movies.people p on p.id = mp.producer_id
      left join movies.media_production_companies mpc on mpc.media_id = m.id
      left join movies.production_companies pc on pc.id = mpc.production_company_id
      where m.public_id = @PublicId
      group by m.id, g.id, a.id, d.id, w.id, p.id, pc.id;
      """;

    var mediaDictionary = new Dictionary<Guid, MediaResponse>();

    var seenGenres = new HashSet<Guid>();
    var seenActors = new HashSet<Guid>();
    var seenDirectors = new HashSet<Guid>();
    var seenWriters = new HashSet<Guid>();
    var seenProducers = new HashSet<Guid>();
    var seenProductionCompanies = new HashSet<Guid>();

    await connection.QueryAsync<
      MediaResponse,
      GenreResponse,
      ActorResponse,
      DirectorResponse,
      WriterResponse,
      ProducerResponse,
      ProductionCompanyResponse,
      MediaResponse
    >(
      sql,
      (media, genre, actor, director, writer, producer, productionCompany) =>
      {
        if (media is null || media.Id == Guid.Empty)
        {
          return null!;
        }

        if (!mediaDictionary.TryGetValue(media.Id, out var mediaEntry))
        {
          mediaEntry = media;

          mediaDictionary[media.Id] = mediaEntry;
        }

        if (genre is not null && genre.Id != Guid.Empty && seenGenres.Add(genre.Id))
          mediaEntry.AddGenre(genre);

        if (
          actor is not null
          && actor.Id != Guid.Empty
          && !string.IsNullOrEmpty(actor.ImdbId)
          && seenActors.Add(actor.Id)
        )
          mediaEntry.AddActor(actor);

        if (
          director is not null
          && director.Id != Guid.Empty
          && !string.IsNullOrEmpty(director.ImdbId)
          && seenDirectors.Add(director.Id)
        )
          mediaEntry.AddDirector(director);

        if (
          writer is not null
          && writer.Id != Guid.Empty
          && !string.IsNullOrEmpty(writer.ImdbId)
          && seenWriters.Add(writer.Id)
        )
          mediaEntry.AddWriter(writer);

        if (
          producer is not null
          && producer.Id != Guid.Empty
          && !string.IsNullOrEmpty(producer.ImdbId)
          && seenProducers.Add(producer.Id)
        )
          mediaEntry.AddProducer(producer);

        if (
          productionCompany is not null
          && productionCompany.Id != Guid.Empty
          && !string.IsNullOrEmpty(productionCompany.ImdbId)
          && seenProductionCompanies.Add(productionCompany.Id)
        )
          mediaEntry.AddProductionCompany(productionCompany);
        return mediaEntry!;
      },
      new { request.PublicId },
      splitOn: "Id,Id,Id,Id,Id,Id"
    );

    var result = mediaDictionary.Values.FirstOrDefault();

    if (result is null)
    {
      return Result.Failure<MediaResponse>(MediaErrors.MediaNotFound(request.PublicId));
    }

    var (appearances, honorific) = await FetchCrossModuleDataAsync(
      request.PublicId,
      request.IncludePatreon,
      cancellationToken
    );

    foreach (var appearance in appearances)
    {
      result.AddMediaAppearance(
        new MediaAppearanceResponse
        {
          DraftPublicId = appearance.DraftPublicId,
          DraftTitle = appearance.DraftTitle,
          EpisodeNumber = appearance.EpisodeNumber,
          PickedByDisplayName = appearance.PickedByDisplayName,
          PickedByPersonPublicId = appearance.PickedByPersonPublicId,
          Position = appearance.Position,
          WasVetoed = appearance.WasVetoed,
          WasVetoOverridden = appearance.WasVetoOverridden,
          WasCommissionerOverride = appearance.WasCommissionerOverride,
          VetoedByDisplayName = appearance.VetoedByDisplayName,
          VetoOverrideByDisplayName = appearance.VetoOverrideByDisplayName,
          IsPatreon = appearance.IsPatreon,
        }
      );
    }

    if (honorific is not null)
    {
      result.SetHonorific(
        new MediaHonorificResponse
        {
          AppearanceHonorificName = honorific.AppearanceHonorificName,
          AppearanceHonorificValue = honorific.AppearanceHonorificValue,
          PositionHonorificValue = honorific.PositionHonorificValue,
          AppearanceCount = honorific.AppearanceCount,
          IsUnifiedNo1 = honorific.IsUnifiedNo1,
          IsTheCycle = honorific.IsTheCycle,
        }
      );
    }

    result.ComputeStats();

    return Result.Success(result);
  }

  private async Task<(
    IReadOnlyList<MediaAppearanceRecord> Appearances,
    MediaHonorificRecord? Honorific
  )> FetchCrossModuleDataAsync(string mediaPublicId, bool includePatreon, CancellationToken ct)
  {
    IReadOnlyList<MediaAppearanceRecord> appearances = [];
    MediaHonorificRecord? honorific = null;

    try
    {
      appearances = await _draftsApi.GetMediaAppearancesAsync(mediaPublicId, includePatreon, ct);
    }
    catch (InvalidOperationException ex)
    {
      Log.Warning(ex, "GetMediaAppearancesAsync failed for {MediaPublicId}", mediaPublicId);
    }

    try
    {
      honorific = await _reportingApi.GetMediaHonorificAsync(mediaPublicId, ct);
    }
    catch (InvalidOperationException ex)
    {
      Log.Warning(ex, "GetMediaHonorificAsync failed for {MediaPublicId}", mediaPublicId);
    }

    return (appearances, honorific);
  }
}
