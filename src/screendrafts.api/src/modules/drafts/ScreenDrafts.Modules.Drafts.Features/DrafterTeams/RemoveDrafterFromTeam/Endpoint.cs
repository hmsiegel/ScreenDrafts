namespace ScreenDrafts.Modules.Drafts.Features.DrafterTeams.RemoveDrafterFromTeam;

internal sealed class Endpoint : ScreenDraftsEndpoint<RemoveDrafterFromTeamRequest>
{
  public override void Configure()
  {
    Delete(DrafterTeamRoutes.MembershipWithDrafterId);
    Description(x =>
    {
      x.WithTags(DraftsOpenApi.Tags.DrafterTeams)
      .WithName(DraftsOpenApi.Names.DrafterTeams_RemoveDrafterFromTeam)
      .Produces(StatusCodes.Status204NoContent)
      .Produces(StatusCodes.Status401Unauthorized)
      .Produces(StatusCodes.Status403Forbidden)
      .Produces(StatusCodes.Status404NotFound);
    });
    Policies(DraftsAuth.Permissions.DrafterTeamMembers);
  }

  public override async Task HandleAsync(RemoveDrafterFromTeamRequest req, CancellationToken ct)
  {
    var command = new RemoveDrafterFromTeamCommand
    {
      DrafterTeamId = req.DrafterTeamId,
      DrafterId = req.DrafterId
    };

    var result = await Sender.Send(command, ct);

    await this.SendNoContentAsync(result, ct);
  }
}
