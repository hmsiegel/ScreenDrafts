using ScreenDrafts.Modules.Drafts.Features.Categories.Create;
using ScreenDrafts.Modules.Drafts.Features.Categories.Get;

namespace ScreenDrafts.Modules.Drafts.IntegrationTests.Categories;

public sealed class GetCategoryTests(DraftsIntegrationTestWebAppFactory factory) : DraftsIntegrationTest(factory)
{
  [Fact]
  public async Task GetCategoryAsync_ByPublicId_ShouldSucceedAsync()
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

    var query = new GetCategoryQuery(publicId);

    // Act
    var result = await Sender.Send(query);

    // Assert
    result.Should().NotBeNull();
    result.IsSuccess.Should().BeTrue();
    result.Value.Should().NotBeNull();
    result.Value.PublicId.Should().Be(publicId);
    result.Value.Name.Should().Be(name);
    result.Value.Description.Should().Be(description);
    result.Value.IsDeleted.Should().BeFalse();
  }

  [Fact]
  public async Task GetCategoryAsync_WithNonExistentPublicId_ShouldFailAsync()
  {
    // Arrange
    var nonExistentPublicId = Faker.Random.AlphaNumeric(10);
    var query = new GetCategoryQuery(nonExistentPublicId);

    // Act
    var result = await Sender.Send(query);

    // Assert
    result.Should().NotBeNull();
    result.IsFailure.Should().BeTrue();
  }

  [Fact]
  public async Task GetCategoryAsync_DeletedCategory_ShouldStillReturnAsync()
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

    // Delete the category
    var deleteCommand = new Features.Categories.Delete.DeleteCategoryCommand(publicId);
    await Sender.Send(deleteCommand);

    var query = new GetCategoryQuery(publicId);

    // Act
    var result = await Sender.Send(query);

    // Assert
    result.Should().NotBeNull();
    result.IsSuccess.Should().BeTrue();
    result.Value.Should().NotBeNull();
    result.Value.IsDeleted.Should().BeTrue();
  }
}
