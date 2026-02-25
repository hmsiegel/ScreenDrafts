using ScreenDrafts.Modules.Drafts.Features.DrafterTeams;

namespace ScreenDrafts.Modules.Drafts.Features.DrafterTeams.Create;

internal sealed class Endpoint : ScreenDraftsEndpoint<CreateDrafterTeamRequest, string>
{
  public override void Configure()
  {
    Post(DrafterTeamRoutes.Base);
    Description(x =>
    {
      x.WithTags(DraftsOpenApi.Tags.DrafterTeams)
      .WithName(DraftsOpenApi.Names.DrafterTeams_CreateDrafterTeam)
      .Produces<string>(StatusCodes.Status201Created)
      .Produces(StatusCodes.Status400BadRequest)
      .Produces(StatusCodes.Status403Forbidden);
    });
  }

  public override async Task HandleAsync(CreateDrafterTeamRequest req, CancellationToken ct)
  {
    var command = new CreateDrafterTeamCommand
    {
      Name = req.Name
    };
    var result = await Sender.Send(command, ct);

    await this.SendCreatedAsync(
      result.Map(id => new CreatedResponse(id)),
      created => DrafterTeamLocations.ById(created.PublicId),
      ct);
  }
}
