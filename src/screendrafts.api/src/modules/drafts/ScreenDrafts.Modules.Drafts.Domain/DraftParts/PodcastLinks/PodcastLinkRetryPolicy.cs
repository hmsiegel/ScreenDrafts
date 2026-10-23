namespace ScreenDrafts.Modules.Drafts.Domain.DraftParts.PodcastLinks;

/// <summary>
/// When to look again after finding no episode. Platforms lag the release
/// date, so: wait for a future release date; then hourly for the first
/// <see cref="HourlyAttempts"/> misses, daily after that; stop once the
/// release is more than <see cref="RetryWindowDays"/> days old. An old
/// release on its first lookup therefore gets one pass and stops.
/// </summary>
public static class PodcastLinkRetryPolicy
{
  public const int RetryWindowDays = 7;
  public const int HourlyAttempts = 24;

  public static RetryDecision Next(DateOnly releaseDate, int attempts, DateTime nowUtc)
  {
    var today = DateOnly.FromDateTime(nowUtc);

    if (releaseDate > today)
    {
      return RetryDecision.RetryAt(
        releaseDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
        countsAsAttempt: false
      );
    }

    if (today.DayNumber - releaseDate.DayNumber > RetryWindowDays)
    {
      return RetryDecision.Stop;
    }

    var wait = attempts < HourlyAttempts ? TimeSpan.FromHours(1) : TimeSpan.FromDays(1);

    return RetryDecision.RetryAt(nowUtc + wait, countsAsAttempt: true);
  }
}
