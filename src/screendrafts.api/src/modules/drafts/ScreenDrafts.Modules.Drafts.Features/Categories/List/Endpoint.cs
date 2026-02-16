using ScreenDrafts.Common.Presentation.Http.Authentication;

namespace ScreenDrafts.Modules.Drafts.Features.Categories.List;

internal sealed class Endpoint(IUsersApi usersApi) : ScreenDraftsEndpoint<ListCategoriesRequest, CategoryCollectionResponse>
{
  private readonly IUsersApi _usersApi = usersApi;

  public override void Configure()
  {
    Get(CategoryRoutes.Category);
    Description(x =>
    {
      x.WithName(DraftsOpenApi.Names.Categories_ListCategories)
      .WithTags(DraftsOpenApi.Tags.Categories)
      .Produces<CategoryCollectionResponse>(StatusCodes.Status200OK)
      .Produces(StatusCodes.Status401Unauthorized)
      .Produces(StatusCodes.Status403Forbidden);
    });
    Permissions(DraftsAuth.Permissions.CampaignList);
  }

  public override async Task HandleAsync(ListCategoriesRequest req, CancellationToken ct)
  {
    var userRoles = await _usersApi.GetUserRolesAsync(User.GetUserId(), ct);

    var isAdmin = userRoles.Contains(DraftsAuth.Roles.Admin, StringComparer.OrdinalIgnoreCase) || 
      userRoles.Contains(DraftsAuth.Roles.SuperAdmin, StringComparer.OrdinalIgnoreCase);

    if (!isAdmin && req.IncludeDeleted)
    {
      await Send.ErrorsAsync(StatusCodes.Status403Forbidden, ct);
      return;
    }

    var ListCategoriesQuery = new ListCategoriesQuery(IncludeDeleted: req.IncludeDeleted);

    var result = await Sender.Send(ListCategoriesQuery, ct);

    await this.SendOkAsync(result, ct);
  }
}



