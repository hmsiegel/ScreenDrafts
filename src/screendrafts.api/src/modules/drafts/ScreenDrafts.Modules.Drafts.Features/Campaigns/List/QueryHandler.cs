namespace ScreenDrafts.Modules.Drafts.Features.Campaigns.List;

internal sealed class QueryHandler(IDbConnectionFactory dbConnectionFactory)
  : IQueryHandler<Query, CampaignCollectionResponse>
{
  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;

  public async Task<Result<CampaignCollectionResponse>> Handle(
    Query request,
    CancellationToken cancellationToken)
  {
    await using var connection = await _dbConnectionFactory.OpenConnectionAsync(cancellationToken);

    const string sql =
      $"""
        SELECT
          c.publicId AS {nameof(CampaignResponse.PublicId)},
          c.name AS {nameof(CampaignResponse.Name)},
          c.slug AS {nameof(CampaignResponse.Slug)}
        FROM
          drafts.campaigns c
        WHERE
          (@IncludeDeleted = TRUE OR c.is_deleted = FALSE)
        ORDER BY
          c.name ASC
      """;

    var campaigns = (await connection.QueryAsync<CampaignResponse>(
      new CommandDefinition(sql, cancellationToken: cancellationToken)
    )).ToList();

    var response = new CampaignCollectionResponse(campaigns);

    return Result.Success(response);
  }
}

