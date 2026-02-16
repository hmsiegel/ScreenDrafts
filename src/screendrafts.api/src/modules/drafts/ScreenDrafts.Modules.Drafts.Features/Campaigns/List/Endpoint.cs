using ScreenDrafts.Common.Presentation.Http.Authentication;

namespace ScreenDrafts.Modules.Drafts.Features.Campaigns.List;

internal sealed class Endpoint(IUsersApi usersApi) : ScreenDraftsEndpoint<ListCampaignsRequest, CampaignCollectionResponse>
{
  private readonly IUsersApi _usersApi = usersApi;

  public override void Configure()
  {
    Get(CampaignRoutes.Campaigns);
    Description(x =>
    {
      x.WithName(DraftsOpenApi.Names.Campaigns_ListCampaigns)
      .WithTags(DraftsOpenApi.Tags.Campaigns)
      .Produces<CampaignCollectionResponse>(StatusCodes.Status200OK)
      .Produces(StatusCodes.Status401Unauthorized)
      .Produces(StatusCodes.Status403Forbidden);
    });
    Policies(DraftsAuth.Permissions.CampaignList);
  }

  public override async Task HandleAsync(ListCampaignsRequest req, CancellationToken ct)
  {
    var userRoles = await _usersApi.GetUserRolesAsync(User.GetUserId(), ct);

    var isAdmin = userRoles.Contains(DraftsAuth.Roles.Admin, StringComparer.OrdinalIgnoreCase) || 
      userRoles.Contains(DraftsAuth.Roles.SuperAdmin, StringComparer.OrdinalIgnoreCase);

    if (!isAdmin && req.IncludeDeleted)
    {
      await Send.ErrorsAsync(StatusCodes.Status403Forbidden, ct);
      return;
    }

    var ListCampaignsQuery = new ListCampaignsQuery(IncludeDeleted: req.IncludeDeleted);

    var result = await Sender.Send(ListCampaignsQuery, ct);

    await this.SendOkAsync(result, ct);
  }
}


