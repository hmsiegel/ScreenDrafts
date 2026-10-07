namespace ScreenDrafts.Modules.Reporting.Features.Drafts.GetTitleHonorifics;

/// <summary>
/// The titles at one movie honorific level, in the order they joined it. Computed from the Record Book
/// facts on each request: a few thousand rows grouped in memory, so nothing is cached server-side.
/// </summary>
internal sealed class GetTitleHonorificsQueryHandler(IDbConnectionFactory connectionFactory)
  : IQueryHandler<GetTitleHonorificsQuery, GetTitleHonorificsResponse>
{
  private readonly IDbConnectionFactory _connectionFactory = connectionFactory;

  public async Task<Result<GetTitleHonorificsResponse>> Handle(
    GetTitleHonorificsQuery request,
    CancellationToken cancellationToken
  )
  {
    var validation = TitleHonorificValidator.Validate(request);

    if (validation.Spec is null)
    {
      return Result.Failure<GetTitleHonorificsResponse>(validation.Error!);
    }

    var spec = validation.Spec;

    await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);

    var rows = await TitleAppearanceLoader.LoadAsync(
      connection,
      request.IncludeAll,
      cancellationToken
    );

    var result = TitleHonorificEngine.Run(rows, spec);

    var totalPages = Math.Max(1, (int)Math.Ceiling(result.TotalMatching / (double)spec.PageSize));

    return Result.Success(
      new GetTitleHonorificsResponse
      {
        Level = spec.Level.Code,
        LevelLabel = spec.Level.Label,
        MinAppearances = spec.Level.MinAppearances,
        IncludesNonCanonical = request.IncludeAll,
        Page = spec.Page,
        PageSize = spec.PageSize,
        TotalPages = totalPages,
        TotalMatching = result.TotalMatching,
        Counts = result.Counts,
        Titles = result.Titles,
      }
    );
  }
}
