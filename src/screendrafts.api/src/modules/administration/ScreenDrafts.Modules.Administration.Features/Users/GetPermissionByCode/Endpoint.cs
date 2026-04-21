namespace ScreenDrafts.Modules.Administration.Features.Users.GetPermissionByCode;

internal sealed class Endpoint : ScreenDraftsEndpoint<GetPermissionByCodeRequest, PermissionResponse>
{
  public override void Configure()
  {
    Get(UserRoutes.PermissionByCode);
    Description(x =>
    {
      x.WithTags(AdministrationOpenApi.Tags.Administration)
      .WithName(AdministrationOpenApi.Names.Administration_GetPermissionsByCode)
      .Produces<PermissionResponse>(StatusCodes.Status200OK)
      .Produces(StatusCodes.Status400BadRequest)
      .Produces(StatusCodes.Status401Unauthorized)
      .Produces(StatusCodes.Status403Forbidden)
      .Produces(StatusCodes.Status404NotFound);
    });
    Policies(AdministrationAuth.Permissions.PermissionsRead);
  }

  public override async Task HandleAsync(GetPermissionByCodeRequest req, CancellationToken ct)
  {
    var query = new GetPermissionByCodeQuery
    {
      Code = req.Code
    };

    var result = await Sender.Send(query, ct);

    await this.SendOkAsync(result, ct);
  }
}


