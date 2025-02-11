namespace ScreenDrafts.Modules.Administration.Presentation.Users;

internal sealed class RemoveUserRole(IUsersApi usersApi) : Endpoint<RemoveUserRoleRequest>
{
  private readonly IUsersApi _usersApi = usersApi;

  public override void Configure()
  {
    Delete("administation/users/{userId:guid}/roles");
    Policies(Presentation.Permissions.ModifyUser);
    Description(x => x.WithTags(Presentation.Tags.Administration));
  }

  public override async Task HandleAsync(RemoveUserRoleRequest req, CancellationToken ct)
  {
    var userId = Route<Guid>("userId");

    var result = await _usersApi.RemoveUserRoleAsync(userId, req.Role);

    if (result.IsFailure)
    {
      await SendErrorsAsync(StatusCodes.Status400BadRequest, ct);
    }
    else
    {
      await SendOkAsync(StatusCodes.Status200OK, ct);
    }
  }
}

public record RemoveUserRoleRequest(string Role);
