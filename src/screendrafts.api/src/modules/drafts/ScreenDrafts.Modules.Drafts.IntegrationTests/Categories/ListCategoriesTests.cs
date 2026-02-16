using ScreenDrafts.Modules.Drafts.Features.Categories.Create;
using ScreenDrafts.Modules.Drafts.Features.Categories.Delete;
using ScreenDrafts.Modules.Drafts.Features.Categories.List;

namespace ScreenDrafts.Modules.Drafts.IntegrationTests.Categories;

public sealed class ListCategoriesTests(DraftsIntegrationTestWebAppFactory factory) : DraftsIntegrationTest(factory)
{
  [Fact]
  public async Task ListCategoriesAsync_ShouldReturnAllNonDeletedCategoriesAsync()
  {
    // Arrange
    var category1 = new CreateCategoryCommand
    {
      Name = Faker.Commerce.Categories(1)[0],
      Description = Faker.Lorem.Sentence()
    };
    await Sender.Send(category1);

    var category2 = new CreateCategoryCommand
    {
      Name = Faker.Commerce.Categories(1)[0],
      Description = Faker.Lorem.Sentence()
    };
    await Sender.Send(category2);

    var query = new ListCategoriesQuery(IncludeDeleted: false);

    // Act
    var result = await Sender.Send(query);

    // Assert
    result.Should().NotBeNull();
    result.IsSuccess.Should().BeTrue();
    result.Value.Items.Should().HaveCountGreaterThanOrEqualTo(2);
    result.Value.Items.Should().OnlyContain(c => !c.IsDeleted);
  }

  [Fact]
  public async Task ListCategoriesAsync_WithIncludeDeleted_ShouldReturnAllCategoriesAsync()
  {
    // Arrange
    var category1Command = new CreateCategoryCommand
    {
      Name = Faker.Commerce.Categories(1)[0],
      Description = Faker.Lorem.Sentence()
    };
    await Sender.Send(category1Command);

    var category2Command = new CreateCategoryCommand
    {
      Name = Faker.Commerce.Categories(1)[0],
      Description = Faker.Lorem.Sentence()
    };
    var category2Result = await Sender.Send(category2Command);

    var deleteCommand = new DeleteCategoryCommand(category2Result.Value);
    await Sender.Send(deleteCommand);

    var query = new ListCategoriesQuery(IncludeDeleted: true);

    // Act
    var result = await Sender.Send(query);

    // Assert
    result.Should().NotBeNull();
    result.IsSuccess.Should().BeTrue();
    result.Value.Items.Should().HaveCountGreaterThanOrEqualTo(2);
    result.Value.Items.Should().Contain(c => c.IsDeleted);
    result.Value.Items.Should().Contain(c => !c.IsDeleted);
  }

  [Fact]
  public async Task ListCategoriesAsync_WithoutIncludeDeleted_ShouldOnlyReturnNonDeletedCategoriesAsync()
  {
    // Arrange
    var category1Command = new CreateCategoryCommand
    {
      Name = Faker.Commerce.Categories(1)[0],
      Description = Faker.Lorem.Sentence()
    };
    await Sender.Send(category1Command);

    var category2Command = new CreateCategoryCommand
    {
      Name = Faker.Commerce.Categories(1)[0],
      Description = Faker.Lorem.Sentence()
    };
    var category2Result = await Sender.Send(category2Command);

    var deleteCommand = new DeleteCategoryCommand(category2Result.Value);
    await Sender.Send(deleteCommand);

    var query = new ListCategoriesQuery(IncludeDeleted: false);

    // Act
    var result = await Sender.Send(query);

    // Assert
    result.Should().NotBeNull();
    result.IsSuccess.Should().BeTrue();
    result.Value.Items.Should().NotContain(c => c.IsDeleted);
  }

  [Fact]
  public async Task ListCategoriesAsync_EmptyDatabase_ShouldReturnEmptyCollectionAsync()
  {
    // Arrange
    var query = new ListCategoriesQuery(IncludeDeleted: false);

    // Act
    var result = await Sender.Send(query);

    // Assert
    result.Should().NotBeNull();
    result.IsSuccess.Should().BeTrue();
    result.Value.Items.Should().BeEmpty();
  }

  [Fact]
  public async Task ListCategoriesAsync_MultipleCategories_ShouldReturnAllAsync()
  {
    // Arrange
    var categoriesToCreate = 5;
    for (var i = 0; i < categoriesToCreate; i++)
    {
      var command = new CreateCategoryCommand
      {
        Name = $"{Faker.Commerce.Categories(1)[0]} {i}",
        Description = Faker.Lorem.Sentence()
      };
      await Sender.Send(command);
    }

    var query = new ListCategoriesQuery(IncludeDeleted: false);

    // Act
    var result = await Sender.Send(query);

    // Assert
    result.Should().NotBeNull();
    result.IsSuccess.Should().BeTrue();
    result.Value.Items.Should().HaveCountGreaterThanOrEqualTo(categoriesToCreate);
  }

  [Fact]
  public async Task ListCategoriesAsync_AfterDeletingAll_ShouldReturnEmptyAsync()
  {
    // Arrange
    var category1Command = new CreateCategoryCommand
    {
      Name = Faker.Commerce.Categories(1)[0],
      Description = Faker.Lorem.Sentence()
    };
    var category1Result = await Sender.Send(category1Command);

    var category2Command = new CreateCategoryCommand
    {
      Name = Faker.Commerce.Categories(1)[0],
      Description = Faker.Lorem.Sentence()
    };
    var category2Result = await Sender.Send(category2Command);

    await Sender.Send(new DeleteCategoryCommand(category1Result.Value));
    await Sender.Send(new DeleteCategoryCommand(category2Result.Value));

    var query = new ListCategoriesQuery(IncludeDeleted: false);

    // Act
    var result = await Sender.Send(query);

    // Assert
    result.Should().NotBeNull();
    result.IsSuccess.Should().BeTrue();
    result.Value.Items.Should().BeEmpty();
  }
}
