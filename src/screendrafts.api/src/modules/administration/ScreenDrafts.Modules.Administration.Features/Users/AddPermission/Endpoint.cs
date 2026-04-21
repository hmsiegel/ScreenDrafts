namespace ScreenDrafts.Modules.Administration.Features.Users.AddPermission;

internal sealed class Endpoint : ScreenDraftsEndpoint<AddPermissionRequest>
{
  public override void Configure()
  {
    Post(UserRoutes.Permissions);
    Description(x =>
    {
      x.WithTags(AdministrationOpenApi.Tags.Administration)
      .WithName(AdministrationOpenApi.Names.Administration_AddPermission)
      .Produces(StatusCodes.Status204NoContent)
      .Produces(StatusCodes.Status400BadRequest)
      .Produces(StatusCodes.Status401Unauthorized)
      .Produces(StatusCodes.Status403Forbidden);
    });
    Policies(AdministrationAuth.Permissions.PermissionsUpdate);
  }

  public override async Task HandleAsync(AddPermissionRequest req, CancellationToken ct)
  {
    var command = new AddPermissionCommand { Code = req.Code };

    var result = await Sender.Send(command, ct);

    await this.SendNoContentAsync(result, ct);
  }
}
