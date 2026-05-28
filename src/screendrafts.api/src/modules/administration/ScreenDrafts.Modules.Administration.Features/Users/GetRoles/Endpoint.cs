namespace ScreenDrafts.Modules.Administration.Features.Users.GetRoles;

internal sealed class Endpoint : ScreenDraftsEndpointWithoutRequest<GetRolesResponse>
{
  public override void Configure()
  {
    Get(UserRoutes.RolesBase);
    Description(x =>
    {
      x.WithName(AdministrationOpenApi.Names.Administration_ListRoles)
        .WithTags(AdministrationOpenApi.Tags.Administration)
        .Produces<GetRolesResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden);
    });
    Policies(AdministrationAuth.Permissions.UserRead);
  }

  public override async Task HandleAsync(CancellationToken ct)
  {
    var result = await Sender.Send(new GetRolesQuery(), ct);
    await this.SendOkAsync(result, ct);
  }
}
