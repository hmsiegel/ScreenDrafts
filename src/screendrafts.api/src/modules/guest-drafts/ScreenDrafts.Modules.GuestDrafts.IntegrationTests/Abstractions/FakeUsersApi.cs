namespace ScreenDrafts.Modules.GuestDrafts.IntegrationTests.Abstractions;

/// <summary>
/// In-memory replacement for <see cref="IUsersApi"/> used by GuestDrafts integration
/// tests. GuestDrafts calls into IUsersApi synchronously on nearly every command
/// (resolving UserPublicId -> UserId), so seeding real Keycloak/User rows for every
/// test would be excessive. This fake lets a test register a UserPublicId -> UserId
/// mapping directly and skips the cross-module hop entirely.
///
/// Mirrors ScreenDrafts.Modules.Drafts.IntegrationTests.Abstractions.FakeUsersApi
/// exactly. Registered as a singleton in GuestDraftsIntegrationTestWebAppFactory so
/// every test in the collection shares one instance.
/// </summary>
public sealed class FakeUsersApi : IUsersApi
{
  private readonly ConcurrentDictionary<string, UserPublicApiResponse> _usersByPublicId = new();
  private readonly ConcurrentDictionary<Guid, UserPublicApiResponse> _usersById = new();

  /// <summary>
  /// Registers a fake user and returns the UserPublicId so the test can pass
  /// it straight into a command (e.g. CreateGuestDraftCommand.OwnerUserPublicId).
  /// </summary>
  public string RegisterUser(
    Guid userId,
    string userPublicId,
    string firstName = "Test",
    string lastName = "User"
  )
  {
    var response = new UserPublicApiResponse
    {
      UserId = userId,
      FirstName = firstName,
      LastName = lastName,
      MiddleName = null,
    };

    _usersByPublicId[userPublicId] = response;
    _usersById[userId] = response;

    return userPublicId;
  }

  public void Reset()
  {
    _usersByPublicId.Clear();
    _usersById.Clear();
  }

  public Task<UserPublicApiResponse?> GetUserById(Guid userId, CancellationToken cancellationToken)
  {
    _usersById.TryGetValue(userId, out var user);
    return Task.FromResult(user);
  }

  public Task<UserPublicApiResponse?> GetUserByPublicId(
    string publicId,
    CancellationToken cancellationToken
  )
  {
    _usersByPublicId.TryGetValue(publicId, out var user);
    return Task.FromResult(user);
  }

  public Task<IReadOnlyList<UserPublicApiResponse>> GetAllUsersAsync(
    string? search,
    CancellationToken cancellationToken
  )
  {
    return Task.FromResult((IReadOnlyList<UserPublicApiResponse>)[.. _usersById.Values]);
  }

  public Task<IReadOnlyList<UserPublicApiResponse>> GetUsersByIds(
    IReadOnlyList<Guid> userIds,
    CancellationToken cancellationToken
  )
  {
    var users = userIds
      .Select(id => _usersById.TryGetValue(id, out var user) ? user : null)
      .Where(user => user != null)
      .ToList();
    return Task.FromResult((IReadOnlyList<UserPublicApiResponse>)users);
  }
}
