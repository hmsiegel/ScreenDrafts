namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.SubDrafts.AssignSubDraftPosition;

internal sealed class Endpoint : ScreenDraftsEndpoint<AssignSubDraftPositionRequest>
{
  public override void Configure()
  {
    Post(DraftPartRoutes.SubDraftPosition);
    Description(x =>
    {
      x.WithTags(DraftsOpenApi.Tags.DraftParts)
        .WithName(DraftsOpenApi.Names.SubDrafts_AssignPosition)
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound);
    });
    Policies(DraftsAuth.Permissions.SubDraftUpdate);
  }

  public override async Task HandleAsync(AssignSubDraftPositionRequest req, CancellationToken ct)
  {
    if (!ParticipantKind.TryFromValue(req.WinnerParticipantKind, out var winnerKind))
    {
      AddError(r => r.WinnerParticipantKind, "Invalid participant kind.");
      await Send.ErrorsAsync(StatusCodes.Status400BadRequest, ct);
      return;
    }

    var command = new AssignSubDraftPositionCommand
    {
      DraftPartId = req.DraftPartId,
      SubDraftId = req.SubDraftId,
      WinnerParticipantPublicId = req.WinnerParticipantPublicId,
      WinnerParticipantKind = winnerKind,
      Choice = req.Choice,
    };

    var result = await Sender.Send(command, ct);

    await this.SendNoContentAsync(result, ct);
  }
}
