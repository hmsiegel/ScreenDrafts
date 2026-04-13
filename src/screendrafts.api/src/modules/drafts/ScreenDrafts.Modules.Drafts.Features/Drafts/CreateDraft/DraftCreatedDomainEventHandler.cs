namespace ScreenDrafts.Modules.Drafts.Features.Drafts.CreateDraft;

internal sealed class DraftCreatedDomainEventHandler(
  IDbConnectionFactory connectionFactory,
  IEventBus eventBus,
  IDateTimeProvider dateTimeProvider)
  : DomainEventHandler<DraftCreatedDomainEvent>
{
  private readonly IDbConnectionFactory _connectionFactory = connectionFactory;
  private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;
  private readonly IEventBus _eventBus = eventBus;

  public override async Task Handle(
    DraftCreatedDomainEvent domainEvent,
    CancellationToken cancellationToken = default)
  {
    await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);

    const string sql =
      """
      SELECT 
        d.title As Title,
        EXISTS (
          SELECT 1
          FROM drafts.draft_channel_releases dcr
          WHERE dcr.draft_id = d.id
            AND dcr.release_channel = 1
        ) AS IsPatreon
      FROM drafts.drafts d
      WHERE d.id = @DraftId
      """;

    var draftRow = await connection.QuerySingleOrDefaultAsync<DraftRow>(
      new CommandDefinition(
        commandText: sql,
        parameters: new { domainEvent.DraftId },
        cancellationToken: cancellationToken));

    if (draftRow is null)
    {
      return;
    }

    await _eventBus.PublishAsync(
      new DraftCreatedIntegrationEvent(
        id: Guid.NewGuid(),
        occurredOnUtc: _dateTimeProvider.UtcNow,
        draftId: domainEvent.DraftId,
        draftTitle: draftRow.Title,
        isPatreon: draftRow.IsPatreon),
      cancellationToken);
  }

  private sealed record DraftRow(string Title, bool IsPatreon);
}
