namespace ScreenDrafts.Modules.Users.Infrastructure.Users;
internal sealed class UserRepository(UsersDbContext dbContext) : IUserRepository
{
  private readonly UsersDbContext _dbContext = dbContext;

  public void Add(User user)
  {
    foreach (var role in user.Roles)
    {
      _dbContext.Attach(role);
    }

    _dbContext.Users.Add(user);
  }

  public async Task<User?> GetAsync(UserId id, CancellationToken cancellationToken = default)
  {
    return await _dbContext.Users
      .Include(x => x.Roles)
      .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
  }

  public async Task<User?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default)
  {
    return await _dbContext.Users.SingleOrDefaultAsync(x => x.Email == email, cancellationToken);
  }

  public async Task<bool> IsEmailUniqueAsync(Email email, CancellationToken cancellationToken = default)
  {
    return !await _dbContext.Users.AnyAsync(x => x.Email == email, cancellationToken);
  }
}
