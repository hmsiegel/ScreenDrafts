using ScreenDrafts.Modules.Drafts.Features.Campaigns.Create;
using ScreenDrafts.Modules.Drafts.Features.Drafts.CreateDraft;
using ScreenDrafts.Modules.Drafts.Features.Drafts.SetCampaign;
using ScreenDrafts.Modules.Drafts.Features.SeriesFeatures.Create;

namespace ScreenDrafts.Modules.Drafts.IntegrationTests.Drafts;

public sealed class SetCampaignTests(DraftsIntegrationTestWebAppFactory factory)
  : DraftsIntegrationTest(factory)
{
  [Fact]
  public async Task SetCampaign_WithValidIds_ShouldSucceedAsync()
  {
    // Arrange
    var draftId = await CreateDraftAsync();
    var campaignId = await CreateCampaignAsync();
    var command = new SetCampaignDraftCommand
    {
      DraftId = draftId,
      CampaignId = campaignId
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.Should().NotBeNull();
    result.IsSuccess.Should().BeTrue();
  }

  [Fact]
  public async Task SetCampaign_WithNonExistentDraft_ShouldReturnErrorAsync()
  {
    // Arrange
    var campaignId = await CreateCampaignAsync();
    var command = new SetCampaignDraftCommand
    {
      DraftId = Faker.Random.AlphaNumeric(10),
      CampaignId = campaignId
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.Should().NotBeNull();
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().NotBeEmpty();
  }

  [Fact]
  public async Task SetCampaign_WithNonExistentCampaign_ShouldReturnErrorAsync()
  {
    // Arrange
    var draftId = await CreateDraftAsync();
    var command = new SetCampaignDraftCommand
    {
      DraftId = draftId,
      CampaignId = Faker.Random.AlphaNumeric(10)
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.Should().NotBeNull();
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().NotBeEmpty();
  }

  [Fact]
  public async Task SetCampaign_WithEmptyDraftId_ShouldReturnErrorAsync()
  {
    // Arrange
    var campaignId = await CreateCampaignAsync();
    var command = new SetCampaignDraftCommand
    {
      DraftId = string.Empty,
      CampaignId = campaignId
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.Should().NotBeNull();
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().NotBeEmpty();
  }

  [Fact]
  public async Task SetCampaign_WithEmptyCampaignId_ShouldReturnErrorAsync()
  {
    // Arrange
    var draftId = await CreateDraftAsync();
    var command = new SetCampaignDraftCommand
    {
      DraftId = draftId,
      CampaignId = string.Empty
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.Should().NotBeNull();
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().NotBeEmpty();
  }

  [Fact]
  public async Task SetCampaign_ChangingExistingCampaign_ShouldSucceedAsync()
  {
    // Arrange
    var draftId = await CreateDraftAsync();
    var firstCampaignId = await CreateCampaignAsync();
    var secondCampaignId = await CreateCampaignAsync();

    // Set first campaign
    await Sender.Send(new SetCampaignDraftCommand
    {
      DraftId = draftId,
      CampaignId = firstCampaignId
    });

    // Change to second campaign
    var command = new SetCampaignDraftCommand
    {
      DraftId = draftId,
      CampaignId = secondCampaignId
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.Should().NotBeNull();
    result.IsSuccess.Should().BeTrue();
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

  private async Task<string> CreateCampaignAsync()
  {
    var command = new CreateCampaignCommand
    {
      Name = Faker.Company.CompanyName(),
      Slug = Faker.Internet.DomainWord()
    };

    var result = await Sender.Send(command);
    return result.Value;
  }
}
