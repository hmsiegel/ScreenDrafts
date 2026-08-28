namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.BoostersChampion.RemoveBoostersChampionAssignment;

internal sealed class Endpoint : ScreenDraftsEndpoint<RemoveBoostersChampionAssignmentRequest>
{
  public override void Configure()
  {
    Delete(DraftPartRoutes.BoostersChampionAssignmentById);
    Description(x =>
    {
      x.WithTags(DraftsOpenApi.Tags.DraftParts)
        .WithName(DraftsOpenApi.Names.DraftParts_RemoveBoostersChampionAssignment)
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound);
    });
    Policies(DraftsAuth.Permissions.DraftPartUpdate);
  }

  public override async Task HandleAsync(
    RemoveBoostersChampionAssignmentRequest req,
    CancellationToken ct
  )
  {
    var command = new RemoveBoostersChampionAssignmentCommand
    {
      DraftPartId = req.DraftPartId,
      AssignmentPublicId = req.AssignmentPublicId,
    };

    var result = await Sender.Send(command, ct);

    await this.SendNoContentAsync(result, ct);
  }
}
