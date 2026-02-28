namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.RemoveParticipantFromDraftPart;

internal sealed class Endpoint : ScreenDraftsEndpoint<RemoveParticipantFromDraftPartRequest>
{
  public override void Configure()
  {
    Delete(DraftPartRoutes.Participants);
    Description(x =>
    {
      x.WithTags(DraftsOpenApi.Tags.DraftParts)
      .WithName(DraftsOpenApi.Names.DraftParts_RemoveParticipant)
      .Produces(StatusCodes.Status204NoContent)
      .Produces(StatusCodes.Status400BadRequest)
      .Produces(StatusCodes.Status403Forbidden)
      .Produces(StatusCodes.Status404NotFound);
    });
    Policies(DraftsAuth.Permissions.DraftPartUpdate);
  }

  public override async Task HandleAsync(RemoveParticipantFromDraftPartRequest req, CancellationToken ct)
  {
    ArgumentNullException.ThrowIfNull(req);

    if (!ParticipantKind.TryFromValue(req.ParticipantKind, out var participantKind))
    {
      AddError(r => r.ParticipantKind, "Invalid participant kind.");
      await Send.ErrorsAsync(StatusCodes.Status400BadRequest, cancellation: ct);
      return;
    }

    var command = new RemoveParticipantFromDraftPartCommand
    {
      DraftPartPublicId = req.DraftPartPublicId,
      ParticipantPublicId = req.ParticipantPublicId,
      ParticipantKind = participantKind
    };

    var result = await Sender.Send(command, ct);

    await this.SendNoContentAsync(result, ct);
  }
}
