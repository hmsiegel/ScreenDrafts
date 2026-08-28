namespace ScreenDrafts.Modules.Drafts.Infrastructure.People;

internal sealed class TeamMembershipProvider(IDbConnectionFactory dbConnectionFactory)
  : ITeamMembershipProvider
{
  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;

  public async Task<IReadOnlyList<Guid>> GetCurrentMemberDrafterIdsAsync(
    Guid drafterTeamId,
    CancellationToken cancellationToken
  )
  {
    await using var connection = await _dbConnectionFactory.OpenConnectionAsync(cancellationToken);

    // Table/columns per DrafterTeamConfiguration.cs's UsingEntity join config
    // (Tables.DrafterTeamDrafter, "drafter_id", "drafter_team_id").
    const string sql = """
      SELECT drafter_id
      FROM drafts.drafter_team_drafter
      WHERE drafter_team_id = @DrafterTeamId;
      """;

    var ids = await connection.QueryAsync<Guid>(
      new CommandDefinition(
        sql,
        new { DrafterTeamId = drafterTeamId },
        cancellationToken: cancellationToken
      )
    );

    return [.. ids];
  }
}
