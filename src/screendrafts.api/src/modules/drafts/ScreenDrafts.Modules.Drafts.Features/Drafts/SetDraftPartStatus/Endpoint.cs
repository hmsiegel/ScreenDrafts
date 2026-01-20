namespace ScreenDrafts.Modules.Drafts.Features.Drafts.SetDraftPartStatus;

internal sealed class Endpoint : ScreenDraftsEndpoint<Request, Response>
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
    Policies(Features.Permissions.DraftPartStatus);
  }

  public override async Task HandleAsync(Request req, CancellationToken ct)
  {
    var command = new Command
    {
      Request = req
    };

    var result = await Sender.Send(command, ct);

    await this.SendOkAsync(result, ct);

  }
}
