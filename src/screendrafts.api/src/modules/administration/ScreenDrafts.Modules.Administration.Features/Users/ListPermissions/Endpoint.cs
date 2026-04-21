namespace ScreenDrafts.Modules.Administration.Features.Users.ListPermissions;

internal sealed class Endpoint
: ScreenDraftsEndpointWithoutRequest<ListPermissionsResponse>
{
  public override void Configure()
  {
    Get(UserRoutes.Permissions);
    Description(x =>
    {
      x.WithName(AdministrationOpenApi.Names.Administration_GetPermissions)
      .WithTags(AdministrationOpenApi.Tags.Administration)
      .Produces<ListPermissionsResponse>(StatusCodes.Status200OK)
      .Produces(StatusCodes.Status401Unauthorized)
      .Produces(StatusCodes.Status403Forbidden);
    });
    Policies(AdministrationAuth.Permissions.PermissionsRead);
  }

  public override async Task HandleAsync(
    CancellationToken ct)
  {
    var query = new ListPermissionsQuery();

    var result = await Sender.Send(query, ct);

    await this.SendOkAsync(result, ct);
  }
}
