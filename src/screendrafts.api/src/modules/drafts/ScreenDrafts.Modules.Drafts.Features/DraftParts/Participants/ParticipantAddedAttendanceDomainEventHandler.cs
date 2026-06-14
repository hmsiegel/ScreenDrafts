namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Participants;

/// <summary>
/// Creates a Pending attendance record when a participant is added to a draft part.
/// Runs alongside ParticipantAddedDomainEventHandler (notifications).
/// Community participants are excluded — they have no attendance record.
/// DrafterTeam participants create one record per team member with a linked person.
/// </summary>
internal sealed class ParticipantAddedAttendanceHandler(
  IDbConnectionFactory dbConnectionFactory,
  IAttendanceRepository attendanceRepository,
  IPublicIdGenerator publicIdGenerator
) : DomainEventHandler<ParticipantAddedDomainEvent>
{
  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;
  private readonly IAttendanceRepository _attendanceRepository = attendanceRepository;
  private readonly IPublicIdGenerator _publicIdGenerator = publicIdGenerator;

  public override async Task Handle(
    ParticipantAddedDomainEvent domainEvent,
    CancellationToken cancellationToken = default
  )
  {
    if (domainEvent.ParticipantKind == ParticipantKind.Community)
    {
      return;
    }

    await using var connection = await _dbConnectionFactory.OpenConnectionAsync(cancellationToken);

    var personPublicIds =
      domainEvent.ParticipantKind == ParticipantKind.Team
        ? await ResolveTeamMemberPublicIdsAsync(
          connection,
          domainEvent.ParticipantIdValue,
          cancellationToken
        )
        : await ResolveDrafterPublicIdAsync(
          connection,
          domainEvent.ParticipantIdValue,
          cancellationToken
        );

    var draftPartId = DraftPartId.Create(domainEvent.DraftPartId);

    foreach (var personPublicId in personPublicIds)
    {
      // Skip if a record already exists (idempotency)
      var exists = await _attendanceRepository.ExistsAsync(
        draftPartId,
        personPublicId,
        cancellationToken
      );
      if (exists)
        continue;

      var createResult = DraftPartAttendance.Create(
        draftPartId: draftPartId,
        personPublicId: personPublicId,
        publicId: _publicIdGenerator.GeneratePublicId(PublicIdPrefixes.Attendance)
      );

      if (createResult.IsFailure)
        continue;

      _attendanceRepository.Add(createResult.Value);
    }
  }

  private static async Task<IReadOnlyList<string>> ResolveDrafterPublicIdAsync(
    IDbConnection connection,
    Guid drafterId,
    CancellationToken cancellationToken
  )
  {
    const string sql = """
      SELECT p.public_id
      FROM drafts.drafters d
      JOIN drafts.people p ON p.id = d.person_id
      WHERE d.id = @DrafterId
      """;

    var publicId = await connection.QuerySingleOrDefaultAsync<string>(
      new CommandDefinition(
        sql,
        new { DrafterId = drafterId },
        cancellationToken: cancellationToken
      )
    );

    return publicId is null ? [] : [publicId];
  }

  private static async Task<IReadOnlyList<string>> ResolveTeamMemberPublicIdsAsync(
    IDbConnection connection,
    Guid teamId,
    CancellationToken cancellationToken
  )
  {
    const string sql = """
      SELECT p.public_id
      FROM drafts.drafter_team_drafter dtd
      JOIN drafts.drafters d ON d.id = dtd.drafter_id
      JOIN drafts.people pe ON pe.id = d.person_id
      WHERE dtd.drafter_team_id = @TeamId
      """;

    var publicIds = await connection.QueryAsync<string>(
      new CommandDefinition(sql, new { TeamId = teamId }, cancellationToken: cancellationToken)
    );

    return [.. publicIds];
  }
}
