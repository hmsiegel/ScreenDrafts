namespace ScreenDrafts.Modules.Drafts.Features.Drafts.CreateDraftPart;

internal sealed class Endpoint : ScreenDraftsEndpoint<CreateDraftPartRequest, string>
{
  public override void Configure()
  {
    Post(DraftRoutes.Parts);
    Description(x =>
    {
      x.WithName(DraftsOpenApi.Names.Drafts_CreateDraftPart)
      .WithTags(DraftsOpenApi.Tags.Drafts)
      .Produces<string>(StatusCodes.Status200OK)
      .Produces(StatusCodes.Status400BadRequest)
      .Produces(StatusCodes.Status401Unauthorized)
      .Produces(StatusCodes.Status403Forbidden)
      .Produces(StatusCodes.Status404NotFound);
    });
    Policies(DraftsAuth.Permissions.DraftPartCreate);
  }

  public override async Task HandleAsync(CreateDraftPartRequest req, CancellationToken ct)
  {
    var command = new CreateDraftPartCommand
    {
      DraftId = req.DraftId,
      PartIndex = req.PartIndex,
      MaximumPosition = req.MaximumPosition,
      MinimumPosition = req.MinimumPosition
    };

    var result = await Sender.Send(command, ct);

    await this.SendCreatedAsync(
      result.Map(id => new CreatedResponse(id)),
      created => DraftLocations.PartById(
        draftPublicId: req.DraftId,
        draftPartPublicId: created.PublicId),
      ct);
  }
}
