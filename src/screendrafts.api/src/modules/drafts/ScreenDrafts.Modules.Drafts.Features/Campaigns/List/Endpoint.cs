using ScreenDrafts.Common.Infrastructure.Authentication;
using ScreenDrafts.Modules.Users.PublicApi;

namespace ScreenDrafts.Modules.Drafts.Features.Campaigns.List;

internal sealed class Endpoint(IUsersApi usersApi) : ScreenDraftsEndpoint<Request, CampaignCollectionResponse>
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
    Permissions(Features.Permissions.CampaignList);
  }

  public override async Task HandleAsync(Request req, CancellationToken ct)
  {
    var user = await _usersApi.GetUserByIdAsync(User.GetUserId(), ct);

    if (user is null)
    {
      await Send.ErrorsAsync(StatusCodes.Status403Forbidden, ct);
      return;
    }

    var userRoles = await _usersApi.GetUserRolesAsync(user.UserId, ct);
    var isAdmin = userRoles.Contains(Features.Roles.Admin, StringComparer.OrdinalIgnoreCase) || 
      userRoles.Contains(Features.Roles.SuperAdmin, StringComparer.OrdinalIgnoreCase);

    if (!isAdmin && req.IncludeDeleted)
    {
      await Send.ErrorsAsync(StatusCodes.Status403Forbidden, ct);
      return;
    }

    var query = new Query(IncludeDeleted: req.IncludeDeleted);

    var result = await Sender.Send(query, ct);

    await this.SendOkAsync(result, ct);
  }
}
