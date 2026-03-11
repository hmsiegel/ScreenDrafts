namespace ScreenDrafts.Modules.Drafts.Features.Drafts.DraftBoards;

internal sealed class DraftBoardParticipantResolver(
  IDbConnectionFactory dbConnectionFactory,
  IUsersApi usersApi,
  ParticipantResolver participantResolver)
{
  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;
  private readonly IUsersApi _usersApi = usersApi;
  private readonly ParticipantResolver _participantResolver = participantResolver;

  public async Task<ResolvedParticipant?> ResolveAsync(string userPublicId, CancellationToken cancellationToken = default)
  {
    var user = await _usersApi.GetUserByPublicId(userPublicId, cancellationToken);

    if (user is null)
    {
      return null;
    }

    await using var connection = await _dbConnectionFactory.OpenConnectionAsync(cancellationToken);

    const string sql =
      $"""
      SELECT
        COALESCE (dr.id, dt.id) AS participant_id,
        CASE WHEN dr.id IS NOT NULL THEN 0 ELSE 1 END
      FROM drafts.people p
      LEFT JOIN drafts.drafters dr ON dr.person_id = p.id
      LEFT JOIN drafts.drafter_team_drafter dtm ON dtm.drafter_id = dr.id
      LEFT JOIN drafts.drafter_teams dt ON dt.id = dtm.drafter_team_id
      WHERE p.user_id = @UserId AND
        (dr.id IS NOT NULL OR dt.id IS NOT NULL)
      LIMIT 1
      """;

    var row = await connection.QuerySingleOrDefaultAsync<(Guid ParticipantId, int ParticipantKind)>(
      new CommandDefinition(
        sql,
        new { user.UserId },
        cancellationToken: cancellationToken));

    if (row == default)
    {
      return null;
    }

    var kind = ParticipantKind.FromValue(row.ParticipantKind);

    var participant = await _participantResolver.ResolveByParticpantIdAsync(
      participantId: row.ParticipantId,
      participantKind: kind,
      cancellationToken: cancellationToken);

    if (participant is null)
    {
      return null;
    }

    return new ResolvedParticipant(participant.Value, user.UserId);
  }
}
