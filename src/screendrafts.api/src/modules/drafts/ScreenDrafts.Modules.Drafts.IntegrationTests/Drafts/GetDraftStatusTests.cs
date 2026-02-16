using ScreenDrafts.Modules.Drafts.Features.Drafts.CreateDraft;
using ScreenDrafts.Modules.Drafts.Features.Drafts.GetDraftStatus;
using ScreenDrafts.Modules.Drafts.Features.SeriesFeatures.Create;

namespace ScreenDrafts.Modules.Drafts.IntegrationTests.Drafts;

public sealed class GetDraftStatusTests(DraftsIntegrationTestWebAppFactory factory)
  : DraftsIntegrationTest(factory)
{
  [Fact]
  public async Task GetDraftStatus_WithValidDraftId_ShouldReturnStatusAsync()
  {
    // Arrange
    var draftId = await CreateDraftAsync();
    var query = new GetDraftStatusQuery
    {
      DraftPublicId = draftId
    };

    // Act
    var result = await Sender.Send(query);

    // Assert
    result.Should().NotBeNull();
    result.IsSuccess.Should().BeTrue();
    result.Value.Should().NotBeNull();
    result.Value.DraftPublicId.Should().Be(draftId);
    result.Value.DraftStatus.Should().NotBeNullOrEmpty();
    result.Value.Parts.Should().NotBeNull();
  }

  [Fact]
  public async Task GetDraftStatus_WithNonExistentDraftId_ShouldReturnErrorAsync()
  {
    // Arrange
    var query = new GetDraftStatusQuery
    {
      DraftPublicId = Faker.Random.AlphaNumeric(10)
    };

    // Act
    var result = await Sender.Send(query);

    // Assert
    result.Should().NotBeNull();
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().NotBeEmpty();
  }

  [Fact]
  public async Task GetDraftStatus_WithEmptyDraftId_ShouldReturnErrorAsync()
  {
    // Arrange
    var query = new GetDraftStatusQuery
    {
      DraftPublicId = string.Empty
    };

    // Act
    var result = await Sender.Send(query);

    // Assert
    result.Should().NotBeNull();
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().NotBeEmpty();
  }

  [Fact]
  public async Task GetDraftStatus_ForNewlyCreatedDraft_ShouldHaveCreatedStatusAsync()
  {
    // Arrange
    var draftId = await CreateDraftAsync();
    var query = new GetDraftStatusQuery
    {
      DraftPublicId = draftId
    };

    // Act
    var result = await Sender.Send(query);

    // Assert
    result.Should().NotBeNull();
    result.IsSuccess.Should().BeTrue();
    result.Value.DraftStatus.Should().NotBeNullOrEmpty();
  }

  [Fact]
  public async Task GetDraftStatus_WithAutoCreatedPart_ShouldHavePartsAsync()
  {
    // Arrange
    var draftId = await CreateDraftWithAutoPartAsync();
    var query = new GetDraftStatusQuery
    {
      DraftPublicId = draftId
    };

    // Act
    var result = await Sender.Send(query);

    // Assert
    result.Should().NotBeNull();
    result.IsSuccess.Should().BeTrue();
    result.Value.Parts.Should().NotBeNull();
    result.Value.Parts.Should().HaveCountGreaterThan(0);
  }

  private async Task<string> CreateDraftAsync()
  {
    var seriesId = await CreateSeriesAsync();
    var command = new CreateDraftCommand
    {
      Title = Faker.Company.CompanyName(),
      DraftType = DraftType.Standard.Value,
      SeriesId = seriesId,
      MinPosition = 1,
      MaxPosition = 7,
      AutoCreateFirstPart = false
    };

    var result = await Sender.Send(command);
    return result.Value;
  }

  private async Task<string> CreateDraftWithAutoPartAsync()
  {
    var seriesId = await CreateSeriesAsync();
    var command = new CreateDraftCommand
    {
      Title = Faker.Company.CompanyName(),
      DraftType = DraftType.Standard.Value,
      SeriesId = seriesId,
      MinPosition = 1,
      MaxPosition = 7,
      AutoCreateFirstPart = true
    };

    var result = await Sender.Send(command);
    return result.Value;
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
