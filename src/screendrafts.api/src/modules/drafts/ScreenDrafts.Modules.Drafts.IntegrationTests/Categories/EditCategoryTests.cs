using ScreenDrafts.Modules.Drafts.Features.Categories.Create;
using ScreenDrafts.Modules.Drafts.Features.Categories.Edit;
using ScreenDrafts.Modules.Drafts.Features.Categories.Get;

namespace ScreenDrafts.Modules.Drafts.IntegrationTests.Categories;

public sealed class EditCategoryTests(DraftsIntegrationTestWebAppFactory factory) : DraftsIntegrationTest(factory)
{
  [Fact]
  public async Task EditCategoryAsync_Name_ShouldSucceed()
  {
    // Arrange
    var originalName = Faker.Commerce.Categories(1)[0];
    var originalDescription = Faker.Lorem.Sentence();
    var createCommand = new CreateCategoryCommand
    {
      Name = originalName,
      Description = originalDescription
    };

    var createResult = await Sender.Send(createCommand);
    var publicId = createResult.Value;

    var newName = Faker.Commerce.Categories(1)[0];
    var editCommand = new EditCategoryCommand
    {
      PublicId = publicId,
      Name = newName,
      Description = null
    };

    // Act
    var result = await Sender.Send(editCommand);

    // Assert
    result.Should().NotBeNull();
    result.IsSuccess.Should().BeTrue();

    var getQuery = new GetCategoryQuery(publicId);
    var category = await Sender.Send(getQuery);
    category.Value.Name.Should().Be(newName);
    category.Value.Description.Should().Be(originalDescription);
  }

  [Fact]
  public async Task EditCategoryAsync_Description_ShouldSucceed()
  {
    // Arrange
    var originalName = Faker.Commerce.Categories(1)[0];
    var originalDescription = Faker.Lorem.Sentence();
    var createCommand = new CreateCategoryCommand
    {
      Name = originalName,
      Description = originalDescription
    };

    var createResult = await Sender.Send(createCommand);
    var publicId = createResult.Value;

    var newDescription = Faker.Lorem.Sentence();
    var editCommand = new EditCategoryCommand
    {
      PublicId = publicId,
      Name = null,
      Description = newDescription
    };

    // Act
    var result = await Sender.Send(editCommand);

    // Assert
    result.Should().NotBeNull();
    result.IsSuccess.Should().BeTrue();

    var getQuery = new GetCategoryQuery(publicId);
    var category = await Sender.Send(getQuery);
    category.Value.Name.Should().Be(originalName);
    category.Value.Description.Should().Be(newDescription);
  }

  [Fact]
  public async Task EditCategoryAsync_BothNameAndDescription_ShouldSucceed()
  {
    // Arrange
    var originalName = Faker.Commerce.Categories(1)[0];
    var originalDescription = Faker.Lorem.Sentence();
    var createCommand = new CreateCategoryCommand
    {
      Name = originalName,
      Description = originalDescription
    };

    var createResult = await Sender.Send(createCommand);
    var publicId = createResult.Value;

    var newName = Faker.Commerce.Categories(1)[0];
    var newDescription = Faker.Lorem.Sentence();
    var editCommand = new EditCategoryCommand
    {
      PublicId = publicId,
      Name = newName,
      Description = newDescription
    };

    // Act
    var result = await Sender.Send(editCommand);

    // Assert
    result.Should().NotBeNull();
    result.IsSuccess.Should().BeTrue();

    var getQuery = new GetCategoryQuery(publicId);
    var category = await Sender.Send(getQuery);
    category.Value.Name.Should().Be(newName);
    category.Value.Description.Should().Be(newDescription);
  }

  [Fact]
  public async Task EditCategoryAsync_WithNonExistentPublicId_ShouldFail()
  {
    // Arrange
    var nonExistentPublicId = Faker.Random.AlphaNumeric(10);
    var editCommand = new EditCategoryCommand
    {
      PublicId = nonExistentPublicId,
      Name = Faker.Commerce.Categories(1)[0],
      Description = null
    };

    // Act
    var result = await Sender.Send(editCommand);

    // Assert
    result.Should().NotBeNull();
    result.IsFailure.Should().BeTrue();
  }

