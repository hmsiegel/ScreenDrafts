namespace ScreenDrafts.Modules.Drafts.Presentation._Legacy.Drafters;

internal sealed class AddDrafterToDrafterTeam(ISender sender) : Endpoint<AddDrafterToDrafterTeamRequest>
{
  private readonly ISender _sender = sender;

  public override void Configure()
  {
    Post("/drafters/teams/{drafterTeamId:guid}");
    Description(x =>
    {
      x.WithTags(_Legacy.Tags.Drafters)
      .WithName(nameof(AddDrafterToDrafterTeam))
      .WithDescription("Add Drafter to Drafter Team");
    });
    Policies(_Legacy.Permissions.CreateDrafter);
  }

  public override async Task HandleAsync(AddDrafterToDrafterTeamRequest req, CancellationToken ct)
  {
    var drafterTeamId = Route<Guid>("drafterTeamId");

    var command = new AddDrafterToDrafterTeamCommand(
      req.DrafterId,
      drafterTeamId);

    await this.MapResultsAsync(await _sender.Send(command, ct), ct);
  }
}

internal sealed record AddDrafterToDrafterTeamRequest(Guid DrafterId);


