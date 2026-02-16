using ScreenDrafts.Modules.Drafts.Features.Categories.Create;

namespace ScreenDrafts.Modules.Drafts.IntegrationTests.Categories;

public sealed class CreateCategoryTests(DraftsIntegrationTestWebAppFactory factory) : DraftsIntegrationTest(factory)
{
  [Fact]
  public async Task CreateCategoryAsync_ShouldSucceedAsync()
  {
    // Arrange
    var name = Faker.Commerce.Categories(1)[0];
    var description = Faker.Lorem.Sentence();
    var command = new CreateCategoryCommand
    {
      Name = name,
      Description = description
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.Should().NotBeNull();
    result.IsSuccess.Should().BeTrue();
    result.Value.Should().NotBeNullOrEmpty();
  }

  [Fact]
  public async Task CreateCategoryAsync_WithEmptyName_ShouldFailAsync()
  {
    // Arrange
    var command = new CreateCategoryCommand
    {
      Name = string.Empty,
      Description = Faker.Lorem.Sentence()
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.Should().NotBeNull();
    result.IsFailure.Should().BeTrue();
  }

  [Fact]
  public async Task CreateCategoryAsync_WithWhitespaceName_ShouldFailAsync()
  {
    // Arrange
    var command = new CreateCategoryCommand
    {
      Name = "   ",
      Description = Faker.Lorem.Sentence()
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.Should().NotBeNull();
    result.IsFailure.Should().BeTrue();
  }

  [Fact]
  public async Task CreateCategoryAsync_WithEmptyDescription_ShouldFailAsync()
  {
    // Arrange
    var command = new CreateCategoryCommand
    {
      Name = Faker.Commerce.Categories(1)[0],
      Description = string.Empty
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.Should().NotBeNull();
    result.IsFailure.Should().BeTrue();
  }

  [Fact]
  public async Task CreateCategoryAsync_WithValidData_ShouldSetCreatedOnUtcAsync()
  {
    // Arrange
    var beforeCreation = DateTime.UtcNow;
    var name = Faker.Commerce.Categories(1)[0];
    var description = Faker.Lorem.Sentence();
    var command = new CreateCategoryCommand
    {
      Name = name,
      Description = description
    };

    // Act
    var result = await Sender.Send(command);
    var afterCreation = DateTime.UtcNow;

    // Assert
    result.Should().NotBeNull();
    result.IsSuccess.Should().BeTrue();
    
    // Verify the category was created within the expected time window
    var createdCategory = await DbContext.Set<Domain.Categories.Category>()
      .FirstOrDefaultAsync(c => c.PublicId == result.Value);
    createdCategory.Should().NotBeNull();
    createdCategory!.CreatedOnUtc.Should().BeOnOrAfter(beforeCreation);
    createdCategory.CreatedOnUtc.Should().BeOnOrBefore(afterCreation);
  }
}
