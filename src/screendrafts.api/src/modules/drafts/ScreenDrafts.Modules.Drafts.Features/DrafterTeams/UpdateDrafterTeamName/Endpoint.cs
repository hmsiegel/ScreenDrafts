namespace ScreenDrafts.Modules.Drafts.Features.DrafterTeams.UpdateDrafterTeamName;

internal sealed class Endpoint : ScreenDraftsEndpoint<UpdateDrafterTeamNameRequest>
{
  public override void Configure()
  {
    Put(DrafterTeamRoutes.ById);
    Description(x =>
    {
      x.WithTags(DraftsOpenApi.Tags.DrafterTeams)
        .WithName(DraftsOpenApi.Names.DrafterTeams_UpdateName)
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound);
    });
    Policies(DraftsAuth.Permissions.DrafterTeamMembers);
  }

  public override async Task HandleAsync(UpdateDrafterTeamNameRequest req, CancellationToken ct)
  {
    var command = new UpdateDrafterTeamNameCommand
    {
      DrafterTeamId = req.DrafterTeamId,
      Name = req.Name,
    };

    var result = await Sender.Send(command, ct);

    await this.SendNoContentAsync(result, ct);
  }
}
