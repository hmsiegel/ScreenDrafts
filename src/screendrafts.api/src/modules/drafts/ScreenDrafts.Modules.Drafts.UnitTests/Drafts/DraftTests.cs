namespace ScreenDrafts.Modules.Drafts.UnitTests.Drafts;

public class DraftTests : BaseTest
{
  [Fact]
  public void Create_ShouldReturnSuccessResult_WhenValidParametersAreProvided()
  {
    // Arrange
    var title = Title.Create("Test Draft");
    var publicId = Faker.Random.AlphaNumeric(10);
    var draftType = DraftType.Standard;
    var series = CreateSeries();

    // Act
    var result = Draft.Create(
      title,
      publicId,
      draftType,
      series);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Title.Should().Be(title);
    result.Value.PublicId.Should().Be(publicId);
    result.Value.DraftType.Should().Be(draftType);
    result.Value.Series.Should().Be(series);
    result.Value.SeriesId.Should().Be(series.Id);
    result.Value.DraftStatus.Should().Be(DraftStatus.Created);
  }

  [Fact]
  public void Create_ShouldRaiseDomainEvent_WhenDraftIsCreated()
  {
    // Arrange
    var title = Title.Create("Test Draft");
    var publicId = Faker.Random.AlphaNumeric(10);
    var draftType = DraftType.Standard;
    var series = CreateSeries();

    // Act
    var draft = Draft.Create(title, publicId, draftType, series).Value;

    // Assert
    var domainEvent = AssertDomainEventWasPublished<DraftCreatedDomainEvent>(draft);
    domainEvent.DraftId.Should().Be(draft.Id.Value);
  }

  [Fact]
  public void Create_ShouldThrowArgumentNullException_WhenSeriesIsNull()
  {
    // Arrange
    var title = Title.Create("Test Draft");
    var publicId = Faker.Random.AlphaNumeric(10);
    var draftType = DraftType.Standard;

    // Act
    Action act = () => Draft.Create(title, publicId, draftType, null!);

    // Assert
    act.Should().Throw<ArgumentNullException>();
  }

  [Fact]
  public void Update_ShouldUpdateTitleDescriptionAndDraftType_WhenValidParametersProvided()
  {
    // Arrange
    var draft = CreateDraft();
    var newTitle = "Updated Title";
    var newDescription = "Updated Description";
    var newDraftType = DraftType.Mega;

    // Act
    draft.Update(newTitle, newDescription, newDraftType.Value);

    // Assert
    draft.Title.Value.Should().Be(newTitle);
    draft.Description.Should().Be(newDescription);
    draft.DraftType.Should().Be(newDraftType);
    draft.UpdatedAtUtc.Should().NotBeNull();
  }

  [Fact]
  public void Update_ShouldOnlyUpdateProvidedFields_WhenSomeParametersAreNull()
  {
    // Arrange
    var draft = CreateDraft();
    var originalTitle = draft.Title;
    var originalDescription = draft.Description;
    var newDraftType = DraftType.MiniMega;

    // Act
    draft.Update(null, null, newDraftType.Value);

    // Assert
    draft.Title.Should().Be(originalTitle);
    draft.Description.Should().Be(originalDescription);
    draft.DraftType.Should().Be(newDraftType);
  }

  [Fact]
  public void AddCategory_ShouldSucceed_WhenCategoryIsValid()
  {
    // Arrange
    var draft = CreateDraft();
    var category = CreateCategory();

    // Act
    var result = draft.AddCategory(category);

    // Assert
    result.IsSuccess.Should().BeTrue();
    draft.DraftCategories.Should().ContainSingle();
    draft.DraftCategories.First().CategoryId.Should().Be(category.Id);
    draft.UpdatedAtUtc.Should().NotBeNull();
  }

  [Fact]
  public void AddCategory_ShouldRaiseDomainEvent_WhenCategoryIsAdded()
  {
    // Arrange
    var draft = CreateDraft();
    var category = CreateCategory();

    // Act
    draft.AddCategory(category);

    // Assert
    var domainEvent = AssertDomainEventWasPublished<CategoryAddedDomainEvent>(draft);
    domainEvent.DraftId.Should().Be(draft.Id.Value);
    domainEvent.CategoryId.Should().Be(category.Id.Value);
  }

