using ScreenDrafts.Modules.Drafts.Domain.DraftParts.PodcastLinks;

namespace ScreenDrafts.Modules.Drafts.UnitTests.DraftParts.PodcastLinks;

public class PodcastLinkRetryPolicyTests
{
  private static readonly DateTime _now = new(2026, 1, 15, 12, 0, 0, DateTimeKind.Utc);
  private static readonly DateOnly _today = DateOnly.FromDateTime(_now);

  [Fact]
  public void Next_ShouldRetryHourly_ForFreshRelease()
  {
    var decision = PodcastLinkRetryPolicy.Next(_today, attempts: 0, _now);

    decision.GiveUp.Should().BeFalse();
    decision.NextCheckAtUtc.Should().Be(_now.AddHours(1));
    decision.CountsAsAttempt.Should().BeTrue();
  }

  [Fact]
  public void Next_ShouldRetryDaily_AfterHourlyAttemptsRunOut()
  {
    var decision = PodcastLinkRetryPolicy.Next(
      _today.AddDays(-3),
      PodcastLinkRetryPolicy.HourlyAttempts,
      _now
    );

    decision.NextCheckAtUtc.Should().Be(_now.AddDays(1));
  }

  [Fact]
  public void Next_ShouldStillRetry_AtSevenDays()
  {
    var decision = PodcastLinkRetryPolicy.Next(_today.AddDays(-7), attempts: 30, _now);

    decision.GiveUp.Should().BeFalse();
  }

  [Fact]
  public void Next_ShouldGiveUp_AtEightDays()
  {
    var decision = PodcastLinkRetryPolicy.Next(_today.AddDays(-8), attempts: 0, _now);

    decision.GiveUp.Should().BeTrue();
  }

  [Fact]
  public void Next_ShouldWaitForReleaseDate_WithoutCountingAttempt_WhenDateIsInFuture()
  {
    var release = _today.AddDays(5);

    var decision = PodcastLinkRetryPolicy.Next(release, attempts: 0, _now);

    decision.GiveUp.Should().BeFalse();
    decision.CountsAsAttempt.Should().BeFalse();
    decision.NextCheckAtUtc.Should().Be(release.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc));
  }
}
