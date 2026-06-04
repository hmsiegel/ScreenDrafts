namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.CommunityFilmRules.UpdateCommunityFilmRule;

internal sealed class Endpoint : ScreenDraftsEndpoint<RemoveCommunityFilmRuleRequest>
{
  public override void Configure()
  {
    Put(DraftPartRoutes.CommunityFilmRuleById);
    Description(x =>
    {
      x.WithTags(DraftsOpenApi.Tags.DraftParts)
        .WithName(DraftsOpenApi.Names.DraftParts_UpdateCommunityFilmRule)
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound);
    });
    Policies(DraftsAuth.Permissions.DraftPartUpdate);
  }

  public override async Task HandleAsync(RemoveCommunityFilmRuleRequest req, CancellationToken ct)
  {
    if (!CommunityFilmRuleKind.TryFromValue(req.RuleKind, out var ruleKind))
    {
      AddError(r => r.RuleKind, "Invalid rule kind.");
      await Send.ErrorsAsync(StatusCodes.Status400BadRequest, cancellation: ct);
      return;
    }

    var command = new RemoveCommunityFilmRuleCommand
    {
      DraftPartId = req.DraftPartId,
      RuleId = req.RuleId,
      RuleKind = ruleKind,
      TargetSlot = req.TargetSlot,
    };

    var result = await Sender.Send(command, ct);

    await this.SendNoContentAsync(result, ct);
  }
}