  [Fact]
  public void AddCategory_ShouldFail_WhenCategoryAlreadyAdded()
  {
    // Arrange
    var draft = CreateDraft();
    var category = CreateCategory();
    draft.AddCategory(category);

    // Act
    var result = draft.AddCategory(category);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(CategoryErrors.CategoryAlreadyAdded(category.Id.Value));
  }

  [Fact]
  public void AddCategory_ShouldThrowArgumentNullException_WhenCategoryIsNull()
  {
    // Arrange
    var draft = CreateDraft();

    // Act
    Action act = () => draft.AddCategory(null!);

    // Assert
    act.Should().Throw<ArgumentNullException>();
  }

  [Fact]
  public void RemoveCategory_ShouldSucceed_WhenCategoryExists()
  {
    // Arrange
    var draft = CreateDraft();
    var category = CreateCategory();
    draft.AddCategory(category);

    // Act
    var result = draft.RemoveCategory(category);

    // Assert
    result.IsSuccess.Should().BeTrue();
    draft.DraftCategories.Should().BeEmpty();
    draft.UpdatedAtUtc.Should().NotBeNull();
  }

  [Fact]
  public void RemoveCategory_ShouldRaiseDomainEvent_WhenCategoryIsRemoved()
  {
    // Arrange
    var draft = CreateDraft();
    var category = CreateCategory();
    draft.AddCategory(category);

    // Act
    draft.RemoveCategory(category);

    // Assert
    var domainEvent = AssertDomainEventWasPublished<CategoryRemovedDomainEvent>(draft);
    domainEvent.DraftId.Should().Be(draft.Id.Value);
    domainEvent.CategoryId.Should().Be(category.Id.Value);
  }

  [Fact]
  public void RemoveCategory_ShouldFail_WhenCategoryNotAdded()
  {
    // Arrange
    var draft = CreateDraft();
    var category = CreateCategory();

    // Act
    var result = draft.RemoveCategory(category);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(CategoryErrors.CannotRemoveACategoryThatIsNotAdded(category.Id.Value));
  }

  [Fact]
  public void ClearDraftCategories_ShouldRemoveAllCategories()
  {
    // Arrange
    var draft = CreateDraft();
    var category1 = CreateCategory();
    var category2 = CreateCategory();
    draft.AddCategory(category1);
    draft.AddCategory(category2);

    // Act
    draft.ClearDraftCategories();

    // Assert
    draft.DraftCategories.Should().BeEmpty();
    draft.UpdatedAtUtc.Should().NotBeNull();
  }

  [Fact]
  public void ReplaceCategories_ShouldReplaceExistingCategories_WithNewCategories()
  {
    // Arrange
    var draft = CreateDraft();
    var oldCategory1 = CreateCategory();
    var oldCategory2 = CreateCategory();
    draft.AddCategory(oldCategory1);
    draft.AddCategory(oldCategory2);

    var newCategory1 = CreateCategory();
    var newCategory2 = CreateCategory();
    var newCategories = new List<Category> { newCategory1, newCategory2 };

    // Act
    draft.ReplaceCategories(newCategories);

    // Assert
    draft.DraftCategories.Should().HaveCount(2);
    draft.DraftCategories.Should().Contain(dc => dc.CategoryId == newCategory1.Id);
    draft.DraftCategories.Should().Contain(dc => dc.CategoryId == newCategory2.Id);
    draft.DraftCategories.Should().NotContain(dc => dc.CategoryId == oldCategory1.Id);
    draft.DraftCategories.Should().NotContain(dc => dc.CategoryId == oldCategory2.Id);
  }

