namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.SubDrafts.AddSubDraft;

internal sealed class Endpoint : ScreenDraftsEndpoint<AddSubDraftRequest, CreatedResponse>
{
  public override void Configure()
  {
    Post(DraftPartRoutes.SubDrafts);
    Description(x =>
    {
      x.WithTags(DraftsOpenApi.Tags.DraftParts)
      .WithName(DraftsOpenApi.Names.SubDrafts_Add)
      .Produces<CreatedResponse>(StatusCodes.Status201Created)
      .Produces(StatusCodes.Status400BadRequest)
      .Produces(StatusCodes.Status401Unauthorized)
      .Produces(StatusCodes.Status403Forbidden)
      .Produces(StatusCodes.Status404NotFound);
    });
    Policies(DraftsAuth.Permissions.SubDraftCreate);
  }

  public override async Task HandleAsync(AddSubDraftRequest req, CancellationToken ct)
  {
    var command = new AddSubDraftCommand
    {
      DraftPartPublicId = req.DraftPartPublicId,
      Index = req.Index
    };

    var result = await Sender.Send(command, ct);

    await this.SendCreatedAsync(
      result.Map(id => new CreatedResponse(id)),
      created => DraftPartLocations.SubDraftById(
        req.DraftPartPublicId,
        created.PublicId),
      cancellationToken: ct);
  }
}
