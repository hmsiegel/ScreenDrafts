namespace ScreenDrafts.Modules.Administration.Presentation.Users;

internal sealed class AddUserRole(IUsersApi usersApi) : Endpoint<AddUserRoleRequest>
{
  private readonly IUsersApi _usersApi = usersApi;

  public override void Configure()
  {
    Post("administation/users/{userId:guid}/roles");
    Policies(Presentation.Permissions.ModifyUser);
    Description(x => x.WithTags(Presentation.Tags.Administration));
  }

  public override async Task HandleAsync(AddUserRoleRequest req, CancellationToken ct)
  {
    var userId = Route<Guid>("userId");

    var result = await _usersApi.AddUserRoleAsync(userId, req.Role);

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

public record AddUserRoleRequest(string Role);
