namespace ScreenDrafts.Modules.Reporting.Features.Drafts.GetRecordBook;

internal sealed class GetRecordBookQueryHandler(
  IDbConnectionFactory connectionFactory,
  ICacheService cacheService,
  IDateTimeProvider dateTimeProvider
) : IQueryHandler<GetRecordBookQuery, GetRecordBookResponse>
{
  private static readonly TimeSpan _cacheDuration = TimeSpan.FromHours(1);

  private readonly IDbConnectionFactory _connectionFactory = connectionFactory;
  private readonly ICacheService _cacheService = cacheService;
  private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;

  public async Task<Result<GetRecordBookResponse>> Handle(
    GetRecordBookQuery request,
    CancellationToken cancellationToken
  )
  {
    var cacheKey = request.IncludeAll
      ? ReportingCacheKeys.RecordBookAllCacheKey
      : ReportingCacheKeys.RecordBookCanonicalCacheKey;

    var cached = await _cacheService.GetAsync<GetRecordBookResponse>(cacheKey, cancellationToken);

    if (cached is not null)
    {
      return Result.Success(cached);
    }

    await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);

    var data = await RecordBookDataLoader.LoadAsync(
      connection,
      request.IncludeAll,
      cancellationToken
    );

    var response = RecordBookBuilder.Build(data, request.IncludeAll, _dateTimeProvider.UtcNow);

    await _cacheService.SetAsync(
      key: cacheKey,
      value: response,
      expiration: _cacheDuration,
      cancellationToken: cancellationToken
    );

    return Result.Success(response);
  }
}
