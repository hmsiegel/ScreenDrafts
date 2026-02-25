namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.ApplyVeto;

internal sealed class Endpoint : ScreenDraftsEndpoint<ApplyVetoRequest>
{
  public override void Configure()
  {
    Post(DraftPartRoutes.ApplyVeto);
    Description(x =>
    {
      x.WithTags(DraftsOpenApi.Tags.DraftParts)
      .WithName(DraftsOpenApi.Names.DraftParts_ApplyVeto)
      .Produces(StatusCodes.Status204NoContent)
      .Produces(StatusCodes.Status400BadRequest)
      .Produces(StatusCodes.Status401Unauthorized)
      .Produces(StatusCodes.Status403Forbidden)
      .Produces(StatusCodes.Status404NotFound);
    });
    Policies(DraftsAuth.Permissions.PickVeto);
  }

  public override async Task HandleAsync(ApplyVetoRequest req, CancellationToken ct)
  {
    var command = new ApplyVetoCommand
    {
      DraftPartId = req.DraftPartId,
      PlayOrder = req.PlayOrder,
      ParticipantPublicId = req.ParticipantPublicId,
      ParticipantKind = ParticipantKind.FromValue(req.ParticipantKind)
    };

    var result = await Sender.Send(command, ct);

    await this.SendNoContentAsync(result, ct);
  }
}
