namespace ScreenDrafts.Modules.Users.Domain.Users;

public interface IUserRepository
{
  Task<User?> GetAsync(UserId id, CancellationToken cancellationToken = default);

  Task<bool> IsEmailUniqueAsync(Email email, CancellationToken cancellationToken = default);

  Task<User?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default);

  void Add(User user);

  void Update(User user);
}
