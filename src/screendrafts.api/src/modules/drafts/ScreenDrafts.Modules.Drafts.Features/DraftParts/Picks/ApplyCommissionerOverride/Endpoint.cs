namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Picks.ApplyCommissionerOverride;

internal sealed class Endpoint : ScreenDraftsEndpoint<ApplyCommissionerOverrideRequest>
{
  public override void Configure()
  {
    Post(DraftPartRoutes.ApplyCommissionerOverride);
    Description(x =>
    {
      x.WithTags(DraftsOpenApi.Tags.DraftParts)
        .WithName(DraftsOpenApi.Names.DraftParts_ApplyCommissionerOverride)
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound);
    });
    Policies(DraftsAuth.Permissions.DraftPartUpdate);
  }

  public override async Task HandleAsync(ApplyCommissionerOverrideRequest req, CancellationToken ct)
  {
    ArgumentNullException.ThrowIfNull(req);

    var command = new ApplyCommissionerOverrideCommand
    {
      DraftPartId = req.DraftPartId,
      PlayOrder = req.PlayOrder,
    };

    var result = await Sender.Send(command, ct);
    await this.SendNoContentAsync(result, ct);
  }
}


