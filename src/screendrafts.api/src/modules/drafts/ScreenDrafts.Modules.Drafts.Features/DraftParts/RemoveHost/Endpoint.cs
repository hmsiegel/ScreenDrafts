using ScreenDrafts.Common.Features.Http;

namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.RemoveHost;

internal sealed class Endpoint : ScreenDraftsEndpoint<Request>
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
    Policies(Features.Permissions.DraftPartUpdate);
  }

  public override async Task HandleAsync(Request req, CancellationToken ct)
  {
    ArgumentNullException.ThrowIfNull(req);
    var command = new Command(req.DraftPartId, req.HostId);

    var result = await Sender.Send(command, ct);

    await this.MapResultsAsync(result, ct);
  }
}
