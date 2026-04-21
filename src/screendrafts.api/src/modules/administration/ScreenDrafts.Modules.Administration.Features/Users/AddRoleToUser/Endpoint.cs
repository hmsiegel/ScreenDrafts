namespace ScreenDrafts.Modules.Administration.Features.Users.AddRoleToUser;

internal sealed class Endpoint() : ScreenDraftsEndpoint<AddRoleToUserRequest>
{
  public override void Configure()
  {
    Post(UserRoutes.UserRoles);
    Description(x =>
    {
      x.WithTags(AdministrationOpenApi.Tags.Administration)
      .WithName(AdministrationOpenApi.Names.Administration_AddUserRole)
      .Produces(StatusCodes.Status204NoContent)
      .Produces(StatusCodes.Status400BadRequest)
      .Produces(StatusCodes.Status401Unauthorized)
      .Produces(StatusCodes.Status403Forbidden);
    });
    Policies(AdministrationAuth.Permissions.UserUpdate);
  }

  public override async Task HandleAsync(AddRoleToUserRequest req, CancellationToken ct)
  {
    var command = new AddRoleToUserCommand
    {
      UserPublicId = req.PublicId,
      RoleName = req.RoleName
    };

    var result = await Sender.Send(command, ct);

    await this.SendNoContentAsync(result, ct);
  }
}
