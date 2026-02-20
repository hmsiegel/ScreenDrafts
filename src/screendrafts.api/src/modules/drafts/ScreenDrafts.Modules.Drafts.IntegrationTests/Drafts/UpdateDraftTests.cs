using ScreenDrafts.Modules.Drafts.Features.Campaigns.Create;
using ScreenDrafts.Modules.Drafts.Features.Drafts.CreateDraft;
using ScreenDrafts.Modules.Drafts.Features.Drafts.Update;
using ScreenDrafts.Modules.Drafts.Features.SeriesFeatures.Create;

namespace ScreenDrafts.Modules.Drafts.IntegrationTests.Drafts;

public sealed class UpdateDraftTests(DraftsIntegrationTestWebAppFactory factory)
  : DraftsIntegrationTest(factory)
{
  [Fact]
  public async Task UpdateDraft_WithNewTitle_ShouldUpdateSuccessfullyAsync()
  {
    // Arrange
    var draftId = await CreateDraftAsync();
    var newTitle = Faker.Company.CompanyName();
    var command = new UpdateDraftCommand
    {
      PublicId = draftId,
      Title = newTitle
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.Should().NotBeNull();
    result.IsSuccess.Should().BeTrue();
  }

  [Fact]
  public async Task UpdateDraft_WithNewDescription_ShouldUpdateSuccessfullyAsync()
  {
    // Arrange
    var draftId = await CreateDraftAsync();
    var newDescription = Faker.Lorem.Paragraph();
    var command = new UpdateDraftCommand
    {
      PublicId = draftId,
      Description = newDescription
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.Should().NotBeNull();
    result.IsSuccess.Should().BeTrue();
  }

  [Fact]
  public async Task UpdateDraft_WithNewSeries_ShouldUpdateSuccessfullyAsync()
  {
    // Arrange
    var draftId = await CreateDraftAsync();
    var newSeriesId = await CreateSeriesAsync();
    var command = new UpdateDraftCommand
    {
      PublicId = draftId,
      SeriesPublicId = newSeriesId.ToString()
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.Should().NotBeNull();
    result.IsSuccess.Should().BeTrue();
  }

  [Fact]
  public async Task UpdateDraft_WithNewCampaign_ShouldUpdateSuccessfullyAsync()
  {
    // Arrange
    var draftId = await CreateDraftAsync();
    var campaignId = await CreateCampaignAsync();
    var command = new UpdateDraftCommand
    {
      PublicId = draftId,
      CampaignPublicId = campaignId
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.Should().NotBeNull();
    result.IsSuccess.Should().BeTrue();
  }

  [Fact]
  public async Task UpdateDraft_WithNonExistentId_ShouldReturnErrorAsync()
  {
    // Arrange
    var command = new UpdateDraftCommand
    {
      PublicId = Faker.Random.AlphaNumeric(10),
      Title = Faker.Company.CompanyName()
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.Should().NotBeNull();
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().NotBeEmpty();
  }

  [Fact]
  public async Task UpdateDraft_WithEmptyTitle_ShouldReturnErrorAsync()
  {
    // Arrange
    var draftId = await CreateDraftAsync();
    var command = new UpdateDraftCommand
    {
      PublicId = draftId,
      Title = string.Empty
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.Should().NotBeNull();
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().NotBeEmpty();
  }

  [Fact]
  public async Task UpdateDraft_WithMultipleFields_ShouldUpdateAllAsync()
  {
    // Arrange
    var draftId = await CreateDraftAsync();
    var newTitle = Faker.Company.CompanyName();
    var newDescription = Faker.Lorem.Paragraph();
    var command = new UpdateDraftCommand
    {
      PublicId = draftId,
      Title = newTitle,
      Description = newDescription
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
    result.IsSuccess.Should().BeTrue($"CreateDraft should succeed, but got errors: {string.Join(", ", result.Errors)}");
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
    result.IsSuccess.Should().BeTrue($"CreateSeries should succeed, but got errors: {string.Join(", ", result.Errors)}");
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
    result.IsSuccess.Should().BeTrue($"CreateCampaign should succeed, but got errors: {string.Join(", ", result.Errors)}");
    return result.Value;
  }
}
