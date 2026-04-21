namespace ScreenDrafts.Modules.Administration.Features.Users.RemoveRoleFromUser;

internal sealed class Endpoint() : ScreenDraftsEndpoint<RemoveRoleFromUserRequest>
{
  public override void Configure()
  {
    Delete(UserRoutes.UserRoles);
    Policies(AdministrationAuth.Permissions.UserUpdate);
    Description(x =>
    {
      x.WithTags(AdministrationOpenApi.Tags.Administration)
      .WithName(AdministrationOpenApi.Names.Administration_RemoveUserRole)
      .Produces(StatusCodes.Status204NoContent)
      .Produces(StatusCodes.Status404NotFound)
      .Produces(StatusCodes.Status401Unauthorized);
    });
  }

  public override async Task HandleAsync(
    RemoveRoleFromUserRequest req,
    CancellationToken ct)
  {
    var command = new RemoveRoleFromUserCommand
    {
      UserPublicId = req.PublicId,
      RoleName = req.RoleName
    };

    var result = await Sender.Send(command, ct);

    await this.SendNoContentAsync(result, ct);
  }
}
