namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.PlayPick;

internal sealed class Endpoint : ScreenDraftsEndpoint<PlayPickRequest, Guid>
{
  public override void Configure()
  {
    Post(DraftPartRoutes.AddPick);
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

    var command = new PlayPickCommand
    {
      DraftPartId = req.DraftPartId,
      Position = req.Position,
      PlayOrder = req.PlayOrder,
      ParticipantPublicId = req.ParticipantPublicId,
      ParticipantKind = ParticipantKind.FromValue(req.ParticipantKind),
      MovieId = req.MovieId,
      MovieVersionName = req.MovieVersionName
    };

    var result = await Sender.Send(command, ct);

    await this.SendOkAsync(result, ct);
  }
}
