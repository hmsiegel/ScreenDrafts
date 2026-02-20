using ScreenDrafts.Common.Presentation.Http.Authentication;

namespace ScreenDrafts.Modules.Drafts.Features.Campaigns.Get;

internal sealed class Endpoint(IUsersApi usersApi) : ScreenDraftsEndpoint<GetCampaignRequest, CampaignResponse>
{
  private readonly IUsersApi _usersApi = usersApi;

  public override void Configure()
  {
    Get(CampaignRoutes.ById);
    Description(x =>
    {
      x.WithName(DraftsOpenApi.Names.Campaigns_GetCampaignById)
      .WithTags(DraftsOpenApi.Tags.Campaigns)
      .Produces<CampaignResponse>(StatusCodes.Status200OK)
      .Produces(StatusCodes.Status401Unauthorized)
      .Produces(StatusCodes.Status403Forbidden);
    });
    Policies(DraftsAuth.Permissions.CampaignRead);
  }

  public override async Task HandleAsync(GetCampaignRequest req, CancellationToken ct)
  {
    var userRoles = await _usersApi.GetUserRolesAsync(User.GetUserId(), ct);

    var isAdmin = userRoles.Contains(DraftsAuth.Roles.Admin, StringComparer.OrdinalIgnoreCase) || 
      userRoles.Contains(DraftsAuth.Roles.SuperAdmin, StringComparer.OrdinalIgnoreCase);

    if (!isAdmin && req.IncludeDeleted)
    {
      await Send.ErrorsAsync(StatusCodes.Status403Forbidden, ct);
      return;
    }

    var GetCampaignQuery = new GetCampaignQuery(req.PublicId, req.IncludeDeleted);

    var result = await Sender.Send(GetCampaignQuery, ct);

    await this.SendOkAsync(result, ct);
  }
}


