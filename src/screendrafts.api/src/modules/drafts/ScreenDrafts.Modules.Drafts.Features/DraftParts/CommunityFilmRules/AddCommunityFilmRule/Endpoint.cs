namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.CommunityFilmRules.AddCommunityFilmRule;

internal sealed class Endpoint
  : ScreenDraftsEndpoint<AddCommunityFilmRuleRequest, AddCommunityFilmRuleResponse>
{
  public override void Configure()
  {
    Post(DraftPartRoutes.CommunityFilmRules);
    Description(x =>
    {
      x.WithTags(DraftsOpenApi.Tags.DraftParts)
        .WithName(DraftsOpenApi.Names.DraftParts_AddCommunityFilmRule)
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound);
    });
    Policies(DraftsAuth.Permissions.DraftPartUpdate);
  }

  public override async Task HandleAsync(AddCommunityFilmRuleRequest req, CancellationToken ct)
  {
    if (!CommunityFilmRuleKind.TryFromValue(req.RuleKind, out var ruleKind))
    {
      AddError(r => r.RuleKind, "Invalid rule kind.");
      await Send.ErrorsAsync(StatusCodes.Status400BadRequest, cancellation: ct);
      return;
    }

    var command = new AddCommunityFilmRuleCommand
    {
      DraftPartId = req.DraftPartId,
      RuleKind = ruleKind,
      TargetSlot = req.TargetSlot,
    };

    var result = await Sender.Send(command, ct);

    await this.SendCreatedAsync(
      result.Map(id => new CreatedResponse(id)),
      created => DraftPartLocations.CommunityFilmRuleById(created.PublicId),
      ct
    );
  }
}
