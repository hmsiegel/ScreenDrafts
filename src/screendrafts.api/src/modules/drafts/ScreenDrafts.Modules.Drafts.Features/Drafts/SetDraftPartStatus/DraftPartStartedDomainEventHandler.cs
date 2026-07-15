namespace ScreenDrafts.Modules.Drafts.Features.Drafts.SetDraftPartStatus;

internal sealed class DraftPartStartedDomainEventHandler(
  IDbConnectionFactory connectionFactory,
  IEventBus eventBus,
  IDateTimeProvider dateTimeProvider
) : DomainEventHandler<DraftPartStartedDomainEvent>
{
  private readonly IDbConnectionFactory _connectionFactory = connectionFactory;
  private readonly IEventBus _eventBus = eventBus;
  private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;

  public override async Task Handle(
    DraftPartStartedDomainEvent domainEvent,
    CancellationToken cancellationToken = default
  )
  {
    await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);

    const string metadataSql = """
      SELECT
        dp.public_id AS DraftPartPublicId,
        s.canonical_policy AS CanonicalPolicyValue,
        EXISTS (
          SELECT 1
          FROM drafts.draft_releases dr
          WHERE dr.part_id = dp.id
            AND dr.release_channel = 0
        ) AS HasMainFeedRelease
      FROM drafts.draft_parts dp
      JOIN drafts.drafts d ON d.id = dp.draft_id
      JOIN drafts.series s ON s.id = d.series_id
      WHERE dp.id = @DraftPartId AND d.id = @DraftId
      """;

    var metadata = await connection.QuerySingleOrDefaultAsync<MetadataRow>(
      new CommandDefinition(
        metadataSql,
        new { domainEvent.DraftPartId, domainEvent.DraftId },
        cancellationToken: cancellationToken
      )
    );

    if (metadata is null)
    {
      // Log and exit if the part is not found — mirrors
      // DraftPartCompletedDomainEventHandler's guard, avoiding exceptions
      // in event handlers.
      return;
    }

    const string participantsSql = """
      SELECT
        p.participant_id_value AS ParticipantIdValue,
        p.participant_kind_value AS ParticipantKindValue
      FROM drafts.draft_part_participants p
      WHERE p.draft_part_id = @DraftPartId
        AND p.participant_kind_value != 2
      """;

    var participantRows = (
      await connection.QueryAsync<ParticipantRow>(
        new CommandDefinition(
          participantsSql,
          new { domainEvent.DraftPartId },
          cancellationToken: cancellationToken
        )
      )
    ).ToList();

    var participants = participantRows
      .Select(p => new DraftPartParticipantModel
      {
        ParticipantIdValue = p.ParticipantIdValue,
        ParticipantKindValue = p.ParticipantKindValue,
      })
      .ToList();

    await _eventBus.PublishAsync(
      new DraftPartStartedIntegrationEvent(
        id: Guid.NewGuid(),
        occurredOnUtc: _dateTimeProvider.UtcNow,
        draftPartId: domainEvent.DraftPartId,
        draftPartPublicId: metadata.DraftPartPublicId,
        draftId: domainEvent.DraftId,
        draftPublicId: domainEvent.DraftPublicId,
        partIndex: domainEvent.Index,
        participants: participants,
        canonicalPolicyValue: metadata.CanonicalPolicyValue,
        hasMainFeedRelease: metadata.HasMainFeedRelease
      ),
      cancellationToken
    );
  }

  private sealed record MetadataRow(
    string DraftPartPublicId,
    int CanonicalPolicyValue,
    bool HasMainFeedRelease
  );

  private sealed record ParticipantRow(Guid ParticipantIdValue, int ParticipantKindValue);
}
