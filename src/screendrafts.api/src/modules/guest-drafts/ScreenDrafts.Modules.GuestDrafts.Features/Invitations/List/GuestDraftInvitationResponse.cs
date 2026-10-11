namespace ScreenDrafts.Modules.GuestDrafts.Features.Invitations.List;

internal sealed record GuestDraftInvitationResponse
{
  public string PublicId { get; init; } = default!;
  public string Email { get; init; } = default!;
  public string Name { get; init; } = default!;

  /// <summary>"Pending" or "Accepted".</summary>
  public string Status { get; init; } = default!;
  public DateTime SentOnUtc { get; init; }
  public bool CanResend { get; init; }
  public DateTime? ResendAvailableOnUtc { get; init; }
}
