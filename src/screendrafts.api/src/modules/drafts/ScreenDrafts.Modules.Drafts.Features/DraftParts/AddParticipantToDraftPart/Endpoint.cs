namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.AddParticipantToDraftPart;

internal sealed class Endpoint : ScreenDraftsEndpoint<AddParticipantToDraftPartRequest>
{
  public override void Configure()
  {
    Post(DraftPartRoutes.Participants);
    Description(x =>
    {
      x.WithTags(DraftsOpenApi.Tags.DraftParts)
      .WithName(DraftsOpenApi.Names.DraftParts_AddParticipant)
      .Produces(StatusCodes.Status204NoContent)
      .Produces(StatusCodes.Status400BadRequest)
      .Produces(StatusCodes.Status403Forbidden)
      .Produces(StatusCodes.Status404NotFound);
    });
    Policies(DraftsAuth.Permissions.DraftPartUpdate);
  }

  public override async Task HandleAsync(AddParticipantToDraftPartRequest req, CancellationToken ct)
  {
    ArgumentNullException.ThrowIfNull(req);

    var command = new AddParticipantToDraftPartCommand
    {
      DraftPartId = req.DraftPartId,
      ParticipantPublicId = req.ParticipantPublicId,
      ParticipantKind = ParticipantKind.FromValue(req.ParticipantKind)
    };

    var result = await Sender.Send(command, ct);

    await this.SendNoContentAsync(result, ct);
  }
}
