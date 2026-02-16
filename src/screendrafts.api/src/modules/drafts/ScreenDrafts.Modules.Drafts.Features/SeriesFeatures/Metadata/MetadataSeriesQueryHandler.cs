namespace ScreenDrafts.Modules.Drafts.Features.SeriesFeatures.Metadata;

internal sealed class MetadataSeriesQueryHandler : IQueryHandler<MetadataSeriesQuery, Response>
{
  public Task<Result<Response>> Handle(MetadataSeriesQuery request, CancellationToken cancellationToken)
  {
    var response = new Response
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


