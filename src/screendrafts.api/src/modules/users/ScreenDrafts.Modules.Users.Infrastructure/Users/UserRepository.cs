namespace ScreenDrafts.Modules.Users.Infrastructure.Users;
internal sealed class UserRepository(UsersDbContext dbContext) : IUserRepository
{
  private readonly UsersDbContext _dbContext = dbContext;

  public void Add(User user)
  {
    _dbContext.Users.Add(user);
  }

  public async Task<User?> GetAsync(Guid id, CancellationToken cancellationToken = default)
  {
    return await _dbContext.Users.SingleOrDefaultAsync(x => x.Id.Value == id, cancellationToken);
  }

  public async Task<User?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default)
  {
    return await _dbContext.Users.SingleOrDefaultAsync(x => x.Email == email, cancellationToken);
  }

  public async Task<bool> IsEmailUniqueAsynk(Email email, CancellationToken cancellationToken = default)
  {
    return !await _dbContext.Users.AnyAsync(x => x.Email == email, cancellationToken);
  }

  public void Update(User user)
  {
    _dbContext.Users.Update(user);
  }
}
