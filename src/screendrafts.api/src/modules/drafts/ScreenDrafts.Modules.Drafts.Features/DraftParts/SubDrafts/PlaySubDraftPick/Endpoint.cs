namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.SubDrafts.PlaySubDraftPick;

internal sealed class Endpoint : ScreenDraftsEndpoint<PlaySubDraftPickRequest, CreatedIdResponse>
{
  public override void Configure()
  {
    Post(DraftPartRoutes.SubDraftPicks);
    Description(x =>
    {
      x.WithTags(DraftsOpenApi.Tags.DraftParts)
      .WithName(DraftsOpenApi.Names.SubDrafts_PlayPick)
      .Produces<CreatedResponse>(StatusCodes.Status201Created)
      .Produces(StatusCodes.Status400BadRequest)
      .Produces(StatusCodes.Status401Unauthorized)
      .Produces(StatusCodes.Status403Forbidden)
      .Produces(StatusCodes.Status404NotFound);
    });
    Policies(DraftsAuth.Permissions.PickAdd);
  }

  public override async Task HandleAsync(PlaySubDraftPickRequest req, CancellationToken ct)
  {
    var actorPublicId = User.GetPublicId();

    if (string.IsNullOrWhiteSpace(actorPublicId))
    {
      await Send.UnauthorizedAsync(ct);
      return;
    }

    if (!ParticipantKind.TryFromValue(req.ParticipantKind, out var participantKind))
    {
      AddError(r => r.ParticipantKind, "Invalid participant kind.");
      await Send.ErrorsAsync(StatusCodes.Status400BadRequest, ct);
      return;
    }

    var command = new PlaySubDraftPickCommand
    {
      DraftPartPublicId = req.DraftPartPublicId,
      SubDraftPublicId = req.SubDraftPublicId,
      MoviePublicId = req.MoviePublicId,
      Position = req.Position,
      PlayOrder = req.PlayOrder,
      ParticipantPublicId = req.ParticipantPublicId,
      ParticipantKind = participantKind,
      ActedByPublicId = actorPublicId
    };

    var result = await Sender.Send(command, ct);

    await this.SendCreatedAsync(
      result.Map(id => new CreatedIdResponse(id.Value)),
      created => DraftPartLocations.SubDraftById(
        draftPartPublicId: req.DraftPartPublicId,
        subDraftPublicId: req.SubDraftPublicId),
       ct);
  }
}
