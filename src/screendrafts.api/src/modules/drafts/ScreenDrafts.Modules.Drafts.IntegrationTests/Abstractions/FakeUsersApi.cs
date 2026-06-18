using System.Collections.Concurrent;

using ScreenDrafts.Modules.Users.PublicApi;

namespace ScreenDrafts.Modules.Drafts.IntegrationTests.Abstractions;

/// <summary>
/// In-memory replacement for <see cref="IUsersApi"/> used by Drafts integration
/// tests. The real implementation lives in the Users module and would require
/// seeding actual Keycloak/User rows just to exercise the User -> Person -> Host
/// resolution chain in handlers like RevealPick. This fake lets a test register
/// a UserPublicId -> UserId mapping directly and skips the cross-module hop.
///
/// Registered as a singleton in DraftsIntegrationTestWebAppFactory so every test
/// in the collection shares one instance — call Reset() between tests if a test
/// class doesn't already truncate/reset DI-scoped fakes itself.
/// </summary>
public sealed class FakeUsersApi : IUsersApi
{
  private readonly ConcurrentDictionary<string, UserResponse> _usersByPublicId = new();
  private readonly ConcurrentDictionary<Guid, UserResponse> _usersById = new();

  /// <summary>
  /// Registers a fake user and returns the UserPublicId so the test can pass
  /// it straight into a command (e.g. RevealPickCommand.UserPublicId).
  /// </summary>
  public string RegisterUser(Guid userId, string userPublicId, string firstName = "Test", string lastName = "User")
  {
    var response = new UserResponse
    {
      UserId = userId,
      FirstName = firstName,
      LastName = lastName,
      MiddleName = null
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

  public Task<UserResponse?> GetUserById(Guid userId, CancellationToken cancellationToken)
  {
    _usersById.TryGetValue(userId, out var user);
    return Task.FromResult(user);
  }

  public Task<UserResponse?> GetUserByPublicId(string publicId, CancellationToken cancellationToken)
  {
    _usersByPublicId.TryGetValue(publicId, out var user);
    return Task.FromResult(user);
  }

  public Task<IReadOnlyList<UserResponse>> GetAllUsersAsync(string? search, CancellationToken cancellationToken)
  {
    return Task.FromResult((IReadOnlyList<UserResponse>)[.. _usersById.Values]);
  }
}
