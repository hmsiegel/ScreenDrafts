namespace ScreenDrafts.Modules.Drafts.Features.Drafts.SetDraftPartStatus;

internal sealed class Endpoint : ScreenDraftsEndpoint<SetDraftPartStatusRequest, Response>
{
  public override void Configure()
  {
    Put(DraftRoutes.DraftPartStatus);
    Description(x =>
    {
      x.WithTags(DraftsOpenApi.Tags.Drafts)
      .WithName(DraftsOpenApi.Names.Drafts_SetDraftPartStatus)
      .Produces<Response>(StatusCodes.Status200OK)
      .Produces(StatusCodes.Status400BadRequest)
      .Produces(StatusCodes.Status401Unauthorized)
      .Produces(StatusCodes.Status403Forbidden)
      .Produces(StatusCodes.Status404NotFound);
    });
    Policies(DraftsAuth.Permissions.DraftPartStatus);
  }

  public override async Task HandleAsync(SetDraftPartStatusRequest req, CancellationToken ct)
  {
    var SetDraftPartStatusCommand = new SetDraftPartStatusCommand
    {
      SetDraftPartStatusRequest = req
    };

    var result = await Sender.Send(SetDraftPartStatusCommand, ct);

    await this.SendOkAsync(result, ct);

  }
}


