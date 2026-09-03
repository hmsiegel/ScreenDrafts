namespace ScreenDrafts.Modules.Users.PublicApi;

public interface IUsersApi
{
  Task<UserPublicApiResponse?> GetUserById(Guid userId, CancellationToken cancellationToken);
  Task<UserPublicApiResponse?> GetUserByPublicId(
    string publicId,
    CancellationToken cancellationToken
  );
  Task<IReadOnlyList<UserPublicApiResponse>> GetAllUsersAsync(
    string? search,
    CancellationToken cancellationToken
  );
  Task<IReadOnlyList<UserPublicApiResponse>> GetUsersByIds(
    IReadOnlyList<Guid> userIds,
    CancellationToken cancellationToken
  );
}
