namespace ScreenDrafts.Modules.Drafts.Features.Campaigns.Get;

internal sealed class GetCampaignQueryHandlerb(IDbConnectionFactory dbConnectionFactory) : IQueryHandler<GetCampaignQuery, CampaignResponse>
{
  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;

  public async Task<Result<CampaignResponse>> Handle(GetCampaignQuery GetCampaignRequest, CancellationToken cancellationToken)
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
          c.publicId = @PublicId AND c.is_deleted = FALSE
      """;

    var result = await connection.QuerySingleOrDefaultAsync<CampaignResponse>(new CommandDefinition(
      sql,
      new { GetCampaignRequest.PublicId },
      cancellationToken: cancellationToken));

    return result is not null
      ? Result.Success(result)
      : Result.Failure<CampaignResponse>(CampaignErrors.NotFound(GetCampaignRequest.PublicId));
  }
}



