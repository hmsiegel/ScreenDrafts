namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.RemoveHost;

internal sealed class Endpoint : ScreenDraftsEndpoint<RemoveHostDraftPartRequest>
{
  public override void Configure()
  {
    Delete(DraftPartRoutes.RemoveHost);
    Description(x =>
    {
      x.WithTags(DraftsOpenApi.Tags.DraftParts)
       .WithName(DraftsOpenApi.Names.DraftParts_RemoveHost)
       .Produces(StatusCodes.Status204NoContent)
       .Produces(StatusCodes.Status400BadRequest)
       .Produces(StatusCodes.Status403Forbidden)
       .Produces(StatusCodes.Status404NotFound);
    });
    Policies(DraftsAuth.Permissions.DraftPartUpdate);
  }

  public override async Task HandleAsync(RemoveHostDraftPartRequest req, CancellationToken ct)
  {
    ArgumentNullException.ThrowIfNull(req);
    var RemoveHostDraftPartCommand = new RemoveHostDraftPartCommand(req.DraftPartId, req.HostId);

    var result = await Sender.Send(RemoveHostDraftPartCommand, ct);

    await this.SendNoContentAsync(result, ct);
  }
}


