namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.CommunityFilmRules.RemoveCommunityFilmRule;

internal sealed class Endpoint : ScreenDraftsEndpoint<RemoveCommunityFilmRuleRequest>
{
  public override void Configure()
  {
    Delete(DraftPartRoutes.CommunityFilmRules);
    Description(x =>
    {
      x.WithTags(DraftsOpenApi.Tags.DraftParts)
        .WithName(DraftsOpenApi.Names.DraftParts_RemoveCommunityFilmRule)
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound);
    });
    Policies(DraftsAuth.Permissions.DraftPartUpdate);
  }

  public override async Task HandleAsync(RemoveCommunityFilmRuleRequest req, CancellationToken ct)
  {
    var command = new RemoveCommunityFilmRuleCommand
    {
      DraftPartId = req.DraftPartId,
      RuleId = req.RuleId,
    };

    var result = await Sender.Send(command, ct);

    await this.SendNoContentAsync(result, ct);
  }
}
