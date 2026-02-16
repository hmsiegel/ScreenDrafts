namespace ScreenDrafts.Modules.Drafts.Presentation._Legacy.Drafters;

internal sealed class CreateDrafterTeam(ISender sender) : Endpoint<CreateDrafterTeamRequest, Guid>
{
  private readonly ISender _sender = sender;

  public override void Configure()
  {
    Post("/drafters/teams");
    Description(x =>
    {
      x.WithTags(_Legacy.Tags.Drafters)
      .WithName("CreateDrafterTeam")
      .WithDescription("Create a new drafter team");
    });
    Policies(_Legacy.Permissions.CreateDrafter);
  }

  public override async Task HandleAsync(CreateDrafterTeamRequest req, CancellationToken ct)
  {
    var command = new CreateDrafterTeamCommand(req.Name);

    await this.MapResultsAsync(await _sender.Send(command, ct), ct);
  }
}

internal sealed record CreateDrafterTeamRequest(string Name);
