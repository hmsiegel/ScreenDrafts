namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.ApplyVetoOverride;

internal sealed class Endpoint : ScreenDraftsEndpoint<ApplyVetoOverrideRequest>
{
  public override void Configure()
  {
    Post(DraftPartRoutes.ApplyVetoOverride);
    Description(x =>
    {
      x.WithTags(DraftsOpenApi.Tags.DraftParts)
      .WithName(DraftsOpenApi.Names.DraftParts_ApplyVetoOverride)
      .Produces(StatusCodes.Status204NoContent)
      .Produces(StatusCodes.Status400BadRequest)
      .Produces(StatusCodes.Status401Unauthorized)
      .Produces(StatusCodes.Status403Forbidden)
      .Produces(StatusCodes.Status404NotFound);
    });
    Policies(DraftsAuth.Permissions.DraftPartUpdate);
  }

  public override async Task HandleAsync(ApplyVetoOverrideRequest req, CancellationToken ct)
  {
    ArgumentNullException.ThrowIfNull(req);

    var actorPublicId = User.GetPublicId();

    if (string.IsNullOrWhiteSpace(actorPublicId))
    {
      await Send.UnauthorizedAsync(ct);
      return;
    }

    if (!ParticipantKind.TryFromValue(req.ParticipantKind, out var participantKind))
    {
      AddError(r => r.ParticipantKind, "Invalid participant kind.");
      await Send.ErrorsAsync(StatusCodes.Status400BadRequest, cancellation: ct);
      return;
    }

    var command = new ApplyVetoOverrideCommand
    {
      DraftPartId = req.DraftPartId,
      PlayOrder = req.PlayOrder,
      ParticipantIdValue = req.ParticipantIdValue,
      ParticipantKind = participantKind,
      ActorPublicId = actorPublicId
    };

    var result = await Sender.Send(command, ct);

    await this.SendNoContentAsync(result, ct);
  }
}
