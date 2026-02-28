using ScreenDrafts.Modules.Drafts.Features.Drafts.CreateDraft;
using ScreenDrafts.Modules.Drafts.Features.SeriesFeatures.Create;

namespace ScreenDrafts.Modules.Drafts.IntegrationTests.Drafts;

public sealed class CreateDraftTests(DraftsIntegrationTestWebAppFactory factory)
  : DraftsIntegrationTest(factory)
{
  [Fact]
  public async Task CreateDraft_WithValidData_ShouldReturnDraftIdAsync()
  {
    // Arrange
    var seriesId = await CreateSeriesAsync();
    var command = new CreateDraftCommand
    {
      Title = Faker.Company.CompanyName(),
      DraftType = DraftType.Standard.Value,
      SeriesId = seriesId,
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.Should().NotBeNull();
    result.IsSuccess.Should().BeTrue();
    result.Value.Should().NotBeNullOrEmpty();
  }

  [Fact]
  public async Task CreateDraft_WithEmptyTitle_ShouldReturnErrorAsync()
  {
    // Arrange
    var seriesId = await CreateSeriesAsync();
    var command = new CreateDraftCommand
    {
      Title = string.Empty,
      DraftType = DraftType.Standard.Value,
      SeriesId = seriesId,
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.Should().NotBeNull();
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().NotBeEmpty();
  }

  [Fact]
  public async Task CreateDraft_WithNonExistentSeries_ShouldReturnErrorAsync()
  {
    // Arrange
    var command = new CreateDraftCommand
    {
      Title = Faker.Company.CompanyName(),
      DraftType = DraftType.Standard.Value,
      SeriesId = Guid.NewGuid(),
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.Should().NotBeNull();
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().NotBeEmpty();
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
