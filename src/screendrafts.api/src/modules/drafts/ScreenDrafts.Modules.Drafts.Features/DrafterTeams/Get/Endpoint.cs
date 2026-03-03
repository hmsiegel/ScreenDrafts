namespace ScreenDrafts.Modules.Drafts.Features.DrafterTeams.Get;

internal sealed class Endpoint : ScreenDraftsEndpoint<GetDrafterTeamRequest, GetDrafterTeamResponse>
{
  public override void Configure()
  {
    Get(DrafterTeamRoutes.ById);
    Description(x =>
    {
      x.WithTags(DraftsOpenApi.Tags.DrafterTeams)
      .WithName(DraftsOpenApi.Names.DrafterTeams_GetDrafterTeam)
      .Produces<GetDrafterTeamResponse>(StatusCodes.Status200OK)
      .Produces(StatusCodes.Status404NotFound, contentType: "application/json");
    });
    Policies(DraftsAuth.Permissions.DrafterTeamRead);
  }

  public override async Task HandleAsync(GetDrafterTeamRequest req, CancellationToken ct)
  {
    var query = new GetDrafterTeamQuery
    {
      PublicId = req.PublicId
    };

    var result = await Sender.Send(query, ct);

    await this.SendOkAsync(result, ct);
  }
}
