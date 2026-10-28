namespace ScreenDrafts.Modules.Drafts.Infrastructure.PodcastLinks;

/// <summary>
/// Finds the Spotify and Apple Podcasts episode for each pending MainFeed
/// release link. One platform fetch per run covers every due link, however
/// many there are. Backfill runs through the same path: a migration inserts
/// pending rows for existing releases.
/// </summary>
[DisallowConcurrentExecution]
internal sealed partial class ResolvePodcastLinksJob(
  IServiceScopeFactory serviceScopeFactory,
  IOptions<PodcastLinksOptions> options,
  ILogger<ResolvePodcastLinksJob> logger
) : IJob
{
  private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;
  private readonly PodcastLinksOptions _options = options.Value;
  private readonly ILogger<ResolvePodcastLinksJob> _logger = logger;

  public async Task Execute(IJobExecutionContext context)
  {
    ArgumentNullException.ThrowIfNull(context);

    var cancellationToken = CancellationToken.None;

    await using var scope = _serviceScopeFactory.CreateAsyncScope();
    var services = scope.ServiceProvider;

    var dbContext = services.GetRequiredService<DraftsDbContext>();
    var integrationsApi = services.GetRequiredService<IIntegrationsApi>();
    var dbConnectionFactory = services.GetRequiredService<IDbConnectionFactory>();
    var nowUtc = services.GetRequiredService<IDateTimeProvider>().UtcNow;

    var dueLinks = await dbContext
      .DraftReleaseLinks.Include(l => l.Release)
      .Where(l => l.Status == ReleaseLinkStatus.Pending && l.NextCheckAtUtc <= nowUtc)
      .OrderBy(l => l.NextCheckAtUtc)
      .Take(_options.BatchSize)
      .ToListAsync(cancellationToken);

    if (dueLinks.Count == 0)
    {
      return;
    }

    LogRunStarted(_logger, dueLinks.Count, _options.DryRun);

    var titles = await GetDraftTitlesAsync(
      dbConnectionFactory,
      [.. dueLinks.Select(l => l.PartId.Value).Distinct()],
      cancellationToken
    );

    var matcher = new PodcastEpisodeMatcher(
      new PodcastMatchSettings(
        _options.ToleranceDays,
        _options.MinTitleScore,
        _options.MinScoreMargin
      )
    );

    foreach (var group in dueLinks.GroupBy(l => l.Platform))
    {
      await ResolvePlatformAsync(
        group.Key,
        [.. group],
        titles,
        matcher,
        integrationsApi,
        nowUtc,
        cancellationToken
      );
    }

    if (_options.DryRun)
    {
      return;
    }

    await dbContext.SaveChangesAsync(cancellationToken);
  }

  private async Task ResolvePlatformAsync(
    ReleaseLinkPlatform platform,
    List<DraftReleaseLink> links,
    Dictionary<Guid, string> titles,
    PodcastEpisodeMatcher matcher,
    IIntegrationsApi integrationsApi,
    DateTime nowUtc,
    CancellationToken cancellationToken
  )
  {
    var earliest = links.Min(l => l.Release.ReleaseDate).AddDays(-_options.ToleranceDays);

    var response = await integrationsApi.ListPodcastEpisodesAsync(
      ToPodcastPlatform(platform),
      earliest,
      cancellationToken
    );

    if (response.IsFailure)
    {
      // Leave the links due. The next run tries again.
      LogPlatformFailed(_logger, platform.Name, response.Errors[0].Code);
      return;
    }

    var episodes = response
      .Value.Results.Select(r => new EpisodeCandidate(r.Title, r.ReleaseDate, r.Url))
      .ToList();

    foreach (var link in links)
    {
      var releaseDate = link.Release.ReleaseDate;
      var title = titles.GetValueOrDefault(link.PartId.Value, string.Empty);

      var match = matcher.Match(new ReleaseMatchContext(releaseDate, title), episodes);

      Apply(link, match, nowUtc);
    }
  }

  private void Apply(DraftReleaseLink link, EpisodeMatchResult match, DateTime nowUtc)
  {
    var part = link.PartId.Value;
    var platform = link.Platform.Name;

    switch (match.Outcome)
    {
      case EpisodeMatchOutcome.Matched:
#pragma warning disable CA1873 // Avoid potentially expensive logging
        LogMatched(_logger, part, platform, match.Episode!.Url.ToString());
#pragma warning restore CA1873 // Avoid potentially expensive logging
        if (!_options.DryRun && link.MarkLinked(match.Episode.Url, nowUtc).IsFailure)
        {
          link.MarkNeedsReview(nowUtc);
        }

        break;

      case EpisodeMatchOutcome.Ambiguous:
        LogAmbiguous(_logger, part, platform);
        if (!_options.DryRun)
        {
          link.MarkNeedsReview(nowUtc);
        }

        break;

      default:
        var decision = PodcastLinkRetryPolicy.Next(link.Release.ReleaseDate, link.Attempts, nowUtc);

        LogNotFound(_logger, part, platform, decision.GiveUp);

        if (_options.DryRun)
        {
          break;
        }

        if (decision.GiveUp)
        {
          link.MarkNotFound(nowUtc);
        }
        else
        {
          link.ScheduleRetry(decision.NextCheckAtUtc!.Value, decision.CountsAsAttempt, nowUtc);
        }

        break;
    }
  }

  private static async Task<Dictionary<Guid, string>> GetDraftTitlesAsync(
    IDbConnectionFactory dbConnectionFactory,
    Guid[] partIds,
    CancellationToken cancellationToken
  )
  {
    await using var connection = await dbConnectionFactory.OpenConnectionAsync(cancellationToken);

    const string sql = """
      SELECT dp.id AS PartId, d.title AS Title
      FROM drafts.draft_parts dp
      JOIN drafts.drafts d ON d.id = dp.draft_id
      WHERE dp.id = ANY(@PartIds)
      """;

    var rows = await connection.QueryAsync<TitleRow>(sql, new { PartIds = partIds });

    return rows.ToDictionary(r => r.PartId, r => r.Title);
  }

  private static PodcastPlatform ToPodcastPlatform(ReleaseLinkPlatform platform) =>
    platform == ReleaseLinkPlatform.Spotify ? PodcastPlatform.Spotify : PodcastPlatform.Apple;

  private sealed record TitleRow(Guid PartId, string Title);

  [LoggerMessage(
    EventId = 1,
    Level = LogLevel.Information,
    Message = "Resolving {Count} podcast links. DryRun: {DryRun}."
  )]
  private static partial void LogRunStarted(ILogger logger, int count, bool dryRun);

  [LoggerMessage(
    EventId = 2,
    Level = LogLevel.Information,
    Message = "Part {PartId}: {Platform} matched {Url}."
  )]
  private static partial void LogMatched(ILogger logger, Guid partId, string platform, string url);

  [LoggerMessage(
    EventId = 3,
    Level = LogLevel.Warning,
    Message = "Part {PartId}: {Platform} has several plausible episodes. Needs review."
  )]
  private static partial void LogAmbiguous(ILogger logger, Guid partId, string platform);

  [LoggerMessage(
    EventId = 4,
    Level = LogLevel.Information,
    Message = "Part {PartId}: {Platform} found no episode. Giving up: {GiveUp}."
  )]
  private static partial void LogNotFound(
    ILogger logger,
    Guid partId,
    string platform,
    bool giveUp
  );

  [LoggerMessage(
    EventId = 5,
    Level = LogLevel.Warning,
    Message = "Episode lookup on {Platform} failed: {ErrorCode}. Retrying next run."
  )]
  private static partial void LogPlatformFailed(ILogger logger, string platform, string errorCode);
}
