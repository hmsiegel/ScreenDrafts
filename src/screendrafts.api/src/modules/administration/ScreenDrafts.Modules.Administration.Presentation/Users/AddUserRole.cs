using ScreenDrafts.Common.Presentation.Results;

namespace ScreenDrafts.Modules.Administration.Presentation.Users;

internal sealed class AddUserRole(IUsersApi usersApi) : Endpoint<AddUserRoleRequest>
{
  private readonly IUsersApi _usersApi = usersApi;

  public override void Configure()
  {
    Post("administration/users/{userId:guid}/roles");
    Policies(Presentation.Permissions.ModifyUser);
    Description(x =>
    {
      x.WithTags(Presentation.Tags.Administration)
      .WithName(nameof(AddUserRole))
      .WithDescription("Add user role");
    });
  }

  public override async Task HandleAsync(AddUserRoleRequest req, CancellationToken ct)
  {
    var result = await _usersApi.AddUserRoleAsync(req.UserId, req.Role);

    await this.SendNoContentAsync(result, ct);
  }
}

public record AddUserRoleRequest(
  string Role,
  [FromRoute(Name = "userId")] Guid UserId);

internal sealed class AddUserRoleSummary : Summary<AddUserRole>
{
  public AddUserRoleSummary()
  {
    Summary = "Add user role";
    Description = "Add user role";
    Response<ErrorResponse>(StatusCodes.Status400BadRequest, "Bad request");
    Response<ErrorResponse>(StatusCodes.Status403Forbidden, "Forbidden");
    Response<ErrorResponse>(StatusCodes.Status404NotFound, "Not found");
    Response<ErrorResponse>(StatusCodes.Status500InternalServerError, "Internal server error");
  }
}
