using ScreenDrafts.Modules.Drafts.Features.Categories.Create;
using ScreenDrafts.Modules.Drafts.Features.Drafts.RemoveCategoryFromDraft;
using ScreenDrafts.Modules.Drafts.Features.Drafts.SetCategory;

namespace ScreenDrafts.Modules.Drafts.IntegrationTests.Drafts;

public sealed class RemoveCategoryFromDraftTests(DraftsIntegrationTestWebAppFactory factory)
  : DraftsIntegrationTest(factory)
{
  [Fact]
  public async Task RemoveCategoryFromDraft_WithValidIds_ShouldSucceedAsync()
  {
    // Arrange
    var draftId = await CreateDraftAsync();
    var categoryId = await CreateCategoryAsync();
    await AddCategoryToDraftAsync(draftId, categoryId);

    var command = new RemoveCategoryFromDraftCommand
    {
      DraftId = draftId,
      CategoryId = categoryId
    };

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    result.Should().NotBeNull();
    result.IsSuccess.Should().BeTrue();
  }

  [Fact]
  public async Task RemoveCategoryFromDraft_CategoryIsActuallyRemovedAsync()
  {
    // Arrange
    var draftId = await CreateDraftAsync();
    var categoryId = await CreateCategoryAsync();
    await AddCategoryToDraftAsync(draftId, categoryId);

    // Act
    await Sender.Send(new RemoveCategoryFromDraftCommand
    {
      DraftId = draftId,
      CategoryId = categoryId
    }, TestContext.Current.CancellationToken);

    // Assert — removing it a second time should fail with "not added" error
    var secondResult = await Sender.Send(new RemoveCategoryFromDraftCommand
    {
      DraftId = draftId,
      CategoryId = categoryId
    }, TestContext.Current.CancellationToken);

    secondResult.IsFailure.Should().BeTrue();
  }

  [Fact]
  public async Task RemoveCategoryFromDraft_WithNonExistentDraft_ShouldReturnErrorAsync()
  {
    // Arrange
    var categoryId = await CreateCategoryAsync();
    var command = new RemoveCategoryFromDraftCommand
    {
      DraftId = $"d_{Faker.Random.AlphaNumeric(10)}",
      CategoryId = categoryId
    };

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    result.Should().NotBeNull();
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().NotBeEmpty();
  }

  [Fact]
  public async Task RemoveCategoryFromDraft_WithNonExistentCategory_ShouldReturnErrorAsync()
  {
    // Arrange
    var draftId = await CreateDraftAsync();
    var command = new RemoveCategoryFromDraftCommand
    {
      DraftId = draftId,
      CategoryId = $"cat_{Faker.Random.AlphaNumeric(10)}"
    };

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    result.Should().NotBeNull();
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().NotBeEmpty();
  }

  [Fact]
  public async Task RemoveCategoryFromDraft_WhenCategoryNotInDraft_ShouldReturnErrorAsync()
  {
    // Arrange — category exists but was never added to this draft
    var draftId = await CreateDraftAsync();
    var categoryId = await CreateCategoryAsync();

    var command = new RemoveCategoryFromDraftCommand
    {
      DraftId = draftId,
      CategoryId = categoryId
    };

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    result.Should().NotBeNull();
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().NotBeEmpty();
  }

  [Fact]
  public async Task RemoveCategoryFromDraft_WithEmptyDraftId_ShouldReturnValidationErrorAsync()
  {
    // Arrange
    var categoryId = await CreateCategoryAsync();
    var command = new RemoveCategoryFromDraftCommand
    {
      DraftId = string.Empty,
      CategoryId = categoryId
    };

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().NotBeEmpty();
  }

  [Fact]
  public async Task RemoveCategoryFromDraft_WithEmptyCategoryId_ShouldReturnValidationErrorAsync()
  {
    // Arrange
    var draftId = await CreateDraftAsync();
    var command = new RemoveCategoryFromDraftCommand
    {
      DraftId = draftId,
      CategoryId = string.Empty
    };

    // Act
    var result = await Sender.Send(command, TestContext.Current.CancellationToken);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().NotBeEmpty();
  }

  [Fact]
  public async Task RemoveCategoryFromDraft_AfterRemoval_CanAddCategoryAgainAsync()
  {
    // Arrange
    var draftId = await CreateDraftAsync();
    var categoryId = await CreateCategoryAsync();
    await AddCategoryToDraftAsync(draftId, categoryId);

    await Sender.Send(new RemoveCategoryFromDraftCommand
    {
      DraftId = draftId,
      CategoryId = categoryId
    }, TestContext.Current.CancellationToken);

    // Act — add the same category again after removal
    var addResult = await AddCategoryToDraftAsync(draftId, categoryId);

    // Assert
    addResult.IsSuccess.Should().BeTrue();
  }

  [Fact]
  public async Task RemoveCategoryFromDraft_WithMultipleCategories_OnlyRemovesSpecifiedOneAsync()
  {
    // Arrange
    var draftId = await CreateDraftAsync();
    var categoryId1 = await CreateCategoryAsync();
    var categoryId2 = await CreateCategoryAsync();
    await AddCategoryToDraftAsync(draftId, categoryId1);
    await AddCategoryToDraftAsync(draftId, categoryId2);

    // Act — remove only the first category
    var result = await Sender.Send(new RemoveCategoryFromDraftCommand
    {
      DraftId = draftId,
      CategoryId = categoryId1
    }, TestContext.Current.CancellationToken);

    // Assert — removal succeeded
    result.IsSuccess.Should().BeTrue();

    // Removing the second category should also succeed (it's still in the draft)
    var result2 = await Sender.Send(new RemoveCategoryFromDraftCommand
    {
      DraftId = draftId,
      CategoryId = categoryId2
    }, TestContext.Current.CancellationToken);
    result2.IsSuccess.Should().BeTrue();

    // Removing the first category again should now fail
    var result3 = await Sender.Send(new RemoveCategoryFromDraftCommand
    {
      DraftId = draftId,
      CategoryId = categoryId1
    }, TestContext.Current.CancellationToken);
    result3.IsFailure.Should().BeTrue();
  }

  // -------------------------------------------------------------------------
  // Helpers
  // -------------------------------------------------------------------------

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

  private async Task<string> CreateCategoryAsync()
  {
    var result = await Sender.Send(new CreateCategoryCommand
    {
      Name = Faker.Commerce.Categories(1)[0] + Faker.Random.AlphaNumeric(8),
      Description = Faker.Lorem.Sentence()
    }, TestContext.Current.CancellationToken);
    return result.Value;
  }

  private async Task<Result> AddCategoryToDraftAsync(string draftId, string categoryId)
  {
    return await Sender.Send(new SetCategoryDraftCommand
    {
      DraftId = draftId,
      CategoryId = categoryId
    }, TestContext.Current.CancellationToken);
  }
}
