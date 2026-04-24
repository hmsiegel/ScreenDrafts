namespace ScreenDrafts.Modules.Drafts.IntegrationTests.Abstractions;

[Collection(nameof(DraftsIntegrationTestCollection))]
[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1051:Do not declare visible instance fields", Justification = "Reviewed")]
public abstract class DraftsIntegrationTest(DraftsIntegrationTestWebAppFactory factory) : BaseIntegrationTest<DraftsDbContext>(factory)
{
  protected override async Task ClearDatabaseAsync()
  {
    await DbContext.Database.ExecuteSqlRawAsync(
      $"""
      TRUNCATE TABLE
        drafts.outbox_messages,
        drafts.campaigns,
        drafts.categories,
        drafts.people,
        drafts.drafts,
        drafts.drafter_teams,
        drafts.drafter_team_drafter,
        drafts.drafters,
        drafts.hosts,
        drafts.draft_positions,
        drafts.draft_part_participants,
        drafts.commissioner_overrides,
        drafts.draft_categories,
        drafts.draft_channel_releases,
        drafts.draft_hosts,
        drafts.draft_parts,
        drafts.draft_releases,
        drafts.series,
        drafts.picks,
        drafts.game_boards,
        drafts.draft_board_items,
        drafts.draft_boards,
        drafts.draft_pool_items,
        drafts.draft_pools,
        drafts.candidate_list_entries,
        drafts.movies,
        drafts.trivia_results,
        drafts.vetoes,
        drafts.veto_overrides,
        drafts.prediction_entries,
        drafts.surrogate_assignments,
        drafts.prediction_results,
        drafts.prediction_standings,
        drafts.prediction_carryovers,
        drafts.draft_prediction_sets,
        drafts.draft_part_prediction_rules,
        drafts.prediction_contestants,
        drafts.prediction_seasons,
        drafts.draft_part_recordings
      RESTART IDENTITY CASCADE;
      """);
  }

  /// <summary>
  /// Manually processes pending outbox messages, dispatching any domain events
  /// whose handlers have not yet run (Quartz is disabled in tests).
  /// </summary>
  protected async Task ProcessOutboxAsync()
  {
    var connectionFactory = ServiceScope.ServiceProvider.GetRequiredService<IDbConnectionFactory>();
    var dispatcher = ServiceScope.ServiceProvider.GetRequiredService<IDraftsDomainEventDispatcher>();
    var scopeFactory = ServiceScope.ServiceProvider.GetRequiredService<IServiceScopeFactory>();

    await using var connection = await connectionFactory.OpenConnectionAsync(TestContext.Current.CancellationToken);
    await using var transaction = await connection.BeginTransactionAsync();

    var outboxMessages = (await connection.QueryAsync<OutboxRow>(
      """
      SELECT id AS Id, content AS Content
      FROM drafts.outbox_messages
      WHERE processed_on_utc IS NULL
      ORDER BY occurred_on_utc
      FOR UPDATE
      """,
      transaction: transaction))
      .ToList();

    foreach (var row in outboxMessages)
    {
      var domainEvent = JsonConvert.DeserializeObject<IDomainEvent>(
        row.Content,
        SerializerSettings.Instance)!;

      using var scope = scopeFactory.CreateScope();
      await dispatcher.DispatchAsync(domainEvent, scope.ServiceProvider);

      await connection.ExecuteAsync(
        "UPDATE drafts.outbox_messages SET processed_on_utc = @Now WHERE id = @Id",
        new { Now = DateTime.UtcNow, row.Id },
        transaction: transaction);
    }

    await transaction.CommitAsync();
  }

  private sealed record OutboxRow(Guid Id, string Content);

  protected async Task<Guid> GetFirstDraftPartIdAsync(string draftPublicId)
  {
    return await DbContext.Drafts
      .Where(d => d.PublicId == draftPublicId)
      .SelectMany(d => d.Parts)
      .OrderBy(p => p.PartIndex)
      .Select(p => p.Id.Value)
      .FirstAsync(TestContext.Current.CancellationToken);
  }

  protected async Task<string> GetFirstDraftPartPublicIdAsync(string draftPublicId)
  {
    return await DbContext.Drafts
      .Where(d => d.PublicId == draftPublicId)
      .SelectMany(d => d.Parts)
      .OrderBy(p => p.PartIndex)
      .Select(p => p.PublicId)
      .FirstAsync(TestContext.Current.CancellationToken);
  }

  protected async Task CreateMovieInDbAsync(int tmdbId)
  {
    var movie = Movie.Create(
      movieTitle: "Test Movie",
      publicId: $"m_{Guid.NewGuid():N}",
      mediaType: MediaType.Movie,
      id: Guid.NewGuid(),
      imdbId: $"tt{tmdbId:D7}",
      tmdbId: tmdbId).Value;
    DbContext.Add(movie);
    await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
  }

  protected async Task<string?> GetFirstParticipantPublicIdAsync(Guid draftPartId)
  {
    var participantIdValues = await DbContext.DraftPartParticipants
      .Where(p => p.DraftPartId == DraftPartId.Create(draftPartId)
               && p.ParticipantKindValue == ParticipantKind.Drafter)
      .Select(p => p.ParticipantIdValue)
      .ToListAsync(TestContext.Current.CancellationToken);

    return await DbContext.Drafters
      .AsAsyncEnumerable()
      .Where(d => participantIdValues.Contains(d.Id.Value))
      .Select(d => d.PublicId)
      .FirstAsync(TestContext.Current.CancellationToken);
  }
}
