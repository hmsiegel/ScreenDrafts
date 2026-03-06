namespace ScreenDrafts.Modules.Drafts.Features.Hosts.Get;

internal sealed class GetHostQueryHandler(IDbConnectionFactory dbConnectionFactory)
  : IQueryHandler<GetHostQuery, GetHostResponse>
{
  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;

  public async Task<Result<GetHostResponse>> Handle(GetHostQuery request, CancellationToken cancellationToken)
  {
    await using var connection = await _dbConnectionFactory.OpenConnectionAsync(cancellationToken);

    const string hostSql =
      $"""
      SELECT
        h.public_id AS {nameof(HostRow.HostPublicId)},
        p.public_id AS {nameof(HostRow.PersonPublicId)},
        p.first_name AS {nameof(HostRow.FirstName)},
        p.last_name AS {nameof(HostRow.LastName)},
        p.display_name AS {nameof(HostRow.DisplayName)}
      FROM drafts.hosts h
      JOIN drafts.people p ON h.person_id = p.id
      WHERE h.public_id = @HostPublicId
      """;

    var host = await connection.QuerySingleOrDefaultAsync<HostRow>(
      new CommandDefinition(
        hostSql,
        new { request.HostPublicId },
        cancellationToken: cancellationToken));

    if (host is null)
    {
      return Result.Failure<GetHostResponse>(HostErrors.NotFound(request.HostPublicId));
    }

    const string draftPartsSql =
      $"""
      SELECT
        dp.public_id AS {nameof(DraftPartPow.DraftPartPublicId)},
        d.public_id AS {nameof(DraftPartPow.DraftPublicId)},
        d.title AS {nameof(DraftPartPow.DraftTitle)},
        dp.part_index AS {nameof(DraftPartPow.PartIndex)},
        dh.role AS {nameof(DraftPartPow.Role)},
        dp.status AS {nameof(DraftPartPow.Status)}
      FROM drafts.draft_hosts dh
      JOIN drafts.draft_parts dp ON dh.draft_part_id = dp.id
      JOIN drafts.drafts d ON dp.draft_id = d.id
      WHERE dh.host_id = (SELECT id FROM drafts.hosts WHERE public_id = @HostPublicId)
      ORDER BY d.title, dp.part_index
      """;

    var draftParts = await connection.QueryAsync<DraftPartPow>(
      new CommandDefinition(
        draftPartsSql,
        new { request.HostPublicId },
        cancellationToken: cancellationToken));

    return Result.Success(new GetHostResponse
    {
      PublicId = host.HostPublicId,
      PublicPersonId = host.PersonPublicId,
      FirstName = host.FirstName,
      LastName = host.LastName,
      DisplayName = host.DisplayName,
      HostedDraftParts = [.. draftParts.Select(dp => new HostedDraftPartResponse
      {
        DraftPartPublicId = dp.DraftPartPublicId,
        DraftPublicId = dp.DraftPublicId,
        Label = dp.PartIndex > 1 ? $"{dp.DraftTitle} - Part {dp.PartIndex}" : dp.DraftTitle,
        Role = HostRole.FromValue(dp.Role).Name,
        Status = DraftPartStatus.FromValue(dp.Status).Name
      })]
    });
  }

  private sealed record HostRow(
    string HostPublicId,
    string PersonPublicId,
    string FirstName,
    string LastName,
    string? DisplayName);

  private sealed record DraftPartPow(
    string DraftPartPublicId,
    string DraftPublicId,
    string DraftTitle,
    int PartIndex,
    int Role,
    int Status);
}

