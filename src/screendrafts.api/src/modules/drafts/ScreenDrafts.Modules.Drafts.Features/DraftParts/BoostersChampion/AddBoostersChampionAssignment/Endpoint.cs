namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.BoostersChampion.AddBoostersChampionAssignment;

internal sealed class Endpoint : ScreenDraftsEndpoint<AddBoostersChampionAssignmentRequest, string>
{
  public override void Configure()
  {
    Post(DraftPartRoutes.BoostersChampionAssignments);
    Description(x =>
    {
      x.WithTags(DraftsOpenApi.Tags.DraftParts)
        .WithName(DraftsOpenApi.Names.DraftParts_AddBoostersChampionAssignment)
        .Produces<string>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound);
    });
    Policies(DraftsAuth.Permissions.DraftPartUpdate);
  }

  public override async Task HandleAsync(
    AddBoostersChampionAssignmentRequest req,
    CancellationToken ct
  )
  {
    var command = new AddBoostersChampionAssignmentCommand
    {
      DraftPartId = req.DraftPartId,
      AssignedDrafterPublicId = req.AssignedDrafterPublicId,
      TmdbId = req.TmdbId,
    };

    var result = await Sender.Send(command, ct);

    await this.SendCreatedAsync(
      result.Map(id => new CreatedResponse(id)),
      created => $"{DraftPartRoutes.BoostersChampionAssignments}/{created.PublicId}",
      ct
    );
  }
}
