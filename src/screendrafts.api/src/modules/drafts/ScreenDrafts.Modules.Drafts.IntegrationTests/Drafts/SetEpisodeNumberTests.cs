using ScreenDrafts.Modules.Drafts.Domain.DraftParts.Enums;
using ScreenDrafts.Modules.Drafts.Features.Drafts.SetEpisodeNumber;

namespace ScreenDrafts.Modules.Drafts.IntegrationTests.Drafts;

public sealed class SetEpisodeNumberTests(DraftsIntegrationTestWebAppFactory factory)
  : DraftsIntegrationTest(factory)
{
  [Fact]
  public async Task SetEpisodeNumber_WithValidData_ShouldSucceedAsync()
  {
    // Arrange
    var draftId = await CreateDraftAsync();
    var command = new SetEpisodeNumberCommand
    {
      DraftId = draftId,
      ReleaseChannel = ReleaseChannel.MainFeed,
      EpisodeNumber = 42
    };

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    result.Should().NotBeNull();
    result.IsSuccess.Should().BeTrue();
  }

  [Fact]
  public async Task SetEpisodeNumber_ShouldPersistEpisodeNumber_InDatabaseAsync()
  {
    // Arrange
    var draftId = await CreateDraftAsync();
    var command = new SetEpisodeNumberCommand
    {
      DraftId = draftId,
      ReleaseChannel = ReleaseChannel.MainFeed,
      EpisodeNumber = 100
    };

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    result.IsSuccess.Should().BeTrue();

    var draft = await DbContext.Drafts.FirstAsync(d => d.PublicId == draftId, TestContext.Current.CancellationToken);
    var channelRelease = await DbContext.DraftChannelReleases
      .FirstAsync(cr => cr.DraftId == draft.Id, TestContext.Current.CancellationToken);

    channelRelease.EpisodeNumber.Should().Be(100);
  }

  [Fact]
  public async Task SetEpisodeNumber_ForPatreonChannel_ShouldSucceedAsync()
  {
    // Arrange
    var draftId = await CreateDraftAsync();
    var command = new SetEpisodeNumberCommand
    {
      DraftId = draftId,
      ReleaseChannel = ReleaseChannel.Patreon,
      EpisodeNumber = 15
    };

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    result.Should().NotBeNull();
    result.IsSuccess.Should().BeTrue();
  }

  [Fact]
  public async Task SetEpisodeNumber_CalledTwice_ShouldUpdateEpisodeNumberAsync()
  {
    // Arrange
    var draftId = await CreateDraftAsync();
    await Sender.Send(new SetEpisodeNumberCommand
    {
      DraftId = draftId,
      ReleaseChannel = ReleaseChannel.MainFeed,
      EpisodeNumber = 1
    }, TestContext.Current.CancellationToken);

    var updateCommand = new SetEpisodeNumberCommand
    {
      DraftId = draftId,
      ReleaseChannel = ReleaseChannel.MainFeed,
      EpisodeNumber = 2
    };

    // Act
    var result = await Sender.Send(updateCommand, TestContext.Current.CancellationToken);

    // Assert
    result.IsSuccess.Should().BeTrue();

    var draft = await DbContext.Drafts.FirstAsync(d => d.PublicId == draftId, TestContext.Current.CancellationToken);
    var channelRelease = await DbContext.DraftChannelReleases
      .FirstAsync(cr => cr.DraftId == draft.Id, TestContext.Current.CancellationToken);

    channelRelease.EpisodeNumber.Should().Be(2);
  }

  [Fact]
  public async Task SetEpisodeNumber_CanSetDifferentChannelsIndependently_ShouldSucceedAsync()
  {
    // Arrange
    var draftId = await CreateDraftAsync();

    // Act
    var mainFeedResult = await Sender.Send(new SetEpisodeNumberCommand
    {
      DraftId = draftId,
      ReleaseChannel = ReleaseChannel.MainFeed,
      EpisodeNumber = 10
    }, TestContext.Current.CancellationToken);

    var patreonResult = await Sender.Send(new SetEpisodeNumberCommand
    {
      DraftId = draftId,
      ReleaseChannel = ReleaseChannel.Patreon,
      EpisodeNumber = 5
    }, TestContext.Current.CancellationToken);

    // Assert
    mainFeedResult.IsSuccess.Should().BeTrue();
    patreonResult.IsSuccess.Should().BeTrue();
  }

  [Fact]
  public async Task SetEpisodeNumber_WithNonExistentDraftId_ShouldReturnErrorAsync()
  {
    // Arrange
    var command = new SetEpisodeNumberCommand
    {
      DraftId = Faker.Random.AlphaNumeric(10),
      ReleaseChannel = ReleaseChannel.MainFeed,
      EpisodeNumber = 1
    };

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    result.Should().NotBeNull();
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().NotBeEmpty();
  }

  [Fact]
  public async Task SetEpisodeNumber_WithEmptyDraftId_ShouldReturnErrorAsync()
  {
    // Arrange
    var command = new SetEpisodeNumberCommand
    {
      DraftId = string.Empty,
      ReleaseChannel = ReleaseChannel.MainFeed,
      EpisodeNumber = 1
    };

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    result.Should().NotBeNull();
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().NotBeEmpty();
  }

  [Fact]
  public async Task SetEpisodeNumber_WithZeroEpisodeNumber_ShouldReturnErrorAsync()
  {
    // Arrange
    var draftId = await CreateDraftAsync();
    var command = new SetEpisodeNumberCommand
    {
      DraftId = draftId,
      ReleaseChannel = ReleaseChannel.MainFeed,
      EpisodeNumber = 0
    };

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    result.Should().NotBeNull();
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().NotBeEmpty();
  }

  [Fact]
  public async Task SetEpisodeNumber_WithNegativeEpisodeNumber_ShouldReturnErrorAsync()
  {
    // Arrange
    var draftId = await CreateDraftAsync();
    var command = new SetEpisodeNumberCommand
    {
      DraftId = draftId,
      ReleaseChannel = ReleaseChannel.MainFeed,
      EpisodeNumber = -1
    };

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    result.Should().NotBeNull();
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().NotBeEmpty();
  }

  // -------------------------------------------------------------------
  // Helpers
  // -------------------------------------------------------------------

  private async Task<string> CreateDraftAsync()
  {
    var seriesId = await CreateSeriesAsync();
    var result = await Sender.Send(new CreateDraftCommand
    {
      Title = Faker.Company.CompanyName(),
      DraftType = DraftType.Standard.Value,
      SeriesId = seriesId,
    }, TestContext.Current.CancellationToken);
    return result.Value;
  }

  private async Task<Guid> CreateSeriesAsync()
  {
    var result = await Sender.Send(new CreateSeriesCommand
    {
      Name = Faker.Company.CompanyName(),
      Kind = SeriesKind.Regular.Value,
      CanonicalPolicy = CanonicalPolicy.Always.Value,
      ContinuityScope = ContinuityScope.None.Value,
      ContinuityDateRule = ContinuityDateRule.AnyChannelFirstRelease.Value,
      AllowedDraftTypes = (int)DraftTypeMask.All,
      DefaultDraftType = DraftType.Standard.Value
    }, TestContext.Current.CancellationToken);
    return result.Value;
  }
}
