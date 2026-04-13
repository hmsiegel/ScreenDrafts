namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Hosts;

internal sealed class HostAddedDomainEventHandler(
  IDbConnectionFactory dbConnectionFactory,
  IEventBus eventBus,
  IDateTimeProvider dateTimeProvider)
  : DomainEventHandler<HostAddedDomainEvent>
{
  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;
  private readonly IEventBus _eventBus = eventBus;
  private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;

  public override async Task Handle(HostAddedDomainEvent domainEvent, CancellationToken cancellationToken = default)
  {
    await using var connection = await _dbConnectionFactory.OpenConnectionAsync(cancellationToken);

    const string hostQuery =
      """
      SELECT 
        p.user_id AS UserId,
        p.full_name AS FullName
      FROM 
        drafts.hosts h
      JOIN 
        drafts.people p ON p.id = h.person_id
      WHERE 
        h.id = @HostId
      """;

    var host = await connection.QuerySingleAsync<HostPersonRow>(
      new CommandDefinition(
        hostQuery,
        new { domainEvent.HostId },
        cancellationToken: cancellationToken));

    if (host is null || host.UserId is null)
    {
      return;
    }

    const string contextQuery =
      """
      SELECT 
        d.name AS DraftName
      FROM 
        drafts.draft_parts dp
      JOIN
        drafts.drafts d ON d.id = dp.draft_id
      WHERE 
        dp.id = @DraftPartId
      """;

    var context = await connection.QuerySingleAsync<DraftPartContextRow>(
      new CommandDefinition(
        contextQuery,
        new { domainEvent.DraftPartId },
        cancellationToken: cancellationToken));

    if (context is null)
    {
      return;
    }

    const string coHostsQuery =
      """
      SELECT 
        p.full_name AS FullName
      FROM 
        drafts.draft_hosts dh
      JOIN drafts.hosts h on h.id = dh.host_id
      JOIN drafts.people p ON p.id = h.person_id
      WHERE 
        dh.draft_part_id = @DraftPartId
        AND h.id != @HostId
      """;

    var coHostNames = (await connection.QueryAsync<string>(
      new CommandDefinition(
        coHostsQuery,
        new { domainEvent.DraftPartId, domainEvent.HostId },
        cancellationToken: cancellationToken)))
      .ToList();

    await _eventBus.PublishAsync(
      new DraftPartHostAddedIntegrationEvent(
        id: Guid.NewGuid(),
        occurredOnUtc: _dateTimeProvider.UtcNow,
        recipientUserId: host.UserId.Value,
        draftName: context.DraftName,
        coHostNames: coHostNames),
      cancellationToken);
  }

  private sealed record HostPersonRow(Guid? UserId, string FullName);
  private sealed record DraftPartContextRow(string DraftName);
}
