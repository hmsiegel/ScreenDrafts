namespace ScreenDrafts.Modules.Users.Infrastructure.Users;

internal sealed class UserRepository(UsersDbContext dbContext) : IUserRepository
{
  private readonly UsersDbContext _dbContext = dbContext;

  public void Add(User user)
  {
    _dbContext.Users.Add(user);
  }

  public void Update(User user)
  {
    _dbContext.Users.Update(user);
  }

  public async Task<User?> GetAsync(UserId id, CancellationToken cancellationToken = default)
  {
    return await _dbContext.Users
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

  public Task<User?> GetByPublicIdAsync(string publicId, CancellationToken cancellationToken = default)
  {
    return _dbContext.Users.SingleOrDefaultAsync(x => x.PublicId == publicId, cancellationToken);
  }

  public Task<User?> GetByIdentityIdAsync(string identityId, CancellationToken cancellationToken = default)
  {
    return _dbContext.Users.SingleOrDefaultAsync(x => x.IdentityId == identityId, cancellationToken);
  }
}
