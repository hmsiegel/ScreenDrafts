namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Picks.PlayPick;

internal sealed class Endpoint : ScreenDraftsEndpoint<PlayPickRequest, CreatedIdResponse>
{
  public override void Configure()
  {
    Post(DraftPartRoutes.Picks);
    Description(x =>
    {
      x.WithName(DraftsOpenApi.Names.DraftParts_PlayPick)
      .WithTags(DraftsOpenApi.Tags.DraftParts)
      .Produces<Guid>(StatusCodes.Status200OK)
      .Produces(StatusCodes.Status400BadRequest)
      .Produces(StatusCodes.Status401Unauthorized)
      .Produces(StatusCodes.Status404NotFound)
      .Produces(StatusCodes.Status403Forbidden);
    });
    Policies(DraftsAuth.Permissions.PickCreate);
  }

  public override async Task HandleAsync(PlayPickRequest req, CancellationToken ct)
  {
    ArgumentNullException.ThrowIfNull(req);

    var actorPublicId = User.GetPublicId();

    if (string.IsNullOrWhiteSpace(actorPublicId))
    {
      await Send.UnauthorizedAsync(cancellation: ct);
      return;
    }

    if (!ParticipantKind.TryFromValue(req.ParticipantKind, out var participantKind))
    {
      AddError(r => r.ParticipantKind, "Invalid participant kind.");
      await Send.ErrorsAsync(StatusCodes.Status400BadRequest, cancellation: ct);
      return;
    }

    var command = new PlayPickCommand
    {
      DraftPartId = req.DraftPartId,
      Position = req.Position,
      PlayOrder = req.PlayOrder,
      ParticipantPublicId = req.ParticipantPublicId,
      ParticipantKind = participantKind,
      MoviePublicId = req.MoviePublicId,
      MovieVersionName = req.MovieVersionName,
      ActedByPublicId = actorPublicId
    };

    var result = await Sender.Send(command, ct);

    await this.SendCreatedAsync(
      result.Map(id => new CreatedIdResponse(id.Value)),
      created => DraftPartLocations.ById(req.DraftPartId),
      ct);
  }
}
