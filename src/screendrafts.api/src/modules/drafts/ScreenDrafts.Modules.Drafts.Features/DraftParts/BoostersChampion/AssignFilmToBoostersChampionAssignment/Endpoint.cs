namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.BoostersChampion.AssignFilmToBoostersChampionAssignment;

internal sealed class Endpoint : ScreenDraftsEndpoint<AssignFilmToBoostersChampionAssignmentRequest>
{
  public override void Configure()
  {
    Put(DraftPartRoutes.BoostersChampionAssignmentFilm);
    Description(x =>
    {
      x.WithTags(DraftsOpenApi.Tags.DraftParts)
        .WithName(DraftsOpenApi.Names.DraftParts_AssignFilmToBoostersChampionAssignment)
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound);
    });
    Policies(DraftsAuth.Permissions.DraftPartUpdate);
  }

  public override async Task HandleAsync(
    AssignFilmToBoostersChampionAssignmentRequest req,
    CancellationToken ct
  )
  {
    var command = new AssignFilmToBoostersChampionAssignmentCommand
    {
      DraftPartId = req.DraftPartId,
      AssignmentPublicId = req.AssignmentPublicId,
      TmdbId = req.TmdbId,
    };

    var result = await Sender.Send(command, ct);

    await this.SendNoContentAsync(result, ct);
  }
}
