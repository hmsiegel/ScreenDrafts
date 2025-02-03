namespace ScreenDrafts.Modules.Users.PublicApi;
public interface IUsersApi
{
  Task<UserResponse?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken);
}
