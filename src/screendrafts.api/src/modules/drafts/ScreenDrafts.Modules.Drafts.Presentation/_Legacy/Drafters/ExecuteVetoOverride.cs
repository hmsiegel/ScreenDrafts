namespace ScreenDrafts.Modules.Drafts.Presentation._Legacy.Drafters;

internal sealed class ExecuteVetoOverride(ISender sender) : Endpoint<VetoOverrideRequest>
{
  private readonly ISender _sender = sender;

  public override void Configure()
  {
    Post("/drafts/{vetoId:guid}/vetooveride");
    Description(x =>
    {
      x.WithTags(_Legacy.Tags.Drafters)
      .WithName(nameof(ExecuteVetoOverride))
      .WithDescription("Override a veto on a pick.");
    });
    Policies(_Legacy.Permissions.VetoOverride);
  }

  public override async Task HandleAsync(VetoOverrideRequest req, CancellationToken ct)
  {
    var command = new ExecuteVetoOverrideCommand(
      req.DrafterId,
      req.DrafterTeamId,
      req.VetoId);

    var result = await _sender.Send(command, ct);

    await this.MapResultsAsync(result, ct);
  }
}

public sealed record VetoOverrideRequest(
  Guid? DrafterId,
  Guid? DrafterTeamId,
  [FromRoute(Name = "vetoId")] Guid VetoId);


internal sealed class ExecuteVetoOverrideSummary : Summary<ExecuteVetoOverride>
{
  public ExecuteVetoOverrideSummary()
  {
    Summary = "Override a veto on a pick.";
    Description = "Override a veto on a pick. This endpoint allows a drafter to override a veto on a specific pick in the draft.";
    Response<Guid>(StatusCodes.Status200OK, "The ID of the veto that was overridden.");
    Response(StatusCodes.Status404NotFound, "Veto not found.");
    Response(StatusCodes.Status400BadRequest, "Invalid request.");
    Response(StatusCodes.Status403Forbidden, "You do not have permission to access this resource.");
  }
}
