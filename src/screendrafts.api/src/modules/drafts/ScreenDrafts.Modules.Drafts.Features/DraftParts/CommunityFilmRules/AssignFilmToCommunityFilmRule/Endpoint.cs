namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.CommunityFilmRules.AssignFilmToCommunityFilmRule;

internal sealed class Endpoint : ScreenDraftsEndpoint<AssignFilmToCommunityFilmRuleRequest>
{
  public override void Configure()
  {
    Put(DraftPartRoutes.CommunityFilmRuleEntry);
    Description(x =>
    {
      x.WithTags(DraftsOpenApi.Tags.DraftParts)
        .WithName(DraftsOpenApi.Names.DraftParts_AssignFilmToCommunityFilmRule)
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound);
    });
    Policies(DraftsAuth.Permissions.DraftPartUpdate);
  }

  public override async Task HandleAsync(
    AssignFilmToCommunityFilmRuleRequest req,
    CancellationToken ct
  )
  {
    var command = new AssignFilmToCommunityFilmRuleCommand
    {
      DraftPartId = req.DraftPartId,
      RuleId = req.RuleId,
      TmdbId = req.TmdbId,
    };

    var result = await Sender.Send(command, ct);

    await this.SendNoContentAsync(result, ct);
  }
}
