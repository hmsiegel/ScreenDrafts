namespace ScreenDrafts.Modules.Drafts.Features.DrafterTeams.AddDrafterToTeam;

internal sealed class Endpoint : ScreenDraftsEndpoint<AddDrafterToTeamRequest>
{
  public override void Configure()
  {
    Post(DrafterTeamRoutes.Membership);
    Description(x =>
    {
      x.WithName(DraftsOpenApi.Names.DrafterTeams_AddDrafter)
      .WithTags(DraftsOpenApi.Tags.DrafterTeams)
      .Produces(StatusCodes.Status204NoContent)
      .Produces(StatusCodes.Status401Unauthorized)
      .Produces(StatusCodes.Status403Forbidden)
      .Produces(StatusCodes.Status404NotFound);
    });
    Policies(DraftsAuth.Permissions.DrafterTeamMembers);
  }

  public override async Task HandleAsync(AddDrafterToTeamRequest req, CancellationToken ct)
  {
    var command = new AddDrafterToTeamCommand
    {
      DrafterTeamId = req.DrafterTeamId,
      DrafterId = req.DrafterId
    };

    var result = await Sender.Send(command, ct);

    await this.SendNoContentAsync(result, ct);
  }
}
