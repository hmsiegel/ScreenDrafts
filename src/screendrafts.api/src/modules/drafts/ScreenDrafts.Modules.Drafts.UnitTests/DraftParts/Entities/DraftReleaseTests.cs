namespace ScreenDrafts.Modules.Drafts.UnitTests.DraftParts.Entities;

public class DraftReleaseTests : DraftsBaseTest
{
  [Fact]
  public void Create_ShouldReturnDraftRelease_WhenValidParametersProvided()
  {
    // Arrange
    var draftPart = CreateDraftPart();
    var channel = ReleaseChannel.MainFeed;
    var releaseDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30));

    // Act
    var release = DraftRelease.Create(draftPart.Id, channel, releaseDate);

    // Assert
    release.Should().NotBeNull();
    release.Value.DraftPart.Id.Should().Be(draftPart.Id);
    release.Value.ReleaseChannel.Should().Be(channel);
    release.Value.ReleaseDate.Should().Be(releaseDate);
  }

  [Fact]
  public void Create_ShouldThrowArgumentNullException_WhenDraftPartIsNull()
  {
    // Arrange
    DraftPart? draftPart = null;
    var channel = ReleaseChannel.MainFeed;
    var releaseDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30));

    // Act
    Action act = () => DraftRelease.Create(draftPart!.Id, channel, releaseDate);
    // Assert
    act.Should().Throw<ArgumentNullException>();
  }

  [Fact]
  public void Create_ShouldThrowArgumentNullException_WhenChannelIsNull()
  {
    // Arrange
    var draftPart = CreateDraftPart();
    ReleaseChannel? channel = null;
    var releaseDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30));

    // Act
    Action act = () => DraftRelease.Create(draftPart.Id, channel!, releaseDate);
    // Assert
    act.Should().Throw<ArgumentNullException>();
  }

  [Fact]
  public void ReleaseChannel_ShouldBeMainFeed()
  {
    // Arrange
    var draftPart = CreateDraftPart();
    var channel = ReleaseChannel.MainFeed;
    var releaseDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30));

    // Act
    var release = DraftRelease.Create(draftPart.Id, channel, releaseDate);

    // Assert
    release.Value.ReleaseChannel.Should().Be(ReleaseChannel.MainFeed);
    release.Value.ReleaseChannel.Name.Should().Be("Main Feed");
  }

  [Fact]
  public void ReleaseChannel_ShouldBePatreonFeed()
  {
    // Arrange
    var draftPart = CreateDraftPart();
    var channel = ReleaseChannel.Patreon;
    var releaseDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30));

    // Act
    var release = DraftRelease.Create(draftPart.Id, channel, releaseDate);
    // Assert
    release.Value.ReleaseChannel.Should().Be(ReleaseChannel.Patreon);
    release.Value.ReleaseChannel.Name.Should().Be("Patreon");
  }

  [Fact]
  public void ReleaseDate_ShouldBeInFuture()
  {
    // Arrange
    var draftPart = CreateDraftPart();
    var channel = ReleaseChannel.MainFeed;
    var futureDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30));

    // Act
    var release = DraftRelease.Create(draftPart.Id, channel, futureDate);
    // Assert
    release.Value.ReleaseDate.Should().Be(futureDate);
    release.Value.ReleaseDate.Should().BeAfter(DateOnly.FromDateTime(DateTime.UtcNow));
  }

  [Fact]
  public void ReleaseDate_ShouldAcceptPastDate()
  {
    // Arrange
    var draftPart = CreateDraftPart();
    var channel = ReleaseChannel.MainFeed;
    var pastDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30));

    // Act
    var release = DraftRelease.Create(draftPart.Id, channel, pastDate);
    // Assert
    release.Value.ReleaseDate.Should().Be(pastDate);
    release.Value.ReleaseDate.Should().BeBefore(DateOnly.FromDateTime(DateTime.UtcNow));
  }

  [Fact]
  public void DraftPartId_ShouldMatchDraftPart()
  {
    // Arrange
    var draftPart = CreateDraftPart();
    var channel = ReleaseChannel.MainFeed;
    var releaseDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30));

    // Act
    var release = DraftRelease.Create(draftPart.Id, channel, releaseDate);
    // Assert
    release.Value.DraftPart.Id.Should().Be(draftPart.Id);
  }

  [Fact]
  public void MultipleReleases_CanBeCreatedForSameDraftPart()
  {
    // Arrange
    var draftPart = CreateDraftPart();
    var channel = ReleaseChannel.MainFeed;
    var mainFeedDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30));
    var patreonFeedDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(23));

    // Act
    var mainRelease = DraftRelease.Create(draftPart.Id, channel, mainFeedDate);
    var patreonRelease = DraftRelease.Create(draftPart.Id, ReleaseChannel.Patreon, patreonFeedDate);

    // Assert
    mainRelease.Value.DraftPart.Id.Should().Be(draftPart.Id);
    patreonRelease.Value.DraftPart.Id.Should().Be(draftPart.Id);
    mainRelease.Value.ReleaseChannel.Should().NotBe(patreonRelease.Value.ReleaseChannel);
  }
}
