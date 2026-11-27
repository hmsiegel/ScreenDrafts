namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Participants.SetPhantomParticipant;

internal sealed class Endpoint : ScreenDraftsEndpoint<SetPhantomParticipantRequest>
{
  public override void Configure()
  {
    Put(DraftPartRoutes.ParticipantPhantom);
    Description(x =>
    {
      x.WithTags(DraftsOpenApi.Tags.DraftParts)
        .WithName(DraftsOpenApi.Names.DraftParts_SetParticipantPhantom)
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound);
    });
    Policies(DraftsAuth.Permissions.DraftPartUpdate);
  }

  public override async Task HandleAsync(SetPhantomParticipantRequest req, CancellationToken ct)
  {
    ArgumentNullException.ThrowIfNull(req);

    var command = new SetPhantomParticipantCommand
    {
      DraftPartId = req.DraftPartId,
      ParticipantPublicId = req.ParticipantPublicId,
      IsPhantom = req.IsPhantom,
    };

    var result = await Sender.Send(command, ct);

    await this.SendNoContentAsync(result, ct);
  }
}
