namespace ScreenDrafts.Modules.Users.Domain.EmailChange;

public interface IEmailChangeTokenRepository : IRepository
{
  /// <summary>The current unused, unexpired token for a user, if one exists.</summary>
  Task<EmailChangeToken?> GetActiveByUserIdAsync(
    UserId userId,
    CancellationToken cancellationToken = default
  );
  Task<EmailChangeToken?> GetByTokenHashAsync(
    string tokenHash,
    CancellationToken cancellationToken = default
  );
  void Add(EmailChangeToken token);
  void Update(EmailChangeToken token);
}
