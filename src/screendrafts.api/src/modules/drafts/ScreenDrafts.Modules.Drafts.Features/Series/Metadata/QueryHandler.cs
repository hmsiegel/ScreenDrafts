namespace ScreenDrafts.Modules.Drafts.Features.Series.Metadata;

internal sealed class QueryHandler : IQueryHandler<Query, Response>
{
  public Task<Result<Response>> Handle(Query request, CancellationToken cancellationToken)
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
