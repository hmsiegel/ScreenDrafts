namespace ScreenDrafts.Modules.Drafts.UnitTests.Categories;

public class CategoryTests : BaseTest
{
  [Fact]
  public void Create_ShouldReturnSuccessResult_WhenValidParametersAreProvided()
  {
    // Arrange
    var publicId = Faker.Random.AlphaNumeric(10);
    var name = Faker.Lorem.Word();
    var description = Faker.Lorem.Sentence();

    // Act
    var result = Category.Create(publicId, name, description);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.PublicId.Should().Be(publicId);
    result.Value.Name.Should().Be(name);
    result.Value.Description.Should().Be(description);
    result.Value.IsDeleted.Should().BeFalse();
  }

  [Fact]
  public void Create_ShouldReturnFailure_WhenNameIsNullOrEmpty()
  {
    // Arrange
    var publicId = Faker.Random.AlphaNumeric(10);
    var description = Faker.Lorem.Sentence();

    // Act
    var result = Category.Create(publicId, string.Empty, description);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(CategoryErrors.CategoryNameIsRequired);
  }

  [Fact]
  public void Rename_ShouldUpdateName_WhenValidNameProvided()
  {
    // Arrange
    var category = CreateCategory();
    var newName = Faker.Lorem.Word();

    // Act
    var result = category.Rename(newName);

    // Assert
    result.IsSuccess.Should().BeTrue();
    category.Name.Should().Be(newName);
    category.ModifiedOnUtc.Should().NotBeNull();
  }

  [Fact]
  public void Rename_ShouldReturnFailure_WhenNameIsNullOrEmpty()
  {
    // Arrange
    var category = CreateCategory();

    // Act
    var result = category.Rename(string.Empty);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(CategoryErrors.CategoryNameIsRequired);
  }

  [Fact]
  public void ChangeDescription_ShouldUpdateDescription_WhenValidDescriptionProvided()
  {
    // Arrange
    var category = CreateCategory();
    var newDescription = Faker.Lorem.Sentence();

    // Act
    var result = category.ChangeDescription(newDescription);

    // Assert
    result.IsSuccess.Should().BeTrue();
    category.Description.Should().Be(newDescription);
    category.ModifiedOnUtc.Should().NotBeNull();
  }

  [Fact]
  public void ChangeDescription_ShouldReturnFailure_WhenDescriptionIsNullOrEmpty()
  {
    // Arrange
    var category = CreateCategory();

    // Act
    var result = category.ChangeDescription(string.Empty);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(CategoryErrors.CategoryDescriptionIsRequired);
  }

  [Fact]
  public void SoftDelete_ShouldMarkCategoryAsDeleted()
  {
    // Arrange
    var category = CreateCategory();

    // Act
    var result = category.SoftDelete();

    // Assert
    result.IsSuccess.Should().BeTrue();
    category.IsDeleted.Should().BeTrue();
    category.ModifiedOnUtc.Should().NotBeNull();
  }

  [Fact]
  public void SoftDelete_ShouldSucceed_WhenAlreadyDeleted()
  {
    // Arrange
    var category = CreateCategory();
    category.SoftDelete();

    // Act
    var result = category.SoftDelete();

    // Assert
    result.IsSuccess.Should().BeTrue();
    category.IsDeleted.Should().BeTrue();
  }

  [Fact]
  public void Restore_ShouldMarkCategoryAsNotDeleted()
  {
    // Arrange
    var category = CreateCategory();
    category.SoftDelete();

    // Act
    var result = category.Restore();

    // Assert
    result.IsSuccess.Should().BeTrue();
    category.IsDeleted.Should().BeFalse();
    category.ModifiedOnUtc.Should().NotBeNull();
  }

  [Fact]
  public void Restore_ShouldSucceed_WhenNotDeleted()
  {
    // Arrange
    var category = CreateCategory();

    // Act
    var result = category.Restore();

    // Assert
    result.IsSuccess.Should().BeTrue();
    category.IsDeleted.Should().BeFalse();
  }

  private static Category CreateCategory()
  {
    return Category.Create(
      publicId: Faker.Random.AlphaNumeric(10),
      name: Faker.Lorem.Word(),
      description: Faker.Lorem.Sentence()).Value;
  }
}
