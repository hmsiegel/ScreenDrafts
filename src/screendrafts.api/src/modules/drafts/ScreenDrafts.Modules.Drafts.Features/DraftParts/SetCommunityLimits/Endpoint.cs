namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.SetCommunityLimits;

internal sealed class Endpoint : ScreenDraftsEndpoint<SetCommunityLimitsRequest>
{
  public override void Configure()
  {
    Put(DraftPartRoutes.SetCommunityLimits);
    Description(x =>
    {
      x.WithTags(DraftsOpenApi.Tags.DraftParts)
      .WithName(DraftsOpenApi.Names.DraftParts_SetCommunityLimits)
      .Produces(StatusCodes.Status204NoContent)
      .Produces(StatusCodes.Status400BadRequest)
      .Produces(StatusCodes.Status401Unauthorized)
      .Produces(StatusCodes.Status403Forbidden)
      .Produces(StatusCodes.Status404NotFound);
    });
    Policies(DraftsAuth.Permissions.DraftPartUpdate);
  }

  public override async Task HandleAsync(SetCommunityLimitsRequest req, CancellationToken ct)
  {
    var command = new SetCommunityLimitsCommand
    {
      DraftPartId = req.DraftPartId,
      MaxCommunityPicks = req.MaxCommunityPicks,
      MaxCommunityVetoes = req.MaxCommunityVetoes
    };

    var result = await  Sender.Send(command, ct);

    await this.SendNoContentAsync(result, ct);
  }
}
