namespace ScreenDrafts.Modules.Users.Domain.Bootstraps;

public interface IEmailBootstrapClaimRepository : IRepository
{
  Task<EmailBootstrapClaim?> GetByUserIdAsync(
    UserId userId,
    CancellationToken cancellationToken = default
  );
  Task<IReadOnlyList<EmailBootstrapClaim>> GetUnclaimedAsync(
    CancellationToken cancellationToken = default
  );
  void Add(EmailBootstrapClaim claim);
  void Update(EmailBootstrapClaim claim);
}
