namespace ScreenDrafts.Modules.Drafts.Features.Drafts.DraftPools.Get;

internal sealed class Endpoint : ScreenDraftsEndpoint<GetDraftPoolRequest, DraftPoolResponse>
{
  public override void Configure()
  {
    Get(DraftRoutes.Pool);
    Description(x =>
    {
      x.WithTags(DraftsOpenApi.Tags.DraftPools)
        .WithName(DraftsOpenApi.Names.DraftPools_GetPool)
        .Produces<DraftPoolResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);
    });
    Policies(DraftsAuth.Permissions.DraftPoolRead);
  }

  public override async Task HandleAsync(GetDraftPoolRequest req, CancellationToken ct)
  {
    var query = new GetDraftPoolQuery { PublicId = req.PublicId };

    var result = await Sender.Send(query, ct);

    await this.SendOkAsync(result, ct);
  }
}
