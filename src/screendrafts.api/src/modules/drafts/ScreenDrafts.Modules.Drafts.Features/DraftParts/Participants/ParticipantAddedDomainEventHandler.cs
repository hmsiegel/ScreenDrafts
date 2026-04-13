namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Participants;

internal sealed class ParticipantAddedDomainEventHandler(
  IDbConnectionFactory dbConnectionFactory,
  IEventBus eventBus,
  IDateTimeProvider dateTimeProvider)
  : DomainEventHandler<ParticipantAddedDomainEvent>
{
  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;
  private readonly IEventBus _eventBus = eventBus;
  private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;

  public override async Task Handle(ParticipantAddedDomainEvent domainEvent, CancellationToken cancellationToken = default)
  {
    if (domainEvent.ParticipantKind == ParticipantKind.Community)
    {
      return;
    }

    await using var connection = await _dbConnectionFactory.OpenConnectionAsync(cancellationToken);

    var (newParticipantUserIds, newParticipantName) = domainEvent.ParticipantKind == ParticipantKind.Team
      ? await ResolveTeamAsync(connection, domainEvent.ParticipantIdValue, cancellationToken)
      : await ResolveDrafterAsync(connection, domainEvent.ParticipantIdValue, cancellationToken);

    if (newParticipantUserIds.Count == 0 && string.IsNullOrWhiteSpace(newParticipantName))
    {
      return;
    }

    const string contextQuery =
      """
      SELECT d.name as DraftName
      FROM drafts.draft_parts dp
      JOIN drafts.drafts d on d.id = dp.draft_id
      LEFT JOIN drafts.draft_part_participants dpp 
        on dpp.draft_part_id = dp.id
        AND dpp.participant_id_value = @ParticipantIdValue
      WHERE dp.id = @DraftPartId
      """;

    var context = await connection.QuerySingleAsync<DraftPartContextRow>(
      new CommandDefinition(
        commandText: contextQuery,
        parameters: new
        {
          domainEvent.ParticipantIdValue,
          domainEvent.DraftPartId
        },
        cancellationToken: cancellationToken));

    if (context is null)
    {
      return;
    }

    const string coParticipantsQuery =
      """
      SELECT
          p.user_id As UserId,
          CASE dpp.participant_kind_value
              WHEN 'Drafter' THEN CONCAT(p.first_name, ' ', p.last_name)
              WHEN 'DrafterTeam' THEN dt.name
              ELSE 'Community'
          END AS Name
      FROM drafts.draft_part_participants dpp
      LEFT JOIN drafts.drafters dr
          ON dr.id = dpp.participant_id_value
         AND dpp.participant_kind_value = 'Drafter'
      LEFT JOIN drafts.people p 
        ON p.id = dr.person_id
        AND p.user_id IS NOT NULL
      LEFT JOIN drafts.drafter_teams dt
          ON dt.id = dpp.participant_id_value
         AND dpp.participant_kind_value = 'DrafterTeam'
      WHERE dpp.draft_part_id = @DraftPartId
        AND dpp.participant_id_value != @ParticipantIdValue
      """;

    var coParticipants = (await connection.QueryAsync<CoParticipantRow>(
      new CommandDefinition(
        commandText: coParticipantsQuery,
        parameters: new
        {
          domainEvent.DraftPartId,
          domainEvent.ParticipantIdValue
        },
        cancellationToken: cancellationToken)))
        .Where(r => r.UserId.HasValue)
        .AsList();

    var coParticipantsNames = coParticipants.Select(r => r.Name).ToList();

    // Notify the new participant(s) - "you've been added"
    foreach (var userId in newParticipantUserIds)
    {
      await _eventBus.PublishAsync(
        new DraftPartParticipantAddedIntegrationEvent(
          id: Guid.NewGuid(),
          occurredOnUtc: _dateTimeProvider.UtcNow,
          recipientUserId: userId,
          draftName: context.DraftName,
          coParticipantNames: coParticipantsNames,
          kind: ParticipantAddedNotificationKind.Added,
          newParticipantName: newParticipantName),
        cancellationToken);
    }

    // Notify existing participants - "someone new has been added"
    // For DrafterTeam participants, fan out to each team member with a linked user account.
    var coParticipantUserIds = domainEvent.ParticipantKind == ParticipantKind.Team
      ? ResolveAllTeamMemberUserIds(coParticipants)
      : [.. coParticipants.Select(r => r.UserId!.Value)];

    var allParticipantNames = new List<string>(coParticipantsNames) { newParticipantName };

    foreach (var userId in coParticipantUserIds)
    {
      await _eventBus.PublishAsync(
        new DraftPartParticipantAddedIntegrationEvent(
          id: Guid.NewGuid(),
          occurredOnUtc: _dateTimeProvider.UtcNow,
          recipientUserId: userId,
          draftName: context.DraftName,
          coParticipantNames: allParticipantNames,
          kind: ParticipantAddedNotificationKind.CoParticipantNotification,
          newParticipantName: newParticipantName),
        cancellationToken);
    }
  }

  private static async Task<(IReadOnlyList<Guid> UserIds, string Name)> ResolveDrafterAsync(
    IDbConnection connection,
    Guid drafterId,
    CancellationToken cancellationToken)
  {
    var query = """
      SELECT 
        p.user_id as UserId,
        CONCAT(p.first_name, ' ', p.last_name) as Name
      FROM drafts.drafters d
      JOIN drafts.people p on p.id = d.person_id
      WHERE d.id = @DrafterId
        AND p.user_id IS NOT NULL
      """;

    var result = await connection.QuerySingleOrDefaultAsync<(Guid? UserId, string Name)>(
      new CommandDefinition(
        commandText: query,
        parameters: new { DrafterId = drafterId },
        cancellationToken: cancellationToken));

    var userIds = result.UserId.HasValue
      ? (IReadOnlyList<Guid>)[result.UserId.Value]
      : [];

    return (userIds, result.Name ?? string.Empty);
  }

  private static async Task<(IReadOnlyList<Guid> UserIds, string Name)> ResolveTeamAsync(
    IDbConnection connection,
    Guid teamId,
    CancellationToken cancellationToken)
  {
    var nameSql = """
      SELECT name
      FROM drafts.drafter_teams
      WHERE id = @TeamId
      """;

    var teamName = await connection.QuerySingleOrDefaultAsync<string>(
      new CommandDefinition(
        commandText: nameSql,
        parameters: new { TeamId = teamId },
        cancellationToken: cancellationToken))
      ?? string.Empty;

    const string emebersSql = """
      SELECT p.user_id
      FROM drafts.drafter_team_drafters dtt
      JOIN drafts.drafters d on d.id = dtt.drafter_id
      JOIN drafts.people p on p.id = d.person_id
      WHERE dtt.team_id = @TeamId
        AND p.user_id IS NOT NULL
      """;

    var userIds = (await connection.QueryAsync<Guid>(
      new CommandDefinition(
        commandText: emebersSql,
        parameters: new { TeamId = teamId },
        cancellationToken: cancellationToken)))
      .AsList();

    return (userIds, teamName);
  }

  private static List<Guid> ResolveAllTeamMemberUserIds(
    IReadOnlyList<CoParticipantRow> coParticipants)
  {
    ArgumentNullException.ThrowIfNull(coParticipants);

    return [.. coParticipants
      .Where(coParticipantRow => coParticipantRow.UserId.HasValue)
      .Select(coParticipantRow => coParticipantRow.UserId!.Value)];
  }

  private sealed record CoParticipantRow(Guid? UserId, string Name);
  private sealed record DraftPartContextRow(string DraftName);
}
