namespace ScreenDrafts.Modules.Users.PublicApi;
public interface IUsersApi
{
  Task<UserResponse?> GetUserById(Guid userId, CancellationToken cancellationToken);

  Task<UserSocialResponse?> GetUserSocialsAsync(string publicId, CancellationToken cancellationToken);

  Task<UserResponse?> GetUserByPublicId(string publicId, CancellationToken cancellationToken);
}
