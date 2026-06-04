namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Get;

internal sealed class Endpoint
  : ScreenDraftsEndpoint<GetDraftPartRequest, GetDraftPartQueryResponse>
{
  public override void Configure()
  {
    Get(DraftPartRoutes.ById);
    Description(x =>
    {
      x.WithTags(DraftsOpenApi.Tags.DraftParts)
        .WithName(DraftsOpenApi.Names.DraftParts_GetDraftPartById)
        .Produces<GetDraftPartQueryResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);
    });
    AllowAnonymous();
  }

  public override async Task HandleAsync(GetDraftPartRequest req, CancellationToken ct)
  {
    var includePatreon = User.HasClaim(c =>
      c.Type == "permission" && c.Value == DraftsAuth.Permissions.PatreonSearch
    );

    var query = new GetDraftPartQuery
    {
      DraftPartId = req.DraftPartId,
      IncludePatreon = includePatreon,
    };

    var result = await Sender.Send(query, ct);

    await this.SendOkAsync(result, ct);
  }
}
