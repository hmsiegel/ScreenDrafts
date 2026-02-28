using ScreenDrafts.Modules.Drafts.Domain.DraftParts.Enums;
using ScreenDrafts.Modules.Drafts.Features.DraftParts.SetReleaseDate;

namespace ScreenDrafts.Modules.Drafts.IntegrationTests.DraftParts;

public sealed class SetReleaseDateTests(DraftsIntegrationTestWebAppFactory factory)
  : DraftsIntegrationTest(factory)
{
  [Fact]
  public async Task SetReleaseDate_WithValidData_ShouldSucceedAsync()
  {
    // Arrange
    var draftPartPublicId = await CreateDraftPartPublicIdAsync();
    var command = new SetReleaseDateCommand
    {
      DraftPartId = draftPartPublicId,
      ReleaseDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
      ReleaseChannel = ReleaseChannel.MainFeed
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.Should().NotBeNull();
    result.IsSuccess.Should().BeTrue();
  }

  [Fact]
  public async Task SetReleaseDate_ShouldPersistRelease_InDatabaseAsync()
  {
    // Arrange
    var (draftPublicId, draftPartPublicId) = await CreateDraftWithPartAsync();
    var releaseDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(14));
    var command = new SetReleaseDateCommand
    {
      DraftPartId = draftPartPublicId,
      ReleaseDate = releaseDate,
      ReleaseChannel = ReleaseChannel.MainFeed
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.IsSuccess.Should().BeTrue();

    var draftPartId = await GetFirstDraftPartIdAsync(draftPublicId);
    var release = await DbContext.DraftReleases
      .FirstAsync(r => r.PartId == DraftPartId.Create(draftPartId));

    release.ReleaseDate.Should().Be(releaseDate);
  }

  [Fact]
  public async Task SetReleaseDate_ForPatreonChannel_ShouldSucceedAsync()
  {
    // Arrange
    var draftPartPublicId = await CreateDraftPartPublicIdAsync();
    var command = new SetReleaseDateCommand
    {
      DraftPartId = draftPartPublicId,
      ReleaseDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(3)),
      ReleaseChannel = ReleaseChannel.Patreon
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.Should().NotBeNull();
    result.IsSuccess.Should().BeTrue();
  }

  [Fact]
  public async Task SetReleaseDate_CanSetMultipleChannelReleases_ShouldSucceedAsync()
  {
    // Arrange
    var draftPartPublicId = await CreateDraftPartPublicIdAsync();

    // Act
    var mainFeedResult = await Sender.Send(new SetReleaseDateCommand
    {
      DraftPartId = draftPartPublicId,
      ReleaseDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
      ReleaseChannel = ReleaseChannel.MainFeed
    });

    var patreonResult = await Sender.Send(new SetReleaseDateCommand
    {
      DraftPartId = draftPartPublicId,
      ReleaseDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(3)),
      ReleaseChannel = ReleaseChannel.Patreon
    });

    // Assert
    mainFeedResult.IsSuccess.Should().BeTrue();
    patreonResult.IsSuccess.Should().BeTrue();
  }

  [Fact]
  public async Task SetReleaseDate_WithNonExistentDraftPartId_ShouldReturnErrorAsync()
  {
    // Arrange
    var command = new SetReleaseDateCommand
    {
      DraftPartId = Faker.Random.AlphaNumeric(10),
      ReleaseDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
      ReleaseChannel = ReleaseChannel.MainFeed
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.Should().NotBeNull();
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().NotBeEmpty();
  }

  [Fact]
  public async Task SetReleaseDate_WithEmptyDraftPartId_ShouldReturnErrorAsync()
  {
    // Arrange
    var command = new SetReleaseDateCommand
    {
      DraftPartId = string.Empty,
      ReleaseDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
      ReleaseChannel = ReleaseChannel.MainFeed
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.Should().NotBeNull();
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().NotBeEmpty();
  }

  // -------------------------------------------------------------------
  // Helpers
  // -------------------------------------------------------------------

  private async Task<string> CreateDraftPartPublicIdAsync()
  {
    var (_, publicId) = await CreateDraftWithPartAsync();
    return publicId;
  }

  private async Task<(string draftPublicId, string draftPartPublicId)> CreateDraftWithPartAsync()
  {
    var seriesId = await CreateSeriesAsync();
    var draftResult = await Sender.Send(new CreateDraftCommand
    {
      Title = Faker.Company.CompanyName(),
      DraftType = DraftType.Standard.Value,
      SeriesId = seriesId,
    });

    var draftPublicId = draftResult.Value;
    var partResult = await Sender.Send(new CreateDraftPartCommand
    {
      DraftPublicId = draftPublicId,
      PartIndex = 1,
      MinimumPosition = 1,
      MaximumPosition = 7,
    });

    return (draftPublicId, partResult.Value);
  }

  private async Task<Guid> CreateSeriesAsync()
  {
    var command = new CreateSeriesCommand
    {
      Name = Faker.Company.CompanyName(),
      Kind = SeriesKind.Regular.Value,
      CanonicalPolicy = CanonicalPolicy.Always.Value,
      ContinuityScope = ContinuityScope.None.Value,
      ContinuityDateRule = ContinuityDateRule.AnyChannelFirstRelease.Value,
      AllowedDraftTypes = (int)DraftTypeMask.All,
      DefaultDraftType = DraftType.Standard.Value
    };

    var result = await Sender.Send(command);
    return result.Value;
  }
}
