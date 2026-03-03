namespace ScreenDrafts.Modules.Drafts.Features.Drafts.GetDraft;

internal sealed class Endpoint : ScreenDraftsEndpoint<GetDraftRequest, GetDraftResponse>
{
  public override void Configure()
  {
    Get(DraftRoutes.ById);
    Description(x =>
    {
      x.WithTags(DraftsOpenApi.Tags.Drafts)
      .WithName(DraftsOpenApi.Names.Drafts_GetDraftById)
      .Produces<GetDraftResponse>(StatusCodes.Status200OK)
      .Produces(StatusCodes.Status404NotFound);
    });
    Policies(DraftsAuth.Permissions.DraftRead);
  }

  public override async Task HandleAsync(GetDraftRequest req, CancellationToken ct)
  {
    var includePatrson = User.HasClaim(c => c.Type == "permission" && c.Value == DraftsAuth.Permissions.PatreonSearch);

    var query = new GetDraftQuery
    {
      DraftId = req.DraftId,
      IncludePatreon = includePatrson
    };

    var result = await Sender.Send(query, ct);

    await this.SendOkAsync(result, ct);
  }
}