  [Fact]
  public void ReplaceCategories_ShouldKeepExistingCategories_WhenTheyAreInNewList()
  {
    // Arrange
    var draft = CreateDraft();
    var category1 = CreateCategory();
    var category2 = CreateCategory();
    draft.AddCategory(category1);
    draft.AddCategory(category2);

    var category3 = CreateCategory();
    var newCategories = new List<Category> { category1, category3 };

    // Act
    draft.ReplaceCategories(newCategories);

    // Assert
    draft.DraftCategories.Should().HaveCount(2);
    draft.DraftCategories.Should().Contain(dc => dc.CategoryId == category1.Id);
    draft.DraftCategories.Should().Contain(dc => dc.CategoryId == category3.Id);
    draft.DraftCategories.Should().NotContain(dc => dc.CategoryId == category2.Id);
  }

  [Fact]
  public void SetCampaign_ShouldSucceed_WhenCampaignIsValid()
  {
    // Arrange
    var draft = CreateDraft();
    var campaign = CreateCampaign();

    // Act
    var result = draft.SetCampaign(campaign);

    // Assert
    result.IsSuccess.Should().BeTrue();
    draft.Campaign.Should().Be(campaign);
    draft.CampaignId.Should().Be(campaign.Id);
    draft.UpdatedAtUtc.Should().NotBeNull();
  }

  [Fact]
  public void SetCampaign_ShouldThrowArgumentNullException_WhenCampaignIsNull()
  {
    // Arrange
    var draft = CreateDraft();

    // Act
    Action act = () => draft.SetCampaign(null!);

    // Assert
    act.Should().Throw<ArgumentNullException>();
  }

  [Fact]
  public void ClearCampaign_ShouldRemoveCampaign()
  {
    // Arrange
    var draft = CreateDraft();
    var campaign = CreateCampaign();
    draft.SetCampaign(campaign);

    // Act
    var result = draft.ClearCampaign();

    // Assert
    result.IsSuccess.Should().BeTrue();
    draft.Campaign.Should().BeNull();
    draft.CampaignId.Should().BeNull();
    draft.UpdatedAtUtc.Should().NotBeNull();
  }

  [Fact]
  public void LinkSeries_ShouldSucceed_WhenSeriesIsValid()
  {
    // Arrange
    var draft = CreateDraft();
    var newSeries = CreateSeries();

    // Act
    var result = draft.LinkSeries(newSeries);

    // Assert
    result.IsSuccess.Should().BeTrue();
    draft.Series.Should().Be(newSeries);
    draft.SeriesId.Should().Be(newSeries.Id);
    draft.UpdatedAtUtc.Should().NotBeNull();
  }

  [Fact]
  public void LinkSeries_ShouldRaiseDomainEvent_WhenSeriesIsLinked()
  {
    // Arrange
    var draft = CreateDraft();
    var newSeries = CreateSeries();

    // Act
    draft.LinkSeries(newSeries);

    // Assert
    var domainEvent = AssertDomainEventWasPublished<SeriesLinkedDomainEvent>(draft);
    domainEvent.DraftId.Should().Be(draft.Id.Value);
    domainEvent.SeriesId.Should().Be(newSeries.Id.Value);
  }

  [Fact]
  public void LinkSeries_ShouldFail_WhenSeriesIsAlreadyLinked()
  {
    // Arrange
    var draft = CreateDraft();
    var sameSeries = draft.Series;

    // Act
    var result = draft.LinkSeries(sameSeries);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(DraftErrors.SeriesAlreadyLinked(sameSeries.Id.Value));
  }

  [Fact]
  public void LinkSeries_ShouldThrowArgumentNullException_WhenSeriesIsNull()
  {
    // Arrange
    var draft = CreateDraft();

    // Act
    Action act = () => draft.LinkSeries(null!);

    // Assert
    act.Should().Throw<ArgumentNullException>();
  }

  [Fact]
  public void AddPart_ShouldSucceed_WhenPartIndexIsValid()
  {
    // Arrange
    var draft = CreateDraft();
    var partIndex = 1;
    var minPosition = 1;
    var maxPosition = 7;


    // Act
    var result = draft.AddPart(partIndex, minPosition, maxPosition);

    // Assert
    result.IsSuccess.Should().BeTrue();
    draft.Parts.Should().ContainSingle();
    draft.Parts.First().PartIndex.Should().Be(partIndex);
  }

