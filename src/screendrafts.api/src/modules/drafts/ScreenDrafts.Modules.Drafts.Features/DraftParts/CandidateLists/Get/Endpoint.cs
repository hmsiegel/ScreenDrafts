namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.CandidateLists.Get;

internal sealed class Endpoint : ScreenDraftsEndpoint<GetCandidateListRequest, GetCandidateListResponse>
{
  public override void Configure()
  {
    Get(DraftPartRoutes.CandidateList);
    Description(x =>
    {
      x.WithTags(DraftsOpenApi.Tags.CandidateLists)
      .WithName(DraftsOpenApi.Names.CandidateLists_GetList)
      .Produces<GetCandidateListResponse>(StatusCodes.Status200OK)
      .Produces(StatusCodes.Status401Unauthorized)
      .Produces(StatusCodes.Status403Forbidden)
      .Produces(StatusCodes.Status404NotFound);
    });
    Policies(DraftsAuth.Permissions.CandidateListList);
  }

  public override async Task HandleAsync(GetCandidateListRequest req, CancellationToken ct)
  {
    var query = new GetCandidateListQuery
    {
      DraftPartId = req.DraftPartId,
      Page = req.Page,
      PageSize = req.PageSize
    };

    var result = await Sender.Send(query, ct);

    await this.SendNoContentAsync(result, ct);
  }
}
