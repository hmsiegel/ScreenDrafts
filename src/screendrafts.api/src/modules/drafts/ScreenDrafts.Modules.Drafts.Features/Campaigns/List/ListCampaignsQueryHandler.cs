namespace ScreenDrafts.Modules.Drafts.Features.Campaigns.List;

internal sealed class ListCampaignsQueryHandlerb(IDbConnectionFactory dbConnectionFactory)
  : IQueryHandler<ListCampaignsQuery, CampaignCollectionResponse>
{
  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;

  public async Task<Result<CampaignCollectionResponse>> Handle(
    ListCampaignsQuery ListCampaignsRequest,
    CancellationToken cancellationToken)
  {
    await using var connection = await _dbConnectionFactory.OpenConnectionAsync(cancellationToken);

    const string sql =
      $"""
        SELECT
          c.publicId AS {nameof(CampaignResponse.PublicId)},
          c.name AS {nameof(CampaignResponse.Name)},
          c.slug AS {nameof(CampaignResponse.Slug)},
          c.is_deleted AS {nameof(CampaignResponse.IsDeleted)}
        FROM
          drafts.campaigns c
        WHERE
          (@IncludeDeleted = TRUE OR c.is_deleted = FALSE)
        ORDER BY
          c.name ASC
      """;

    var campaigns = (await connection.QueryAsync<CampaignResponse>(
      new CommandDefinition(
        sql,
        parameters: new { ListCampaignsRequest.IncludeDeleted },
        cancellationToken: cancellationToken)
    )).ToList();

    var response = new CampaignCollectionResponse(campaigns);

    return Result.Success(response);
  }
}




