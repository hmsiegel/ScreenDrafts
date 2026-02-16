namespace ScreenDrafts.Modules.Drafts.UnitTests.Drafts.Entities;

public class DraftCategoryTests : DraftsBaseTest
{
  [Fact]
  public void Create_ShouldReturnDraftCategory_WhenValidParametersProvided()
  {
    // Arrange
    var draft = CreateDraft();
    var category = CreateCategory();

    // Act
    var draftCategory = DraftCategory.Create(draft, category);

    // Assert
    draftCategory.Should().NotBeNull();
    draftCategory.Draft.Should().Be(draft);
    draftCategory.DraftId.Should().Be(draft.Id);
    draftCategory.Category.Should().Be(category);
    draftCategory.CategoryId.Should().Be(category.Id);
  }

  [Fact]
  public void Create_ShouldThrowArgumentNullException_WhenDraftIsNull()
  {
    // Arrange
    Draft? draft = null;
    var category = CreateCategory();

    // Act
    Action act = () => DraftCategory.Create(draft!, category);

    // Assert
    act.Should().Throw<ArgumentNullException>();
  }

  [Fact]
  public void Create_ShouldThrowArgumentNullException_WhenCategoryIsNull()
  {
    // Arrange
    var draft = CreateDraft();
    Category? category = null;

    // Act
    Action act = () => DraftCategory.Create(draft, category!);

    // Assert
    act.Should().Throw<ArgumentNullException>();
  }

  [Fact]
  public void DraftId_ShouldMatchDraft()
  {
    // Arrange
    var draft = CreateDraft();
    var category = CreateCategory();

    // Act
    var draftCategory = DraftCategory.Create(draft, category);

    // Assert
    draftCategory.DraftId.Should().Be(draft.Id);
    draftCategory.Draft.Should().Be(draft);
  }

  [Fact]
  public void CategoryId_ShouldMatchCategory()
  {
    // Arrange
    var draft = CreateDraft();
    var category = CreateCategory();

    // Act
    var draftCategory = DraftCategory.Create(draft, category);

    // Assert
    draftCategory.CategoryId.Should().Be(category.Id);
    draftCategory.Category.Should().Be(category);
  }

  [Fact]
  public void Id_ShouldBeUniqueForEachDraftCategory()
  {
    // Arrange
    var draft = CreateDraft();
    var category1 = CreateCategory();
    var category2 = CreateCategory();

    // Act
    var draftCategory1 = DraftCategory.Create(draft, category1);
    var draftCategory2 = DraftCategory.Create(draft, category2);

    // Assert
    draftCategory1.DraftId.Should().NotBe(draftCategory2.DraftId);
  }

  [Fact]
  public void MultipleDraftCategories_CanBeCreatedForSameDraft()
  {
    // Arrange
    var draft = CreateDraft();
    var category1 = CreateCategory();
    var category2 = CreateCategory();
    var category3 = CreateCategory();

    // Act
    var draftCategory1 = DraftCategory.Create(draft, category1);
    var draftCategory2 = DraftCategory.Create(draft, category2);
    var draftCategory3 = DraftCategory.Create(draft, category3);

    // Assert
    draftCategory1.DraftId.Should().Be(draft.Id);
    draftCategory2.DraftId.Should().Be(draft.Id);
    draftCategory3.DraftId.Should().Be(draft.Id);
    
    var categoryIds = new[] { 
      draftCategory1.CategoryId, 
      draftCategory2.CategoryId, 
      draftCategory3.CategoryId 
    };
    categoryIds.Should().OnlyHaveUniqueItems();
  }

  [Fact]
  public void SameCategory_CanBeUsedInMultipleDrafts()
  {
    // Arrange
    var draft1 = CreateDraft();
    var draft2 = CreateDraft();
    var category = CreateCategory();

    // Act
    var draftCategory1 = DraftCategory.Create(draft1, category);
    var draftCategory2 = DraftCategory.Create(draft2, category);

    // Assert
    draftCategory1.CategoryId.Should().Be(category.Id);
    draftCategory2.CategoryId.Should().Be(category.Id);
    draftCategory1.DraftId.Should().NotBe(draftCategory2.DraftId);
  }

  [Fact]
  public void Category_ShouldProvideAccessToFullCategoryData()
  {
    // Arrange
    var draft = CreateDraft();
    var category = CreateCategory();

    // Act
    var draftCategory = DraftCategory.Create(draft, category);

    // Assert
    draftCategory.Category.Name.Should().Be(category.Name);
    draftCategory.Category.Id.Should().Be(category.Id);
  }

  [Fact]
  public void Draft_ShouldProvideAccessToFullDraftData()
  {
    // Arrange
    var draft = CreateDraft();
    var category = CreateCategory();

    // Act
    var draftCategory = DraftCategory.Create(draft, category);

    // Assert
    draftCategory.Draft.Title.Should().Be(draft.Title);
    draftCategory.Draft.DraftType.Should().Be(draft.DraftType);
    draftCategory.Draft.Id.Should().Be(draft.Id);
  }
}
