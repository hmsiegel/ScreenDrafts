using ScreenDrafts.Modules.Drafts.Features.Categories.Create;
using ScreenDrafts.Modules.Drafts.Features.Categories.Delete;
using ScreenDrafts.Modules.Drafts.Features.Categories.Get;

namespace ScreenDrafts.Modules.Drafts.IntegrationTests.Categories;

public sealed class DeleteCategoryTests(DraftsIntegrationTestWebAppFactory factory) : DraftsIntegrationTest(factory)
{
  [Fact]
  public async Task DeleteCategoryAsync_ShouldSucceedAsync()
  {
    // Arrange
    var name = Faker.Commerce.Categories(1)[0];
    var description = Faker.Lorem.Sentence();
    var createCommand = new CreateCategoryCommand
    {
      Name = name,
      Description = description
    };

    var createResult = await Sender.Send(createCommand, TestContext.Current.CancellationToken);
    var publicId = createResult.Value;

    var deleteCommand = new DeleteCategoryCommand(publicId);

    // Act
    var result = await Sender.Send(deleteCommand, TestContext.Current.CancellationToken);

    // Assert
    result.Should().NotBeNull();
    result.IsSuccess.Should().BeTrue();

    var getQuery = new GetCategoryQuery(publicId);
    var category = await Sender.Send(getQuery, TestContext.Current.CancellationToken);
    category.Value.IsDeleted.Should().BeTrue();
  }

  [Fact]
  public async Task DeleteCategoryAsync_WithNonExistentPublicId_ShouldFailAsync()
  {
    // Arrange
    var nonExistentPublicId = Faker.Random.AlphaNumeric(10);
    var deleteCommand = new DeleteCategoryCommand(nonExistentPublicId);

    // Act
    var result = await Sender.Send(deleteCommand, TestContext.Current.CancellationToken);

    // Assert
    result.Should().NotBeNull();
    result.IsFailure.Should().BeTrue();
  }

  [Fact]
  public async Task DeleteCategoryAsync_AlreadyDeleted_ShouldSucceedAsync()
  {
    // Arrange
    var name = Faker.Commerce.Categories(1)[0];
    var description = Faker.Lorem.Sentence();
    var createCommand = new CreateCategoryCommand
    {
      Name = name,
      Description = description
    };

    var createResult = await Sender.Send(createCommand, TestContext.Current.CancellationToken);
    var publicId = createResult.Value;

    var deleteCommand1 = new DeleteCategoryCommand(publicId);
    await Sender.Send(deleteCommand1, TestContext.Current.CancellationToken);

    var deleteCommand2 = new DeleteCategoryCommand(publicId);

    // Act
    var result = await Sender.Send(deleteCommand2, TestContext.Current.CancellationToken);

    // Assert
    result.Should().NotBeNull();
    result.IsSuccess.Should().BeTrue();

    var getQuery = new GetCategoryQuery(publicId);
    var category = await Sender.Send(getQuery, TestContext.Current.CancellationToken);
    category.Value.IsDeleted.Should().BeTrue();
  }

  [Fact]
  public async Task DeleteCategoryAsync_ShouldUpdateModifiedOnUtcAsync()
  {
    // Arrange
    var name = Faker.Commerce.Categories(1)[0];
    var description = Faker.Lorem.Sentence();
    var createCommand = new CreateCategoryCommand
    {
      Name = name,
      Description = description
    };

    var createResult = await Sender.Send(createCommand, TestContext.Current.CancellationToken);
    var publicId = createResult.Value;

    // Wait a bit to ensure timestamp difference
    await Task.Delay(100, TestContext.Current.CancellationToken);

    var beforeDelete = DateTime.UtcNow;
    var deleteCommand = new DeleteCategoryCommand(publicId);

    // Act
    var result = await Sender.Send(deleteCommand, TestContext.Current.CancellationToken);
    var afterDelete = DateTime.UtcNow;

    // Assert
    result.Should().NotBeNull();
    result.IsSuccess.Should().BeTrue();

    var deletedCategory = await DbContext.Set<Domain.Categories.Category>()
      .FirstOrDefaultAsync(c => c.PublicId == publicId, TestContext.Current.CancellationToken);
    deletedCategory.Should().NotBeNull();
    deletedCategory!.ModifiedOnUtc.Should().NotBeNull();
    deletedCategory.ModifiedOnUtc!.Value.Should().BeOnOrAfter(beforeDelete);
    deletedCategory.ModifiedOnUtc.Value.Should().BeOnOrBefore(afterDelete);
  }
}
