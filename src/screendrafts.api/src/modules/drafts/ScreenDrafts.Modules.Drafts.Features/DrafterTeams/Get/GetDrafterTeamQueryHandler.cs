namespace ScreenDrafts.Modules.Drafts.Features.DrafterTeams.Get;

internal sealed class GetDrafterTeamQueryHandler(IDbConnectionFactory dbConnectionFactory)
  : IQueryHandler<GetDrafterTeamQuery, GetDrafterTeamResponse>
{
  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;

  public async Task<Result<GetDrafterTeamResponse>> Handle(
    GetDrafterTeamQuery request,
    CancellationToken cancellationToken)
  {
    await using var connection = await _dbConnectionFactory.OpenConnectionAsync(cancellationToken);

    const string teamSql = 
      $"""
      SELECT
        dt.public_id AS {nameof(GetDrafterTeamResponse.PublicId)},
        dt.name AS {nameof(GetDrafterTeamResponse.Name)},
        dt.number_of_drafters AS {nameof(GetDrafterTeamResponse.NumberOfDrafters)}
      FROM drafts.drafter_teams dt
      WHERE dt.public_id = @PublicId
      """;

    const string membersSql =
      $"""
      SELECT
        d.public_id AS {nameof(GetDrafterTeamMemberResponse.PublicId)},
        p.display_name AS {nameof(GetDrafterTeamMemberResponse.DisplayName)}
      FROM drafts.drafter_team_drafter dtd
      JOIN drafts.drafter_teams dt ON dt.id = dtd.drafter_team_id
      JOIN drafts.drafters d ON d.id = dtd.drafter_id
      JOIN drafts.people p ON p.id = d.person_id
      WHERE dt.public_id = @PublicId
      """;

    var team = await connection.QuerySingleOrDefaultAsync<(
      string PublicId,
      string Name,
      int NumberOfDrafters)>(new CommandDefinition(
        teamSql,
        new { request.PublicId },
        cancellationToken: cancellationToken));

    if (team == default)
    {
      return Result.Failure<GetDrafterTeamResponse>(DrafterTeamErrors.NotFound(request.PublicId));
    }

    var members = (await connection.QueryAsync<GetDrafterTeamMemberResponse>(new CommandDefinition(
      membersSql,
      new { request.PublicId },
      cancellationToken: cancellationToken))).ToList();

    return Result.Success(new GetDrafterTeamResponse
    {
      PublicId = team.PublicId,
      Name = team.Name,
      NumberOfDrafters = team.NumberOfDrafters,
      Members = members
    });
  }
}
