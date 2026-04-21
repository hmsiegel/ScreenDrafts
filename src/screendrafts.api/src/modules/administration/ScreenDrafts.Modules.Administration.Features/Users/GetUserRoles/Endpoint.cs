namespace ScreenDrafts.Modules.Administration.Features.Users.GetUserRoles;

internal sealed class Endpoint : ScreenDraftsEndpoint<GetUserRolesRequest, GetUserRolesResponse>
{
  public override void Configure()
  {
    Get(UserRoutes.UserRolesList);
    Description(x =>
    {
      x.WithName(AdministrationOpenApi.Names.Administration_GetUserRoles)
      .WithTags(AdministrationOpenApi.Tags.Administration)
      .Produces<GetUserRolesResponse>(StatusCodes.Status200OK)
      .Produces(StatusCodes.Status400BadRequest)
      .Produces(StatusCodes.Status401Unauthorized)
      .Produces(StatusCodes.Status403Forbidden)
      .Produces(StatusCodes.Status404NotFound);
    });
    Policies(AdministrationAuth.Permissions.UserRead);
  }

  public override async Task HandleAsync(GetUserRolesRequest req, CancellationToken ct)
  {
    var query = new GetUserRolesQuery { PublicId = req.PublicId };

    var result = await Sender.Send(query, ct);

    await this.SendOkAsync(result, ct);
  }
}
