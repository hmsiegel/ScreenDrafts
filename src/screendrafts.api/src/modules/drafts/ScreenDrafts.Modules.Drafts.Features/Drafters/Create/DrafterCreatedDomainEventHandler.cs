namespace ScreenDrafts.Modules.Drafts.Features.Drafters.Create;

internal sealed class DrafterCreatedDomainEventHandler(
  IDbConnectionFactory dbConnectionFactory,
  IEventBus eventBus,
  IDateTimeProvider dateTimeProvider
) : DomainEventHandler<DrafterCreatedDomainEvent>
{
  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;
  private readonly IEventBus _eventBus = eventBus;
  private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;

  public override async Task Handle(
    DrafterCreatedDomainEvent domainEvent,
    CancellationToken cancellationToken = default
  )
  {
    await using var connection = await _dbConnectionFactory.OpenConnectionAsync(cancellationToken);

    const string sql = """
      SELECT
        d.public_id     AS DrafterPublicId,
        u.public_id     AS UserPublicId
      FROM drafts.drafters d
      JOIN drafts.people p ON p.id = d.person_id
      LEFT JOIN users.users u ON u.id = p.user_id
      WHERE d.id = @DrafterId
      """;

    var row = await connection.QuerySingleOrDefaultAsync<DrafterRow>(
      new CommandDefinition(
        sql,
        new { domainEvent.DrafterId },
        cancellationToken: cancellationToken
      )
    );

    if (row is null || string.IsNullOrWhiteSpace(row.UserPublicId))
    {
      return;
    }

    await _eventBus.PublishAsync(
      new DrafterCreatedIntegrationEvent(
        id: Guid.NewGuid(),
        occurredOnUtc: _dateTimeProvider.UtcNow,
        drafterId: domainEvent.DrafterId,
        userPublicId: row.UserPublicId
      ),
      cancellationToken
    );
  }

  private sealed record DrafterRow(string DrafterPublicId, string? UserPublicId);
}