  [Fact]
  public async Task EditCategoryAsync_WithEmptyName_ShouldFail()
  {
    // Arrange
    var originalName = Faker.Commerce.Categories(1)[0];
    var originalDescription = Faker.Lorem.Sentence();
    var createCommand = new CreateCategoryCommand
    {
      Name = originalName,
      Description = originalDescription
    };

    var createResult = await Sender.Send(createCommand);
    var publicId = createResult.Value;

    var editCommand = new EditCategoryCommand
    {
      PublicId = publicId,
      Name = string.Empty,
      Description = null
    };

    // Act
    var result = await Sender.Send(editCommand);

    // Assert
    result.Should().NotBeNull();
    result.IsFailure.Should().BeTrue();
  }

  [Fact]
  public async Task EditCategoryAsync_WithEmptyDescription_ShouldFail()
  {
    // Arrange
    var originalName = Faker.Commerce.Categories(1)[0];
    var originalDescription = Faker.Lorem.Sentence();
    var createCommand = new CreateCategoryCommand
    {
      Name = originalName,
      Description = originalDescription
    };

    var createResult = await Sender.Send(createCommand);
    var publicId = createResult.Value;

    var editCommand = new EditCategoryCommand
    {
      PublicId = publicId,
      Name = null,
      Description = string.Empty
    };

    // Act
    var result = await Sender.Send(editCommand);

    // Assert
    result.Should().NotBeNull();
    result.IsFailure.Should().BeTrue();
  }

  [Fact]
  public async Task EditCategoryAsync_WithNoChanges_ShouldSucceed()
  {
    // Arrange
    var originalName = Faker.Commerce.Categories(1)[0];
    var originalDescription = Faker.Lorem.Sentence();
    var createCommand = new CreateCategoryCommand
    {
      Name = originalName,
      Description = originalDescription
    };

    var createResult = await Sender.Send(createCommand);
    var publicId = createResult.Value;

    var editCommand = new EditCategoryCommand
    {
      PublicId = publicId,
      Name = null,
      Description = null
    };

    // Act
    var result = await Sender.Send(editCommand);

    // Assert
    result.Should().NotBeNull();
    result.IsSuccess.Should().BeTrue();

    var getQuery = new GetCategoryQuery(publicId);
    var category = await Sender.Send(getQuery);
    category.Value.Name.Should().Be(originalName);
    category.Value.Description.Should().Be(originalDescription);
  }

  [Fact]
  public async Task EditCategoryAsync_ShouldUpdateModifiedOnUtc()
  {
    // Arrange
    var originalName = Faker.Commerce.Categories(1)[0];
    var originalDescription = Faker.Lorem.Sentence();
    var createCommand = new CreateCategoryCommand
    {
      Name = originalName,
      Description = originalDescription
    };

    var createResult = await Sender.Send(createCommand);
    var publicId = createResult.Value;

    // Wait a bit to ensure timestamp difference
    await Task.Delay(100);

    var beforeEdit = DateTime.UtcNow;
    var newName = Faker.Commerce.Categories(1)[0];
    var editCommand = new EditCategoryCommand
    {
      PublicId = publicId,
      Name = newName,
      Description = null
    };

    // Act
    var result = await Sender.Send(editCommand);
    var afterEdit = DateTime.UtcNow;

    // Assert
    result.Should().NotBeNull();
    result.IsSuccess.Should().BeTrue();

    var editedCategory = await DbContext.Set<Domain.Categories.Category>()
      .FirstOrDefaultAsync(c => c.PublicId == publicId);
    editedCategory.Should().NotBeNull();
    editedCategory!.ModifiedOnUtc.Should().NotBeNull();
    editedCategory.ModifiedOnUtc!.Value.Should().BeOnOrAfter(beforeEdit);
    editedCategory.ModifiedOnUtc.Value.Should().BeOnOrBefore(afterEdit);
  }
}
