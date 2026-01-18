using UserResponse = ScreenDrafts.Modules.Users.PublicApi.UserResponse;

namespace ScreenDrafts.Modules.Users.Infrastructure.PublicApi;

internal sealed class UsersApi(ISender sender) : IUsersApi
{
  private readonly ISender _sender = sender;

  public async Task<UserResponse?> GetUserById(
    Guid userId,
    CancellationToken cancellationToken)
  {
    var query = new Features.Users.GetByUserId.Query(userId);

    var result = await _sender.Send(query, cancellationToken: cancellationToken);

    if (result.IsFailure)
    {
      return null;
    }

    return new UserResponse
    {
      UserId = result.Value.UserId,
      FirstName = result.Value.FirstName,
      LastName = result.Value.LastName,
      MiddleName = result.Value.MiddleName
    };
  }

  public async Task<Result> AddUserRoleAsync(Guid userId, string role)
  {
    var command = new Features.Admin.AddRoleToUser.Command(userId, role);

    var result = await _sender.Send(command);

    if (result.IsFailure)
    {
      return Result.Failure(result.Errors[0]);
    }

    return Result.Success();
  }

  public async Task<Result> RemoveUserRoleAsync(Guid userId, string role)
  {
    var command = new Features.Admin.RemoveRoleFromUser.Command(userId, role);

    var result = await _sender.Send(command);

    if (result.IsFailure)
    {
      return Result.Failure(result.Errors[0]);
    }

    return Result.Success();
  }

  public async Task<Result> AddPermissionToRoleAsync(string role, string permission)
  {
    var command = new Features.Admin.AddPermissionToRole.Command(role, permission);

    var result = await _sender.Send(command);

    if (result.IsFailure)
    {
      return Result.Failure(result.Errors[0]);
    }

    return Result.Success();
  }

  public async Task<IReadOnlyCollection<string>> GetUserRolesAsync(Guid userId, CancellationToken cancellationToken)
  {
    var query = new Features.Admin.GetUserRoles.Query(userId);

    var result = await _sender.Send(query, cancellationToken);

    if (result.IsFailure)
    {
      return [];
    }

    return result.Value;
  }

  public async Task<UserSocialResponse?> GetUserSocialsAsync(string publicId, CancellationToken cancellationToken)
  {
    var query = new Features.Users.GetUserSocials.Query(publicId);

    var result = await _sender.Send(query, cancellationToken);

    if (result.IsFailure)
    {
      return null;
    }

    return new UserSocialResponse(
      PublicId: publicId,
      Twitter: result.Value?.Twitter,
      Instagram: result.Value?.Instagram,
      Letterboxd: result.Value?.Letterboxd,
      Bluesky: result.Value?.Bluesky,
      ProfilePicturePath: result.Value?.ProfilePicturePath);
  }
}
