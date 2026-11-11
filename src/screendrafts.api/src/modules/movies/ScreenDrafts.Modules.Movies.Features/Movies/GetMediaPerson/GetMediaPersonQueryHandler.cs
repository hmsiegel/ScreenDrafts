using ScreenDrafts.Common.Application.Caching;

namespace ScreenDrafts.Modules.Movies.Features.Movies.GetMediaPerson;

internal sealed partial class GetMediaPersonQueryHandler(
  IDbConnectionFactory dbConnectionFactory,
  IIntegrationsApi integrationsApi,
  ICacheService cacheService,
  ILogger<GetMediaPersonQueryHandler> logger
) : IQueryHandler<GetMediaPersonQuery, MediaPersonProfileResponse>
{
  private static readonly TimeSpan PhotoTtl = TimeSpan.FromDays(7);
  private static readonly TimeSpan NoPhotoTtl = TimeSpan.FromDays(1);
  private static readonly string[] RoleOrder = ["actor", "director", "writer", "producer"];

  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;
  private readonly IIntegrationsApi _integrationsApi = integrationsApi;
  private readonly ICacheService _cacheService = cacheService;
  private readonly ILogger<GetMediaPersonQueryHandler> _logger = logger;

  // A person can exist as several rows (an IMDb-first import and a TMDb import of the same
  // human). Matching on either id, and on any id the match shares with another row, folds
  // those rows into one profile until the duplicates are merged.
  private const string PeopleByTmdbSql = """
    SELECT p.id AS Id, p.name AS Name, p.tmdb_id AS TmdbId, p.imdb_id AS ImdbId
    FROM movies.people p
    WHERE p.tmdb_id = @TmdbId
       OR p.imdb_id IN (SELECT i.imdb_id
                        FROM movies.people i
                        WHERE i.tmdb_id = @TmdbId AND i.imdb_id IS NOT NULL AND i.imdb_id <> '')
    """;

  private const string PeopleByImdbSql = """
    SELECT p.id AS Id, p.name AS Name, p.tmdb_id AS TmdbId, p.imdb_id AS ImdbId
    FROM movies.people p
    WHERE p.imdb_id = @ImdbId
       OR (p.tmdb_id > 0 AND p.tmdb_id IN (SELECT i.tmdb_id
                                           FROM movies.people i
                                           WHERE i.imdb_id = @ImdbId AND i.tmdb_id > 0))
    """;

  private const string CreditsSql = """
    SELECT 'actor' AS Role, m.public_id AS MediaPublicId, m.title AS Title, m.year AS Year,
           m.image AS PosterUrl, m.media_type AS MediaType, c.billing_order AS BillingOrder
    FROM movies.media_actors c
    JOIN movies.media m ON m.id = c.media_id
    WHERE c.actor_id = ANY(@PersonIds)
    UNION ALL
    SELECT 'director', m.public_id, m.title, m.year, m.image, m.media_type, NULL::int
    FROM movies.media_directors c
    JOIN movies.media m ON m.id = c.media_id
    WHERE c.director_id = ANY(@PersonIds)
    UNION ALL
    SELECT 'writer', m.public_id, m.title, m.year, m.image, m.media_type, NULL::int
    FROM movies.media_writers c
    JOIN movies.media m ON m.id = c.media_id
    WHERE c.writer_id = ANY(@PersonIds)
    UNION ALL
    SELECT 'producer', m.public_id, m.title, m.year, m.image, m.media_type, NULL::int
    FROM movies.media_producers c
    JOIN movies.media m ON m.id = c.media_id
    WHERE c.producer_id = ANY(@PersonIds)
    """;

  public async Task<Result<MediaPersonProfileResponse>> Handle(
    GetMediaPersonQuery request,
    CancellationToken cancellationToken
  )
  {
    if (!TryParseId(request.Id, out var tmdbId, out var imdbId))
    {
      return Result.Failure<MediaPersonProfileResponse>(MediaPersonErrors.NotFound(request.Id));
    }

    await using var connection = await _dbConnectionFactory.OpenConnectionAsync(cancellationToken);

    var people =
      tmdbId > 0
        ? (
          await connection.QueryAsync<PersonRow>(PeopleByTmdbSql, new { TmdbId = tmdbId })
        ).ToList()
        : (
          await connection.QueryAsync<PersonRow>(PeopleByImdbSql, new { ImdbId = imdbId })
        ).ToList();

    if (people.Count == 0)
    {
      return Result.Failure<MediaPersonProfileResponse>(MediaPersonErrors.NotFound(request.Id));
    }

    var personIds = people.Select(p => p.Id).ToArray();

    var credits = (
      await connection.QueryAsync<CreditRow>(CreditsSql, new { PersonIds = personIds })
    ).ToList();

    // Someone with no credits at all is not "in the Vault"; the filter and the media pages
    // can never lead here, so treat a direct hit the same as an unknown id.
    if (credits.Count == 0)
    {
      return Result.Failure<MediaPersonProfileResponse>(MediaPersonErrors.NotFound(request.Id));
    }

    var primary = people
      .OrderByDescending(p => p.TmdbId > 0)
      .ThenBy(p => p.Name, StringComparer.Ordinal)
      .First();
    var resolvedTmdbId = people.Max(p => p.TmdbId);
    var resolvedImdbId = !string.IsNullOrWhiteSpace(primary.ImdbId)
      ? primary.ImdbId
      : people.Select(p => p.ImdbId).FirstOrDefault(i => !string.IsNullOrWhiteSpace(i));

    var groups = credits
      .GroupBy(c => c.Role)
      .Select(g => new MediaPersonCreditGroupResponse
      {
        Role = g.Key,
        Credits =
        [
          .. g.DistinctBy(c => c.MediaPublicId)
            .OrderByDescending(c => c.Year ?? string.Empty, StringComparer.Ordinal)
            .ThenBy(c => c.Title, StringComparer.OrdinalIgnoreCase)
            .Select(c => new MediaPersonCreditResponse
            {
              MediaPublicId = c.MediaPublicId,
              Title = c.Title,
              Year = c.Year,
              PosterUrl = c.PosterUrl,
              MediaType = c.MediaType,
              BillingOrder = c.BillingOrder,
            }),
        ],
      })
      .OrderByDescending(g => g.Credits.Count)
      .ThenBy(g => Array.IndexOf(RoleOrder, g.Role))
      .ToList();

    var photoUrl = await GetPhotoUrlAsync(resolvedTmdbId, resolvedImdbId, cancellationToken);

    return Result.Success(
      new MediaPersonProfileResponse
      {
        Id =
          resolvedTmdbId > 0
            ? resolvedTmdbId.ToString(CultureInfo.InvariantCulture)
            : resolvedImdbId ?? request.Id,
        Name = primary.Name,
        TmdbId = resolvedTmdbId,
        ImdbId = resolvedImdbId,
        PhotoUrl = photoUrl,
        TitleCount = credits.Select(c => c.MediaPublicId).Distinct().Count(),
        Groups = groups,
      }
    );
  }

  /// <summary>
  /// Photo from TMDb, cached in Redis. A TMDb failure costs the photo, not the page.
  /// </summary>
  private async Task<string?> GetPhotoUrlAsync(
    int tmdbId,
    string? imdbId,
    CancellationToken cancellationToken
  )
  {
    if (tmdbId <= 0 && string.IsNullOrWhiteSpace(imdbId))
    {
      return null;
    }

    var key = tmdbId > 0 ? $"media-person-photo:t:{tmdbId}" : $"media-person-photo:i:{imdbId}";

    var cached = await _cacheService.GetAsync<CachedPersonPhoto>(key, cancellationToken);
    if (cached is not null)
    {
      return cached.Url;
    }

    try
    {
      var profile = await _integrationsApi.GetPersonProfileAsync(tmdbId, imdbId, cancellationToken);

      if (profile.IsFailure)
      {
        return null;
      }

      var url = profile.Value.PhotoPath;
      await _cacheService.SetAsync(
        key,
        new CachedPersonPhoto(url),
        url is null ? NoPhotoTtl : PhotoTtl,
        cancellationToken
      );
      return url;
    }
    catch (Exception ex) when (ex is not OperationCanceledException)
    {
      LogPhotoLookupFailed(_logger, key, ex);
      return null;
    }
  }

  private static bool TryParseId(string? value, out int tmdbId, out string? imdbId)
  {
    tmdbId = 0;
    imdbId = null;

    if (string.IsNullOrWhiteSpace(value))
    {
      return false;
    }

    var id = value.Trim();

    if (
      id.All(char.IsAsciiDigit)
      && int.TryParse(id, NumberStyles.None, CultureInfo.InvariantCulture, out tmdbId)
      && tmdbId > 0
    )
    {
      return true;
    }

    tmdbId = 0;

    if (
      id.Length > 2
      && id.StartsWith("nm", StringComparison.OrdinalIgnoreCase)
      && id[2..].All(char.IsAsciiDigit)
    )
    {
      imdbId = id.ToLowerInvariant();
      return true;
    }

    return false;
  }

  [LoggerMessage(
    EventId = 1101,
    Level = LogLevel.Warning,
    Message = "GetMediaPerson: photo lookup failed for {CacheKey}; returning the profile without a photo."
  )]
  private static partial void LogPhotoLookupFailed(
    ILogger<GetMediaPersonQueryHandler> logger,
    string cacheKey,
    Exception exception
  );

  private sealed record PersonRow(Guid Id, string Name, int TmdbId, string? ImdbId);

  private sealed record CreditRow(
    string Role,
    string MediaPublicId,
    string Title,
    string? Year,
    string? PosterUrl,
    MediaType MediaType,
    int? BillingOrder
  );
}

internal sealed record CachedPersonPhoto(string? Url);