  [Fact]
  public void AddPart_ShouldRaiseDomainEvent_WhenPartIsAdded()
  {
    // Arrange
    var draft = CreateDraft();
    var partIndex = 1;
    var minPosition = 1;
    var maxPosition = 7;

    // Act
    draft.AddPart(partIndex, minPosition, maxPosition);
    // Assert
    var domainEvent = AssertDomainEventWasPublished<DraftPartAddedDomainEvent>(draft);
    domainEvent.DraftId.Should().Be(draft.Id.Value);
    domainEvent.PartIndex.Should().Be(partIndex);
  }

  [Fact]
  public void AddPart_ShouldFail_WhenPartIndexIsZeroOrNegative()
  {
    // Arrange
    var draft = CreateDraft();
    var minPosition = 1;
    var maxPosition = 7;

    // Act
    var result = draft.AddPart(0, minPosition, maxPosition);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(DraftErrors.PartIndexMustBeGreaterThanZero);
  }

  [Fact]
  public void AddPart_ShouldFail_WhenPartWithIndexAlreadyExists()
  {
    // Arrange
    var draft = CreateDraft();
    var partIndex = 1;
    var minPosition = 1;
    var maxPosition = 7;
    draft.AddPart(partIndex, minPosition, maxPosition);

    // Act
    var result = draft.AddPart(partIndex, minPosition, maxPosition);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(DraftErrors.DraftPartWithIndexAlreadyExists(partIndex));
  }

  [Fact]
  public void RemovePart_ShouldSucceed_WhenPartExists()
  {
    // Arrange
    var draft = CreateDraft();
    var partId = draft.AddPart(1, 1, 7).Value;

    // Act
    var result = draft.RemovePart(partId);

    // Assert
    result.IsSuccess.Should().BeTrue();
    draft.Parts.Should().BeEmpty();
  }

  [Fact]
  public void RemovePart_ShouldRaiseDomainEvent_WhenPartIsRemoved()
  {
    // Arrange
    var draft = CreateDraft();
    var partId = draft.AddPart(1, 1, 7).Value;

    // Act
    draft.RemovePart(partId);

    // Assert
    var domainEvent = AssertDomainEventWasPublished<DraftPartRemovedDomainEvent>(draft);
    domainEvent.DraftId.Should().Be(draft.Id.Value);
    domainEvent.DraftPartId.Should().Be(partId.Value);
  }

  [Fact]
  public void RemovePart_ShouldFail_WhenPartNotFound()
  {
    // Arrange
    var draft = CreateDraft();
    var nonExistentPartId = DraftPartId.CreateUnique();

    // Act
    var result = draft.RemovePart(nonExistentPartId);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(DraftErrors.DraftPartNotFound(nonExistentPartId.Value));
  }

  [Fact]
  public void DeriveDraftStatus_ShouldSetStatusToCancelled_WhenAllPartsAreCancelled()
  {
    // Arrange
    var draft = CreateDraftWithPart();
    var part = draft.Parts.First();
    part.SetDraftStatus(DraftPartStatus.Cancelled);
    var utcNow = DateTime.UtcNow;

    // Act
    draft.DeriveDraftStatus(utcNow);

    // Assert
    draft.DraftStatus.Should().Be(DraftStatus.Cancelled);
  }

  [Fact]
  public void DeriveDraftStatus_ShouldSetStatusToInProgress_WhenAnyPartIsInProgress()
  {
    // Arrange
    var draft = CreateDraftWithPart();
    var part = draft.Parts.First();
    part.SetDraftStatus(DraftPartStatus.InProgress);
    var utcNow = DateTime.UtcNow;

    // Act
    draft.DeriveDraftStatus(utcNow);

    // Assert
    draft.DraftStatus.Should().Be(DraftStatus.InProgress);
  }

  [Fact]
  public void DeriveDraftStatus_ShouldSetStatusToCompleted_WhenAllPartsAreCompletedOrCancelled()
  {
    // Arrange
    var draft = CreateDraftWithPart();
    var part = draft.Parts.First();
    part.SetDraftStatus(DraftPartStatus.Completed);
    var utcNow = DateTime.UtcNow;

    // Act
    draft.DeriveDraftStatus(utcNow);

    // Assert
    draft.DraftStatus.Should().Be(DraftStatus.Completed);
  }

