namespace ScreenDrafts.Modules.Users.Infrastructure.Identity;

internal sealed class EmailBootstrapClaimRepository(UsersDbContext dbContext)
  : IEmailBootstrapClaimRepository
{
  private readonly UsersDbContext _dbContext = dbContext;

  public async Task<EmailBootstrapClaim?> GetByUserIdAsync(
    UserId userId,
    CancellationToken cancellationToken = default
  ) =>
    await _dbContext
      .Set<EmailBootstrapClaim>()
      .SingleOrDefaultAsync(x => x.UserId == userId, cancellationToken);

  public async Task<IReadOnlyList<EmailBootstrapClaim>> GetUnclaimedAsync(
    CancellationToken cancellationToken = default
  ) =>
    await _dbContext
      .Set<EmailBootstrapClaim>()
      .Where(x => x.ClaimedAt == null)
      .ToListAsync(cancellationToken);

  public void Add(EmailBootstrapClaim claim) => _dbContext.Set<EmailBootstrapClaim>().Add(claim);

  public void Update(EmailBootstrapClaim claim) =>
    _dbContext.Set<EmailBootstrapClaim>().Update(claim);
}
