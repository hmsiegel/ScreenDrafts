namespace ScreenDrafts.Modules.Users.Infrastructure.Users;

internal sealed class EmailChangeTokenRepository(UsersDbContext dbContext)
  : IEmailChangeTokenRepository
{
  private readonly UsersDbContext _dbContext = dbContext;

  public async Task<EmailChangeToken?> GetActiveByUserIdAsync(
    UserId userId,
    CancellationToken cancellationToken = default
  ) =>
    await _dbContext
      .EmailChangeTokens.Where(x =>
        x.UserId == userId && x.UsedAt == null && x.ExpiresAt > DateTimeOffset.UtcNow
      )
      .OrderByDescending(x => x.IssuedAt)
      .FirstOrDefaultAsync(cancellationToken);

  public async Task<EmailChangeToken?> GetByTokenHashAsync(
    string tokenHash,
    CancellationToken cancellationToken = default
  ) =>
    await _dbContext.EmailChangeTokens.SingleOrDefaultAsync(
      x => x.TokenHash == tokenHash,
      cancellationToken
    );

  public void Add(EmailChangeToken token) => _dbContext.EmailChangeTokens.Add(token);

  public void Update(EmailChangeToken token) => _dbContext.EmailChangeTokens.Update(token);
}