  [Fact]
  public void GetLifecycleView_ShouldReturnInProgress_WhenStatusIsInProgress()
  {
    // Arrange
    var draft = CreateDraftWithPart();
    var part = draft.Parts.First();
    part.SetDraftStatus(DraftPartStatus.InProgress);
    draft.DeriveDraftStatus(DateTime.UtcNow);
    var utcNow = DateTime.UtcNow;

    // Act
    var view = draft.GetLifecycleView(utcNow);

    // Assert
    view.Should().Be(DraftLifecycleView.InProgress);
  }

  [Fact]
  public void GetLifecycleView_ShouldReturnCompleted_WhenStatusIsCompleted()
  {
    // Arrange
    var draft = CreateDraftWithPart();
    var part = draft.Parts.First();
    part.SetDraftStatus(DraftPartStatus.Completed);
    draft.DeriveDraftStatus(DateTime.UtcNow);
    var utcNow = DateTime.UtcNow;

    // Act
    var view = draft.GetLifecycleView(utcNow);

    // Assert
    view.Should().Be(DraftLifecycleView.Completed);
  }

  [Fact]
  public void GetLifecycleView_ShouldReturnCancelled_WhenStatusIsCancelled()
  {
    // Arrange
    var draft = CreateDraftWithPart();
    var part = draft.Parts.First();
    part.SetDraftStatus(DraftPartStatus.Cancelled);
    draft.DeriveDraftStatus(DateTime.UtcNow);
    var utcNow = DateTime.UtcNow;

    // Act
    var view = draft.GetLifecycleView(utcNow);

    // Assert
    view.Should().Be(DraftLifecycleView.Cancelled);
  }

  [Fact]
  public void TotalParts_ShouldReturnCorrectCount()
  {
    // Arrange
    var draft = CreateDraft();
    draft.AddPart(1, 15, 21);
    draft.AddPart(2, 8, 14);
    draft.AddPart(3, 1, 7);

    // Act
    var totalParts = draft.TotalParts;

    // Assert
    totalParts.Should().Be(3);
  }

  [Fact]
  public void RenumberDraftParts_ShouldRenumberPartsSequentially()
  {
    // Arrange
    var draft = CreateDraft();
    draft.AddPart(1, 15, 21);
    draft.AddPart(3, 8, 14);
    draft.AddPart(5, 1, 7);

    // Act
    draft.RenumberDraftParts();

    // Assert
    var parts = draft.Parts.OrderBy(p => p.PartIndex).ToList();
    parts[0].PartIndex.Should().Be(1);
    parts[1].PartIndex.Should().Be(2);
    parts[2].PartIndex.Should().Be(3);
  }

  private static Draft CreateDraft()
  {
    var title = Title.Create(Faker.Lorem.Word());
    var publicId = Faker.Random.AlphaNumeric(10);
    var draftType = DraftType.Standard;
    var series = CreateSeries();

    return Draft.Create(title, publicId, draftType, series).Value;
  }

  private static Draft CreateDraftWithPart()
  {
    var draft = CreateDraft();
    draft.AddPart(1, 15, 21);
    return draft;
  }

  private static Series CreateSeries()
  {
    return Series.Create(
      name: Faker.Lorem.Word(),
      publicId: Faker.Random.AlphaNumeric(10),
      canonicalPolicy: CanonicalPolicy.Always,
      continuityScope: ContinuityScope.Global,
      continuityDateRule: ContinuityDateRule.AnyChannelFirstRelease,
      kind: SeriesKind.Regular,
      defaultDraftType: DraftType.Standard,
      allowedDraftTypes: DraftTypeMask.All).Value;
  }

  private static Category CreateCategory()
  {
    return Category.Create(
      publicId: Faker.Random.AlphaNumeric(10),
      name: Faker.Lorem.Word(),
      description: Faker.Lorem.Sentence()).Value;
  }

  private static Campaign CreateCampaign()
  {
    return Campaign.Create(
      slug: Faker.Internet.DomainWord(),
      name: Faker.Lorem.Word(),
      publicId: Faker.Random.AlphaNumeric(10)).Value;
  }
}

