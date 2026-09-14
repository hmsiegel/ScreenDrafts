namespace ScreenDrafts.Modules.Users.Domain.Users;

public interface IUserRepository : IRepository
{
  Task<User?> GetAsync(UserId id, CancellationToken cancellationToken = default);
  Task<bool> IsEmailUniqueAsync(Email email, CancellationToken cancellationToken = default);
  Task<User?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default);
  Task<User?> GetByPublicIdAsync(string publicId, CancellationToken cancellationToken = default);
  Task<User?> GetByIdentityIdAsync(
    string identityId,
    CancellationToken cancellationToken = default
  );

  /// <summary>
  /// Every user in the module. Only used by the bootstrap-token batch generator
  /// ("everyone unclaimed") — not a general-purpose listing method, so don't
  /// reach for this in query handlers; Dapper is still the right tool for
  /// paginated/filtered reads.
  /// </summary>
  Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken = default);
  void Add(User user);
  void Update(User user);
}
