namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Zoom.GetZoomSessionToken;

internal sealed class Endpoint : ScreenDraftsEndpoint<GetZoomSessionTokenRequest, ZoomSessionTokenResult>
{
  public override void Configure()
  {
    Get(DraftPartRoutes.ZoomSessionToken);
    Description(b =>
    {
      b.WithTags(DraftsOpenApi.Tags.DraftParts)
      .WithName(DraftsOpenApi.Names.DraftParts_GetZoomSessionToken)
      .Produces<ZoomSessionTokenResult>(StatusCodes.Status200OK)
      .Produces(StatusCodes.Status400BadRequest)
      .Produces(StatusCodes.Status401Unauthorized)
      .Produces(StatusCodes.Status403Forbidden)
      .Produces(StatusCodes.Status404NotFound);
    });
    Policies(DraftsAuth.Permissions.DraftPartRead);
  }

  public override async Task HandleAsync(GetZoomSessionTokenRequest req, CancellationToken ct)
  {
    var participantPublicId = User.GetPublicId();

    var query = new GetZoomSessionTokenQuery
    {
      DraftPartPublicId = req.DraftPartId,
      ParticipantPublicId = participantPublicId
    };

    var result = await Sender.Send(query, ct);

    await this.SendOkAsync(result, ct);
  }
}

internal sealed record GetZoomSessionTokenRequest
{
  [FromRoute(Name = "draftPartId")]
  public required string DraftPartId { get; init; }
}
