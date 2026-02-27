namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.SetDraftPosition;

internal sealed class Endpoint : ScreenDraftsEndpoint<SetDraftPositionsRequest>
{
  public override void Configure()
  {
    Put(DraftPartRoutes.SetDraftPositions);
    Description(x =>
    {
      x.WithTags(DraftsOpenApi.Tags.DraftParts)
        .WithName(DraftsOpenApi.Names.DraftParts_SetDraftPosition)
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden);
    });
    Policies(DraftsAuth.Permissions.DraftPartUpdate);
  }

  public override async Task HandleAsync(SetDraftPositionsRequest req, CancellationToken ct)
  {
    ArgumentNullException.ThrowIfNull(req);

    var command = new SetDraftPositionsCommand
    {
      DraftPartId = req.DraftPartId,
      Positions = [.. req.Positions.Select(p => new DraftPositionRequest
      {
        Name = p.Name,
        Picks = p.Picks,
        HasBonusVeto = p.HasBonusVeto,
        HasBonusVetoOverride = p.HasBonusVetoOverride
      })]
    };

    var result = await Sender.Send(command, ct);

    await this.SendNoContentAsync(result, ct);
  }
}
