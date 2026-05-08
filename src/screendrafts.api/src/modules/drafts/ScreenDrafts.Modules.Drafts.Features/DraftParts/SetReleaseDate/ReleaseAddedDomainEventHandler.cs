namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.SetReleaseDate;

internal sealed class ReleaseAddedDomainEventHandler(
  IDbConnectionFactory connectionFactory,
  IEventBus eventBus,
  IDateTimeProvider dateTimeProvider
) : DomainEventHandler<ReleaseAddedDomainEvent>
{
  private readonly IDbConnectionFactory _connectionFactory = connectionFactory;
  private readonly IEventBus _eventBus = eventBus;
  private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;

  public override async Task Handle(
    ReleaseAddedDomainEvent domainEvent,
    CancellationToken cancellationToken = default
  )
  {
    await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);

    const string sql = """
      SELECT 
       d.id                     AS DraftId,
       d.public_id              AS DraftPublicId,
       dp.public_id             AS DraftPartPublicId,
       dr.release_channel       AS ReleaseChannel,
       dr.release_date          AS ReleaseDate,
       (
          SELECT dcr.episode_number
          FROM drafts.draft_channel_releases dcr
          WHERE dcr.draft_id = d.id
            AND dcr.release_channel = 0
       )                        AS EpisodeNumber,
       FROM drafts.draft_releases dr
       JOIN drafts.draft_parts dp ON dp.id = dr.part_id
       JOIN drafts.drafts d ON d.id = dp.draft_id
       WHERE dr.part_id = @PartId
       ORDER BY dr.created_on_utc DESC
       LIMIT 1
      """;

    var row = await connection.QuerySingleOrDefaultAsync<ReleaseRow>(
      new CommandDefinition(
        commandText: sql,
        parameters: new { domainEvent.PartId },
        cancellationToken: cancellationToken
      )
    );

    if (row is null)
    {
      return;
    }

    var channelName = row.ReleaseChannel == 0 ? "MainFeed" : "Patreon";

    await _eventBus.PublishAsync(
      new DraftPartReleaseAddedIntegrationEvent(
        id: Guid.NewGuid(),
        occurredOnUtc: _dateTimeProvider.UtcNow,
        draftId: row.DraftId,
        draftPublicId: row.DraftPublicId,
        draftPartPublicId: row.DraftPartPublicId,
        releaseChannel: channelName,
        releaseDate: row.ReleaseDate,
        episodeNumber: row.EpisodeNumber
      ),
      cancellationToken
    );
  }

  private sealed record ReleaseRow(
    Guid DraftId,
    string DraftPublicId,
    string DraftPartPublicId,
    int ReleaseChannel,
    DateOnly ReleaseDate,
    int? EpisodeNumber
  );
}
