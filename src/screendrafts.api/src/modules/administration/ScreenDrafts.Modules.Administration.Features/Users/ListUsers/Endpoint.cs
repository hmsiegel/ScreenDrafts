namespace ScreenDrafts.Modules.Administration.Features.Users.ListUsers;

internal sealed class Endpoint : ScreenDraftsEndpoint<ListUsersRequest, PagedResult<UserItem>>
{
  private const int DefaultPage = 1;
  private const int DefaultPageSize = 25;
  private const int MaxPageSize = 100;

  public override void Configure()
  {
    Get(UserRoutes.UsersBase);
    Description(x =>
    {
      x.WithTags(AdministrationOpenApi.Tags.Administration)
        .WithName(AdministrationOpenApi.Names.Administration_ListUsers)
        .Produces<PagedResult<UserItem>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden);
    });
    Policies(AdministrationAuth.Permissions.UserRead);
  }

  public override async Task HandleAsync(ListUsersRequest req, CancellationToken ct)
  {
    var page = req.Page is > 0 ? req.Page : DefaultPage;
    var pageSize = req.PageSize is > 0 ? Math.Min(req.PageSize, MaxPageSize) : DefaultPageSize;

    var query = new ListUsersQuery
    {
      Search = req.Search,
      Page = page,
      PageSize = pageSize,
    };

    var result = await Sender.Send(query, ct);

    await this.SendOkAsync(result, ct);
  }
}
