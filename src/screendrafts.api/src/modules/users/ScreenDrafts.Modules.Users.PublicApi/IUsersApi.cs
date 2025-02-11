using ScreenDrafts.Common.Domain;

namespace ScreenDrafts.Modules.Users.PublicApi;
public interface IUsersApi
{
  Task<UserResponse?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken);

  Task<Result> AddUserRoleAsync(Guid userId, string Role);

  Task<Result> RemoveUserRoleAsync(Guid userId, string Role);
}
