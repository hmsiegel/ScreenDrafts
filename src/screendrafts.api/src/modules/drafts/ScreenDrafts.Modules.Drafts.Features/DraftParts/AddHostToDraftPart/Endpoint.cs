using ScreenDrafts.Modules.Drafts.Domain.Hosts;

namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.AddHostToDraftPart;

internal sealed class Endpoint : ScreenDraftsEndpoint<AddHostToDraftPartRequest>
{
  public override void Configure()
  {
    Post(DraftPartRoutes.Hosts);
    Description(x =>
    {
      x.WithTags(DraftsOpenApi.Tags.Hosts)
      .WithName(DraftsOpenApi.Names.DraftParts_AddHost)
      .Produces(StatusCodes.Status204NoContent)
      .Produces(StatusCodes.Status400BadRequest)
      .Produces(StatusCodes.Status401Unauthorized)
      .Produces(StatusCodes.Status403Forbidden)
      .Produces(StatusCodes.Status404NotFound);
    });
    Policies(DraftsAuth.Permissions.DraftPartUpdate);
  }

  public override async Task HandleAsync(AddHostToDraftPartRequest req, CancellationToken ct)
  {
    ArgumentNullException.ThrowIfNull(req);

    var command = new AddHostToDraftPartCommand
    {
      DraftPartId = req.DraftPartId,
      HostPublicId = req.HostPublicId,
      HostRole = HostRole.FromValue(req.HostRole)
    };

    var result = await Sender.Send(command, ct);

    await this.SendNoContentAsync(result, ct);
  }
}
