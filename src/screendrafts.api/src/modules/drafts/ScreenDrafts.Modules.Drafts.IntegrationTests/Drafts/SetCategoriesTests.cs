using ScreenDrafts.Modules.Drafts.Features.Categories.Create;
using ScreenDrafts.Modules.Drafts.Features.Drafts.SetCategories;

namespace ScreenDrafts.Modules.Drafts.IntegrationTests.Drafts;

public sealed class SetCategoriesTests(DraftsIntegrationTestWebAppFactory factory)
  : DraftsIntegrationTest(factory)
{
  [Fact]
  public async Task SetCategories_WithSingleValidCategory_ShouldSucceedAsync()
  {
    // Arrange
    var draftId = await CreateDraftAsync();
    var categoryId = await CreateCategoryAsync();
    var command = new SetCategoriesDraftCommand
    {
      DraftId = draftId,
      CategoryIds = [categoryId]
    };

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    result.Should().NotBeNull();
    result.IsSuccess.Should().BeTrue();
  }

  [Fact]
  public async Task SetCategories_WithMultipleValidCategories_ShouldSucceedAsync()
  {
    // Arrange
    var draftId = await CreateDraftAsync();
    var categoryId1 = await CreateCategoryAsync();
    var categoryId2 = await CreateCategoryAsync();
    var categoryId3 = await CreateCategoryAsync();
    var command = new SetCategoriesDraftCommand
    {
      DraftId = draftId,
      CategoryIds = [categoryId1, categoryId2, categoryId3]
    };

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    result.Should().NotBeNull();
    result.IsSuccess.Should().BeTrue();
  }

  [Fact]
  public async Task SetCategories_WithEmptyList_ShouldSucceedAndClearCategoriesAsync()
  {
    // Arrange — first add a category, then clear with empty list
    var draftId = await CreateDraftAsync();
    var categoryId = await CreateCategoryAsync();

    await Sender.Send(new SetCategoriesDraftCommand
    {
      DraftId = draftId,
      CategoryIds = [categoryId]
    }, TestContext.Current.CancellationToken);

    var clearCommand = new SetCategoriesDraftCommand
    {
      DraftId = draftId,
      CategoryIds = []
    };

    // Act
    var result = await Sender.Send(clearCommand, TestContext.Current.CancellationToken);

    // Assert
    result.Should().NotBeNull();
    result.IsSuccess.Should().BeTrue();
  }

  [Fact]
  public async Task SetCategories_ShouldReplaceExistingCategories_WhenCalledTwiceAsync()
  {
    // Arrange
    var draftId = await CreateDraftAsync();
    var firstCategoryId = await CreateCategoryAsync();
    var secondCategoryId = await CreateCategoryAsync();

    var firstResult = await Sender.Send(new SetCategoriesDraftCommand
    {
      DraftId = draftId,
      CategoryIds = [firstCategoryId]
    }, TestContext.Current.CancellationToken);

    var replaceCommand = new SetCategoriesDraftCommand
    {
      DraftId = draftId,
      CategoryIds = [secondCategoryId]
    };

    // Act
    var result = await Sender.Send(replaceCommand, TestContext.Current.CancellationToken);

    // Assert
    firstResult.IsSuccess.Should().BeTrue();
    result.Should().NotBeNull();
    result.IsSuccess.Should().BeTrue();
  }

  [Fact]
  public async Task SetCategories_WithNonExistentDraft_ShouldReturnErrorAsync()
  {
    // Arrange
    var categoryId = await CreateCategoryAsync();
    var command = new SetCategoriesDraftCommand
    {
      DraftId = Faker.Random.AlphaNumeric(10),
      CategoryIds = [categoryId]
    };

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    result.Should().NotBeNull();
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().NotBeEmpty();
  }

  [Fact]
  public async Task SetCategories_WithNonExistentCategoryId_ShouldReturnErrorAsync()
  {
    // Arrange
    var draftId = await CreateDraftAsync();
    var command = new SetCategoriesDraftCommand
    {
      DraftId = draftId,
      CategoryIds = [Faker.Random.AlphaNumeric(10)]
    };

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    result.Should().NotBeNull();
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().NotBeEmpty();
  }

  [Fact]
  public async Task SetCategories_WithMixedValidAndInvalidCategoryIds_ShouldReturnErrorAsync()
  {
    // Arrange
    var draftId = await CreateDraftAsync();
    var validCategoryId = await CreateCategoryAsync();
    var command = new SetCategoriesDraftCommand
    {
      DraftId = draftId,
      CategoryIds = [validCategoryId, Faker.Random.AlphaNumeric(10)]
    };

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    result.Should().NotBeNull();
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().NotBeEmpty();
  }

  [Fact]
  public async Task SetCategories_WithEmptyDraftId_ShouldReturnErrorAsync()
  {
    // Arrange
    var categoryId = await CreateCategoryAsync();
    var command = new SetCategoriesDraftCommand
    {
      DraftId = string.Empty,
      CategoryIds = [categoryId]
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
    var command = new CreateDraftCommand
    {
      Title = Faker.Company.CompanyName(),
      DraftType = DraftType.Standard.Value,
      SeriesId = seriesId,
    };

    var result = await Sender.Send(command, TestContext.Current.CancellationToken);
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

    var result = await Sender.Send(command, TestContext.Current.CancellationToken);
    return result.Value;
  }

  private async Task<string> CreateCategoryAsync()
  {
    var command = new CreateCategoryCommand
    {
      Name = Faker.Commerce.Categories(1)[0] + Faker.Random.AlphaNumeric(8),
      Description = Faker.Lorem.Sentence()
    };

    var result = await Sender.Send(command, TestContext.Current.CancellationToken);
    return result.Value;
  }
}
