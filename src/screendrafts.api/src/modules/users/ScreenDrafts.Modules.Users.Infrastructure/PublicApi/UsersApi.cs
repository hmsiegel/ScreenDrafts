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
}
