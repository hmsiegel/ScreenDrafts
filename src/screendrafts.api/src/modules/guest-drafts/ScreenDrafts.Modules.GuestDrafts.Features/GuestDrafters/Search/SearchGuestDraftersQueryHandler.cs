namespace ScreenDrafts.Modules.GuestDrafts.Features.GuestDrafters.Search;

internal sealed class SearchGuestDraftersQueryHandler(IDbConnectionFactory dbConnectionFactory)
  : IQueryHandler<SearchGuestDraftersQuery, IReadOnlyList<GuestDrafterSummaryResponse>>
{
  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;

  public async Task<Result<IReadOnlyList<GuestDrafterSummaryResponse>>> Handle(
    SearchGuestDraftersQuery request,
    CancellationToken cancellationToken
  )
  {
    await using var connection = await _dbConnectionFactory.OpenConnectionAsync(cancellationToken);

    var pattern = string.IsNullOrWhiteSpace(request.Search) ? "%" : $"%{request.Search.Trim()}%";

    const string sql = $"""
      SELECT
        gd.public_id  AS {nameof(GuestDrafterRow.PublicId)},
        gd.first_name AS {nameof(GuestDrafterRow.FirstName)},
        gd.last_name  AS {nameof(GuestDrafterRow.LastName)}
      FROM guest_drafts.guest_drafters gd
      WHERE gd.first_name ILIKE @Pattern OR gd.last_name ILIKE @Pattern
      ORDER BY gd.last_name, gd.first_name
      LIMIT 50
      """;

    var rows = await connection.QueryAsync<GuestDrafterRow>(
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

  private sealed record GuestDrafterRow(string PublicId, string FirstName, string LastName);
}
