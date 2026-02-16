using ScreenDrafts.Modules.Drafts.Features.Categories.Create;
using ScreenDrafts.Modules.Drafts.Features.Categories.Delete;
using ScreenDrafts.Modules.Drafts.Features.Categories.Get;
using ScreenDrafts.Modules.Drafts.Features.Categories.Restore;

namespace ScreenDrafts.Modules.Drafts.IntegrationTests.Categories;

public sealed class RestoreCategoryTests(DraftsIntegrationTestWebAppFactory factory) : DraftsIntegrationTest(factory)
{
  [Fact]
  public async Task RestoreCategoryAsync_DeletedCategory_ShouldSucceedAsync()
  {
    // Arrange
    var name = Faker.Commerce.Categories(1)[0];
    var description = Faker.Lorem.Sentence();
    var createCommand = new CreateCategoryCommand
    {
      Name = name,
      Description = description
    };

    var createResult = await Sender.Send(createCommand);
    var publicId = createResult.Value;

    var deleteCommand = new DeleteCategoryCommand(publicId);
    await Sender.Send(deleteCommand);

    var restoreCommand = new RestoreCategoryCommand(publicId);

    // Act
    var result = await Sender.Send(restoreCommand);

    // Assert
    result.Should().NotBeNull();
    result.IsSuccess.Should().BeTrue();

    var getQuery = new GetCategoryQuery(publicId);
    var category = await Sender.Send(getQuery);
    category.Value.IsDeleted.Should().BeFalse();
  }

  [Fact]
  public async Task RestoreCategoryAsync_WithNonExistentPublicId_ShouldFailAsync()
  {
    // Arrange
    var nonExistentPublicId = Faker.Random.AlphaNumeric(10);
    var restoreCommand = new RestoreCategoryCommand(nonExistentPublicId);

    // Act
    var result = await Sender.Send(restoreCommand);

    // Assert
    result.Should().NotBeNull();
    result.IsFailure.Should().BeTrue();
  }

  [Fact]
  public async Task RestoreCategoryAsync_NotDeletedCategory_ShouldSucceedAsync()
  {
    // Arrange
    var name = Faker.Commerce.Categories(1)[0];
    var description = Faker.Lorem.Sentence();
    var createCommand = new CreateCategoryCommand
    {
      Name = name,
      Description = description
    };

    var createResult = await Sender.Send(createCommand);
    var publicId = createResult.Value;

    var restoreCommand = new RestoreCategoryCommand(publicId);

    // Act
    var result = await Sender.Send(restoreCommand);

    // Assert
    result.Should().NotBeNull();
    result.IsSuccess.Should().BeTrue();

    var getQuery = new GetCategoryQuery(publicId);
    var category = await Sender.Send(getQuery);
    category.Value.IsDeleted.Should().BeFalse();
  }

  [Fact]
  public async Task RestoreCategoryAsync_MultipleRestores_ShouldSucceedAsync()
  {
    // Arrange
    var name = Faker.Commerce.Categories(1)[0];
    var description = Faker.Lorem.Sentence();
    var createCommand = new CreateCategoryCommand
    {
      Name = name,
      Description = description
    };

    var createResult = await Sender.Send(createCommand);
    var publicId = createResult.Value;

    var deleteCommand = new DeleteCategoryCommand(publicId);
    await Sender.Send(deleteCommand);

    var restoreCommand1 = new RestoreCategoryCommand(publicId);
    await Sender.Send(restoreCommand1);

    var restoreCommand2 = new RestoreCategoryCommand(publicId);

    // Act
    var result = await Sender.Send(restoreCommand2);

    // Assert
    result.Should().NotBeNull();
    result.IsSuccess.Should().BeTrue();

    var getQuery = new GetCategoryQuery(publicId);
    var category = await Sender.Send(getQuery);
    category.Value.IsDeleted.Should().BeFalse();
  }

  [Fact]
  public async Task RestoreCategoryAsync_ShouldUpdateModifiedOnUtcAsync()
  {
    // Arrange
    var name = Faker.Commerce.Categories(1)[0];
    var description = Faker.Lorem.Sentence();
    var createCommand = new CreateCategoryCommand
    {
      Name = name,
      Description = description
    };

    var createResult = await Sender.Send(createCommand);
    var publicId = createResult.Value;

    var deleteCommand = new DeleteCategoryCommand(publicId);
    await Sender.Send(deleteCommand);

    // Wait a bit to ensure timestamp difference
    await Task.Delay(100);

    var beforeRestore = DateTime.UtcNow;
    var restoreCommand = new RestoreCategoryCommand(publicId);

    // Act
    var result = await Sender.Send(restoreCommand);
    var afterRestore = DateTime.UtcNow;

    // Assert
    result.Should().NotBeNull();
    result.IsSuccess.Should().BeTrue();

    var restoredCategory = await DbContext.Set<Domain.Categories.Category>()
      .FirstOrDefaultAsync(c => c.PublicId == publicId);
    restoredCategory.Should().NotBeNull();
    restoredCategory!.ModifiedOnUtc.Should().NotBeNull();
    restoredCategory.ModifiedOnUtc!.Value.Should().BeOnOrAfter(beforeRestore);
    restoredCategory.ModifiedOnUtc.Value.Should().BeOnOrBefore(afterRestore);
  }
}
