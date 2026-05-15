namespace ScreenDrafts.Modules.Drafts.Features.Hosts.Create;

internal sealed class HostCreatedDomainEventHandler(
  IDbConnectionFactory dbConnectionFactory,
  IEventBus eventBus,
  IDateTimeProvider dateTimeProvider
) : DomainEventHandler<HostCreatedDomainEvent>
{
  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;
  private readonly IEventBus _eventBus = eventBus;
  private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;

  public override async Task Handle(
    HostCreatedDomainEvent domainEvent,
    CancellationToken cancellationToken = default
  )
  {
    await using var connection = await _dbConnectionFactory.OpenConnectionAsync(cancellationToken);

    const string sql = """
      SELECT
        h.public_id     AS HostPublicId,
        u.public_id     AS UserPublicId
      FROM drafts.hosts h
      JOIN drafts.people p ON p.id = h.person_id
      LEFT JOIN users.users u ON u.id = p.user_id
      WHERE h.id = @HostId
      """;

    var row = await connection.QuerySingleOrDefaultAsync<HostRow>(
      new CommandDefinition(sql, new { domainEvent.HostId }, cancellationToken: cancellationToken)
    );

    if (row is null || string.IsNullOrWhiteSpace(row.UserPublicId))
    {
      return;
    }

    await _eventBus.PublishAsync(
      new HostCreatedIntegrationEvent(
        id: Guid.NewGuid(),
        occurredOnUtc: _dateTimeProvider.UtcNow,
        hostId: domainEvent.HostId,
        userPublicId: row.UserPublicId,
        hostPublicId: row.HostPublicId
      ),
      cancellationToken
    );
  }

  private sealed record HostRow(string HostPublicId, string? UserPublicId);
}
