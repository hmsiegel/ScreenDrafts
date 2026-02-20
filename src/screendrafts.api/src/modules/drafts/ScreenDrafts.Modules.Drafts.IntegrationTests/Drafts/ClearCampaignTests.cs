using ScreenDrafts.Modules.Drafts.Features.Campaigns.Create;
using ScreenDrafts.Modules.Drafts.Features.Drafts.ClearCampaign;
using ScreenDrafts.Modules.Drafts.Features.Drafts.CreateDraft;
using ScreenDrafts.Modules.Drafts.Features.Drafts.SetCampaign;
using ScreenDrafts.Modules.Drafts.Features.SeriesFeatures.Create;

namespace ScreenDrafts.Modules.Drafts.IntegrationTests.Drafts;

public sealed class ClearCampaignTests(DraftsIntegrationTestWebAppFactory factory)
  : DraftsIntegrationTest(factory)
{
  [Fact]
  public async Task ClearCampaign_WithValidDraftId_ShouldSucceedAsync()
  {
    // Arrange
    var draftId = await CreateDraftAsync();
    var campaignId = await CreateCampaignAsync();

    // Set campaign first
    await Sender.Send(new SetCampaignDraftCommand
    {
      DraftId = draftId,
      CampaignId = campaignId
    });

    var command = new ClearCampaignDraftCommand
    {
      DraftId = draftId
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.Should().NotBeNull();
    result.IsSuccess.Should().BeTrue();
  }

  [Fact]
  public async Task ClearCampaign_WithNonExistentDraft_ShouldReturnErrorAsync()
  {
    // Arrange
    var command = new ClearCampaignDraftCommand
    {
      DraftId = Faker.Random.AlphaNumeric(10)
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.Should().NotBeNull();
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().NotBeEmpty();
  }

  [Fact]
  public async Task ClearCampaign_WithEmptyDraftId_ShouldReturnErrorAsync()
  {
    // Arrange
    var command = new ClearCampaignDraftCommand
    {
      DraftId = string.Empty
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.Should().NotBeNull();
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().NotBeEmpty();
  }

  [Fact]
  public async Task ClearCampaign_WhenNoCampaignSet_ShouldBeIdempotentAsync()
  {
    // Arrange
    var draftId = await CreateDraftAsync();
    var command = new ClearCampaignDraftCommand
    {
      DraftId = draftId
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.Should().NotBeNull();
    result.IsSuccess.Should().BeTrue();
  }

  [Fact]
  public async Task ClearCampaign_MultipleTimes_ShouldBeIdempotentAsync()
  {
    // Arrange
    var draftId = await CreateDraftAsync();
    var campaignId = await CreateCampaignAsync();

    // Set campaign
    await Sender.Send(new SetCampaignDraftCommand
    {
      DraftId = draftId,
      CampaignId = campaignId
    });

    var command = new ClearCampaignDraftCommand
    {
      DraftId = draftId
    };

    // Act - Clear first time
    var firstResult = await Sender.Send(command);
    // Act - Clear second time
    var secondResult = await Sender.Send(command);

    // Assert
    firstResult.Should().NotBeNull();
    firstResult.IsSuccess.Should().BeTrue();
    secondResult.Should().NotBeNull();
    secondResult.IsSuccess.Should().BeTrue();
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
      MaxPosition = 7
    };

    var result = await Sender.Send(command);
    result.IsSuccess.Should().BeTrue($"CreateDraftCommand failed: {string.Join(", ", result.Errors.Select(e => e.Description))}");
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
    result.IsSuccess.Should().BeTrue($"CreateSeriesCommand failed: {string.Join(", ", result.Errors.Select(e => e.Description))}");
    return result.Value;
  }

  private async Task<string> CreateCampaignAsync()
  {
    var command = new CreateCampaignCommand
    {
      Name = Faker.Company.CompanyName(),
      Slug = Faker.Internet.DomainWord()
    };

    var result = await Sender.Send(command);
    result.IsSuccess.Should().BeTrue($"CreateCampaignCommand failed: {string.Join(", ", result.Errors.Select(e => e.Description))}");
    return result.Value;
  }
}
