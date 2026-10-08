namespace ScreenDrafts.Modules.Communications.Domain.Email;

public interface IEmailService
{
  /// <summary>
  /// When true, placeholder (@screendrafts.fake) recipients are sent to like any other.
  /// Development only, to exercise email flows against a capture server. Leave unset in
  /// production: the default is false, so placeholder addresses are never emailed.
  /// </summary>
  public bool AllowPlaceholderRecipients { get; }
  Task SendAsync(EmailMessage emailMessage, CancellationToken cancellationToken = default);
}
