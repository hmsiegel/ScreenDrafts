using ScreenDrafts.Modules.Drafts.Features.Categories.Create;
using ScreenDrafts.Modules.Drafts.Features.Drafts.CreateDraft;
using ScreenDrafts.Modules.Drafts.Features.Drafts.SetCategory;
using ScreenDrafts.Modules.Drafts.Features.SeriesFeatures.Create;

namespace ScreenDrafts.Modules.Drafts.IntegrationTests.Drafts;

public sealed class SetCategoryTests(DraftsIntegrationTestWebAppFactory factory)
  : DraftsIntegrationTest(factory)
{
  [Fact]
  public async Task SetCategory_WithValidIds_ShouldSucceedAsync()
  {
    // Arrange
    var draftId = await CreateDraftAsync();
    var categoryId = await CreateCategoryAsync();
    var command = new SetCategoryDraftCommand
    {
      DraftId = draftId,
      CategoryId = categoryId
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.Should().NotBeNull();
    result.IsSuccess.Should().BeTrue();
  }

  [Fact]
  public async Task SetCategory_WithNonExistentDraft_ShouldReturnErrorAsync()
  {
    // Arrange
    var categoryId = await CreateCategoryAsync();
    var command = new SetCategoryDraftCommand
    {
      DraftId = Faker.Random.AlphaNumeric(10),
      CategoryId = categoryId
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.Should().NotBeNull();
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().NotBeEmpty();
  }

  [Fact]
  public async Task SetCategory_WithNonExistentCategory_ShouldReturnErrorAsync()
  {
    // Arrange
    var draftId = await CreateDraftAsync();
    var command = new SetCategoryDraftCommand
    {
      DraftId = draftId,
      CategoryId = Faker.Random.AlphaNumeric(10)
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.Should().NotBeNull();
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().NotBeEmpty();
  }

  [Fact]
  public async Task SetCategory_WithEmptyDraftId_ShouldReturnErrorAsync()
  {
    // Arrange
    var categoryId = await CreateCategoryAsync();
    var command = new SetCategoryDraftCommand
    {
      DraftId = string.Empty,
      CategoryId = categoryId
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.Should().NotBeNull();
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().NotBeEmpty();
  }

  [Fact]
  public async Task SetCategory_WithEmptyCategoryId_ShouldReturnErrorAsync()
  {
    // Arrange
    var draftId = await CreateDraftAsync();
    var command = new SetCategoryDraftCommand
    {
      DraftId = draftId,
      CategoryId = string.Empty
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.Should().NotBeNull();
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().NotBeEmpty();
  }

  [Fact]
  public async Task SetCategory_AddingDuplicateCategory_ShouldReturnErrorAsync()
  {
    // Arrange
    var draftId = await CreateDraftAsync();
    var categoryId = await CreateCategoryAsync();

    await Sender.Send(new SetCategoryDraftCommand
    {
      DraftId = draftId,
      CategoryId = categoryId
    });

    var command = new SetCategoryDraftCommand
    {
      DraftId = draftId,
      CategoryId = categoryId
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.Should().NotBeNull();
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().NotBeEmpty();
  }

  [Fact]
  public async Task SetCategory_AddingMultipleDistinctCategories_ShouldSucceedAsync()
  {
    // Arrange
    var draftId = await CreateDraftAsync();
    var firstCategoryId = await CreateCategoryAsync();
    var secondCategoryId = await CreateCategoryAsync();

    await Sender.Send(new SetCategoryDraftCommand
    {
      DraftId = draftId,
      CategoryId = firstCategoryId
    });

    var command = new SetCategoryDraftCommand
    {
      DraftId = draftId,
      CategoryId = secondCategoryId
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

  private async Task<string> CreateCategoryAsync()
  {
    var command = new CreateCategoryCommand
    {
      Name = Faker.Commerce.Categories(1)[0] + Faker.Random.AlphaNumeric(8),
      Description = Faker.Lorem.Sentence()
    };

    var result = await Sender.Send(command);
    return result.Value;
  }
}
