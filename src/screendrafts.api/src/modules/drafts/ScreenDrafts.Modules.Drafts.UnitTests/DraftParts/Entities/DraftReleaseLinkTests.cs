namespace ScreenDrafts.Modules.Drafts.UnitTests.DraftParts.Entities;

public class DraftReleaseLinkTests : DraftsBaseTest
{
  private static readonly DateOnly _date = new(2026, 1, 15);

  [Fact]
  public void Create_ShouldAddPendingLinkPerPlatform_WhenChannelIsMainFeed()
  {
    var part = CreateDraftPart();

    var release = DraftRelease.Create(part.Id, ReleaseChannel.MainFeed, _date).Value;

    release.Links.Should().HaveCount(2);
    release.Links.Select(l => l.Platform).Should().BeEquivalentTo(ReleaseLinkPlatform.List);
    release.Links.Should().OnlyContain(l => l.Status == ReleaseLinkStatus.Pending);
  }

  [Fact]
  public void Create_ShouldAddNoLinks_WhenChannelIsPatreon()
  {
    var part = CreateDraftPart();

    var release = DraftRelease.Create(part.Id, ReleaseChannel.Patreon, _date).Value;

    release.Links.Should().BeEmpty();
  }

  [Fact]
  public void MarkLinked_ShouldLink_WhenUrlIsValidSpotifyEpisode()
  {
    var link = CreateLink(ReleaseLinkPlatform.Spotify);

    var result = link.MarkLinked(
      new Uri("https://open.spotify.com/episode/abc123"),
      DateTime.UtcNow
    );

    result.IsSuccess.Should().BeTrue();
    link.Status.Should().Be(ReleaseLinkStatus.Linked);
    link.Url.Should().Be(new Uri("https://open.spotify.com/episode/abc123"));
    link.NextCheckAtUtc.Should().BeNull();
  }

  [Fact]
  public void MarkLinked_ShouldLink_WhenUrlIsValidApple()
  {
    var link = CreateLink(ReleaseLinkPlatform.Apple);

    var result = link.MarkLinked(
      new Uri("https://podcasts.apple.com/us/podcast/x/id1431963108?i=1000"),
      DateTime.UtcNow
    );

    result.IsSuccess.Should().BeTrue();
  }

  [Theory]
  [InlineData("http://open.spotify.com/episode/abc123")]
  [InlineData("https://open.spotify.com/show/abc123")]
  [InlineData("https://evil.example.com/episode/abc123")]
#pragma warning disable CA1054 // URI-like parameters should not be strings
  public void MarkLinked_ShouldFail_WhenSpotifyUrlIsInvalid(string url)
#pragma warning restore CA1054 // URI-like parameters should not be strings
  {
    ArgumentNullException.ThrowIfNull(url);
    var link = CreateLink(ReleaseLinkPlatform.Spotify);

    var result = link.MarkLinked(new Uri(url), DateTime.UtcNow);

    result.IsFailure.Should().BeTrue();
    link.Status.Should().Be(ReleaseLinkStatus.Pending);
  }

  [Fact]
  public void ScheduleRetry_ShouldCountAttempt_WhenFlagged()
  {
    var link = CreateLink(ReleaseLinkPlatform.Spotify);
    var next = DateTime.UtcNow.AddHours(1);

    link.ScheduleRetry(next, countsAsAttempt: true, DateTime.UtcNow);

    link.Attempts.Should().Be(1);
    link.NextCheckAtUtc.Should().Be(next);
    link.Status.Should().Be(ReleaseLinkStatus.Pending);
  }

  [Fact]
  public void ScheduleRetry_ShouldNotCountAttempt_WhenNotFlagged()
  {
    var link = CreateLink(ReleaseLinkPlatform.Spotify);

    link.ScheduleRetry(DateTime.UtcNow.AddDays(3), countsAsAttempt: false, DateTime.UtcNow);

    link.Attempts.Should().Be(0);
  }

  [Fact]
  public void MarkNotFound_ShouldStopChecking()
  {
    var link = CreateLink(ReleaseLinkPlatform.Apple);

    link.MarkNotFound(DateTime.UtcNow);

    link.Status.Should().Be(ReleaseLinkStatus.NotFound);
    link.NextCheckAtUtc.Should().BeNull();
  }

  private static DraftReleaseLink CreateLink(ReleaseLinkPlatform platform)
  {
    var part = CreateDraftPart();
    var release = DraftRelease.Create(part.Id, ReleaseChannel.MainFeed, _date).Value;

    return release.Links.Single(l => l.Platform == platform);
  }
}
