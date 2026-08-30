namespace ScreenDrafts.Modules.Drafts.Features.SeriesFeatures.GetSeriesMetadata;

internal sealed class GetSeriesMetadataQueryHandler
  : IQueryHandler<GetSeriesMetadataQuery, GetSeriesMetadataResponse>
{
  public Task<Result<GetSeriesMetadataResponse>> Handle(
    GetSeriesMetadataQuery request,
    CancellationToken cancellationToken
  )
  {
    var response = new GetSeriesMetadataResponse
    {
      SeriesKinds = QueryMapping.AllSmartEnums<SeriesKind>(),
      CanonicalPolicies = QueryMapping.AllSmartEnums<CanonicalPolicy>(),
      ContinuityScopes = QueryMapping.AllSmartEnums<ContinuityScope>(),
      ContinuityDateRules = QueryMapping.AllSmartEnums<ContinuityDateRule>(),
      DraftTypes = QueryMapping.AllDraftTypes(),
    };
    return Task.FromResult(Result.Success(response));
  }
}
