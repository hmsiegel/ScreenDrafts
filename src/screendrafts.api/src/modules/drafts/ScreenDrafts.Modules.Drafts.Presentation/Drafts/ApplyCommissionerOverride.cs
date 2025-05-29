namespace ScreenDrafts.Modules.Drafts.Presentation.Drafts;

internal sealed class ApplyCommissionerOverride(ISender sender) : Endpoint<ApplyCommissionerOverrideRequest>
{
  private readonly ISender _sender = sender;

  public override void Configure()
  {
    Post("/drafts/{draftId:guid}/commissioner-override/{pickId:guid}");
    Description(x =>
    {
      x.WithTags(Presentation.Tags.Drafts)
      .WithDescription("Applies a Commissioner Override to a draft pick.")
      .WithName(nameof(ApplyCommissionerOverride));
    });
    Policies(Presentation.Permissions.ModifyDraft);
  }

  public override async Task HandleAsync(ApplyCommissionerOverrideRequest req, CancellationToken ct)
  {
    var command = new ApplyCommissionerOverrideCommand(req.PickId);
    var result = await _sender.Send(command, ct);
    await this.MapResultsAsync(result, ct);
  }
}

public sealed record ApplyCommissionerOverrideRequest(
    [FromRoute(Name = "draftId")] Guid DraftId,
    [FromRoute(Name = "pickId")] Guid PickId);

internal sealed class ApplyCommissionerOverrideSummary : Summary<ApplyCommissionerOverride>
{
  public ApplyCommissionerOverrideSummary()
  {
    Summary = "Applies a Commissioner Override to a draft pick";
    Description = "Applies a Commissioner Override to a draft pick. This will override the current pick and apply the commissioner override.";
    Response<Guid>(StatusCodes.Status200OK, "The ID of the pick that was overridden.");
    Response(StatusCodes.Status404NotFound, "Draft or pick not found.");
    Response(StatusCodes.Status400BadRequest, "Invalid request.");
    Response(StatusCodes.Status403Forbidden, "You do not have permission to apply a commissioner override to this draft pick.");
  }
}
