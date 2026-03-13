namespace ScreenDrafts.Modules.Drafts.Features.Drafts.ListUpcomingDrafts;

internal sealed class Endpoint : ScreenDraftsEndpointWithoutRequest<ListUpcomingDraftsResponse>
{
  public override void Configure()
  {
    Get(DraftRoutes.Upcoming);
    Description(x =>
    {
      x.WithName(DraftsOpenApi.Names.Drafts_ListUpcomingDrafts)
      .WithTags(DraftsOpenApi.Tags.Drafts)
      .Produces<ListUpcomingDraftsResponse>(StatusCodes.Status200OK);
    });
    Policies(DraftsAuth.Permissions.DraftsList);
  }

  public override async Task HandleAsync(CancellationToken ct)
  {
    var includePatreon = User.Claims.Any(c => c.Type == "sub" && c.Value == "patreon");

    var query = new ListUpcomingDraftsQuery
    {
      UserId = User.GetUserId(),
      IsAdmin = User.IsInRole(DraftRoles.Admin),
      IncludePatreon = includePatreon
    };

    var result = await Sender.Send (query, ct);

    await this.SendOkAsync(result, ct);
  }
}
