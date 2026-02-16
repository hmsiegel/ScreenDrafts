using ScreenDrafts.Common.Abstractions.Results;

namespace ScreenDrafts.Modules.Users.PublicApi;
public interface IUsersApi
{
  Task<UserResponse?> GetUserById(Guid userId, CancellationToken cancellationToken);

  Task<Result> AddUserRoleAsync(Guid userId, string role);

  Task<Result> RemoveUserRoleAsync(Guid userId, string role);

  Task<Result> AddPermissionToRoleAsync(string role, string permission);

  Task<IReadOnlyCollection<string>> GetUserRolesAsync(Guid userId, CancellationToken cancellationToken);

  Task<UserSocialResponse?> GetUserSocialsAsync(string publicId, CancellationToken cancellationToken);

  Task<UserResponse?> GetUserByPublicId(string publicId, CancellationToken cancellationToken);
}
