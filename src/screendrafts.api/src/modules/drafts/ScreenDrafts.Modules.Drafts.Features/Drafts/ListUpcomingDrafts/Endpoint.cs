namespace ScreenDrafts.Modules.Drafts.Features.Drafts.ListUpcomingDrafts;

internal sealed class Endpoint(IAdministrationApi administrationApi)
  : ScreenDraftsEndpointWithoutRequest<ListUpcomingDraftsResponse>
{
  private readonly IAdministrationApi _administrationApi = administrationApi;

  public override void Configure()
  {
    Get(DraftRoutes.Upcoming);
    Description(x =>
    {
      x.WithName(DraftsOpenApi.Names.Drafts_ListUpcomingDrafts)
      .WithTags(DraftsOpenApi.Tags.Drafts)
      .Produces<ListUpcomingDraftsResponse>(StatusCodes.Status200OK);
    });
    AllowAnonymous();
  }

  public override async Task HandleAsync(CancellationToken ct)
  {
    var isAuthenticated = User.Identity?.IsAuthenticated == true;

    var includePatreon = isAuthenticated && User.HasPermission(DraftsAuth.Permissions.PatreonSearch);

    var isAdmin = false;
    if (isAuthenticated)
    {
      var roles = await _administrationApi.GetUserRolesAsync(User.GetPublicId(), ct);
      isAdmin = roles.Contains(DraftsAuth.Roles.SuperAdmin, StringComparer.OrdinalIgnoreCase)
        || roles.Contains(DraftsAuth.Roles.Admin, StringComparer.OrdinalIgnoreCase);
    }

    var query = new ListUpcomingDraftsQuery
    {
      UserId = isAuthenticated ? User.GetUserId() : Guid.Empty,
      IsAdmin = isAdmin,
      IncludePatreon = includePatreon
    };

    var result = await Sender.Send(query, ct);

    await this.SendOkAsync(result, ct);
  }
}
