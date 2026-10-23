namespace ScreenDrafts.Modules.Drafts.Domain.DraftParts.PodcastLinks;

public sealed record RetryDecision(bool GiveUp, DateTime? NextCheckAtUtc, bool CountsAsAttempt)
{
  public static readonly RetryDecision Stop = new(true, null, false);

  public static RetryDecision RetryAt(DateTime nextCheckAtUtc, bool countsAsAttempt) =>
    new(false, nextCheckAtUtc, countsAsAttempt);
}
