using UserResponse = ScreenDrafts.Modules.Users.PublicApi.UserResponse;

namespace ScreenDrafts.Modules.Users.Infrastructure.PublicApi;
internal sealed class UsersApi(ISender sender) : IUsersApi
{
  private readonly ISender _sender = sender;

  public async Task<UserResponse?> GetUserByIdAsync(
    Guid userId,
    CancellationToken cancellationToken)
  {
    var query = new GetUserQuery(userId);

    var result = await _sender.Send(query, cancellationToken);

    if (result.IsFailure)
    {
      return null;
    }

    return new UserResponse(
      UserId: result.Value.UserId,
      FirstName: result.Value.FirstName,
      LastName: result.Value.LastName,
      MiddleName: result.Value.MiddleName);
  }

  public async Task<Result> AddUserRoleAsync(Guid userId, string Role)
  {
    var command = new AddUserRoleCommand(userId, Role);

    var result = await _sender.Send(command);

    if (result.IsFailure)
    {
      return Result.Failure(result.Errors[0]);
    }

    return Result.Success();
  }

  public async Task<Result> RemoveUserRoleAsync(Guid userId, string Role)
  {
    var command = new RemoveUserRoleCommand(userId, Role);

    var result = await _sender.Send(command);

    if (result.IsFailure)
    {
      return Result.Failure(result.Errors[0]);
    }

    return Result.Success();
  }

  public async Task<Result> AddPermissionToRoleAsync(string Role, string Permission)
  {
    var command = new AddPermissionToRoleCommand(Role, Permission);

    var result = await _sender.Send(command);

    if (result.IsFailure)
    {
      return Result.Failure(result.Errors[0]);
    }

    return Result.Success();
  }

  public async Task<IReadOnlyCollection<string>> GetUserRolesAsync(Guid userId, CancellationToken cancellationToken)
  {
    var query = new GetUserRolesQuery(userId);

    var result = await _sender.Send(query, cancellationToken);

    if (result.IsFailure)
    {
      return [];
    }

    return result.Value;
  }
}
