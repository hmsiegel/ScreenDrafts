namespace ScreenDrafts.Modules.GuestDrafts.Features.Drafters.Search;

internal sealed class SearchDraftersQueryHandler(IDbConnectionFactory dbConnectionFactory)
  : IQueryHandler<SearchDraftersQuery, IReadOnlyList<GuestDrafterSummaryResponse>>
{
  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;

  public async Task<Result<IReadOnlyList<GuestDrafterSummaryResponse>>> Handle(
    SearchDraftersQuery request,
    CancellationToken cancellationToken
  )
  {
    await using var connection = await _dbConnectionFactory.OpenConnectionAsync(cancellationToken);

    var pattern = string.IsNullOrWhiteSpace(request.Search) ? "%" : $"%{request.Search.Trim()}%";

    const string sql = $"""
      SELECT
        gd.public_id  AS {nameof(DrafterRow.PublicId)},
        gd.first_name AS {nameof(DrafterRow.FirstName)},
        gd.last_name  AS {nameof(DrafterRow.LastName)}
      FROM guest_drafts.drafters gd
      WHERE gd.first_name ILIKE @Pattern OR gd.last_name ILIKE @Pattern
      ORDER BY gd.last_name, gd.first_name
      LIMIT 50
      """;

    var rows = await connection.QueryAsync<DrafterRow>(
      new CommandDefinition(sql, new { Pattern = pattern }, cancellationToken: cancellationToken)
    );

    return Result.Success<IReadOnlyList<GuestDrafterSummaryResponse>>([
      .. rows.Select(r => new GuestDrafterSummaryResponse
      {
        PublicId = r.PublicId,
        DisplayName = $"{r.FirstName} {r.LastName}".Trim(),
      }),
    ]);
  }

  private sealed record DrafterRow(string PublicId, string FirstName, string LastName);
}
