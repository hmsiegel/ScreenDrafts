namespace ScreenDrafts.Modules.Reporting.Features.Drafts.GetStatsQueryOptions;

/// <summary>
/// The choices behind the custom query picker: which metrics exist, which group-bys each allows,
/// and the series and draft types a filter can name. Series and draft types follow the caller's scope,
/// so a Patreon-only series name is never listed to someone who cannot query it.
/// </summary>
internal sealed class GetStatsQueryOptionsQueryHandler(IDbConnectionFactory connectionFactory)
  : IQueryHandler<GetStatsQueryOptionsQuery, StatsQueryOptionsResponse>
{
  private readonly IDbConnectionFactory _connectionFactory = connectionFactory;

  public async Task<Result<StatsQueryOptionsResponse>> Handle(
    GetStatsQueryOptionsQuery request,
    CancellationToken cancellationToken
  )
  {
    await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);

    const string seriesSql =
      RecordBookDataLoader.ScopedPicksCte
      + """

        SELECT DISTINCT series_name FROM scoped_picks ORDER BY series_name
        """;

    const string draftTypesSql =
      RecordBookDataLoader.ScopedPicksCte
      + """

        SELECT DISTINCT draft_type FROM scoped_picks ORDER BY draft_type
        """;

    const string episodesSql =
      RecordBookDataLoader.ScopedPicksCte
      + """

        SELECT MIN(ds.episode_number) AS MinEpisode, MAX(ds.episode_number) AS MaxEpisode
        FROM reporting.draft_summaries ds
        WHERE ds.episode_number IS NOT NULL
          AND ds.draft_id IN (SELECT draft_id FROM scoped_picks)
        """;

    var parameters = new { request.IncludeAll };

    var series = (
      await connection.QueryAsync<string>(
        new CommandDefinition(seriesSql, parameters, cancellationToken: cancellationToken)
      )
    ).ToList();

    var draftTypes = (
      await connection.QueryAsync<string>(
        new CommandDefinition(draftTypesSql, parameters, cancellationToken: cancellationToken)
      )
    ).ToList();

    var episodes = await connection.QuerySingleAsync<EpisodeRangeRow>(
      new CommandDefinition(episodesSql, parameters, cancellationToken: cancellationToken)
    );

    return Result.Success(
      new StatsQueryOptionsResponse
      {
        Metrics =
        [
          .. StatsMetrics.All.Select(m => new StatsMetricOption(
            m.Code,
            m.Label,
            m.Format,
            m.Description,
            m.GroupBys
          )),
        ],
        GroupBys = [.. StatsGroupBys.All.Select(g => new StatsGroupByOption(g.Code, g.Label))],
        Series = series,
        DraftTypes = draftTypes,
        MinEpisode = episodes.MinEpisode,
        MaxEpisode = episodes.MaxEpisode,
      }
    );
  }

  private sealed record EpisodeRangeRow(int? MinEpisode, int? MaxEpisode);
}

internal sealed class Endpoint : ScreenDraftsEndpointWithoutRequest<StatsQueryOptionsResponse>
{
  public override void Configure()
  {
    Get(DraftReportingRoutes.StatsQueryOptions);
    Description(x =>
    {
      x.WithTags(ReportingOpenApi.Tags.Stats)
        .WithName(ReportingOpenApi.Names.Stats_GetQueryOptions)
        .Produces<StatsQueryOptionsResponse>(StatusCodes.Status200OK);
    });

    // Signed-in users only, like POST /stats/query.
  }

  public override async Task HandleAsync(CancellationToken ct)
  {
    var isPatreonMember = User.HasPermission(ReportingAuth.Permissions.StatsReadPatreon);

    var result = await Sender.Send(
      new GetStatsQueryOptionsQuery { IncludeAll = isPatreonMember },
      ct
    );

    await this.SendOkAsync(result, ct);
  }
}
