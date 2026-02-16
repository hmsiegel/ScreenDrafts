using ScreenDrafts.Modules.Drafts.Features.Campaigns.Create;

namespace ScreenDrafts.Modules.Drafts.IntegrationTests.Campaigns;

public sealed class CreateCampaignTests(DraftsIntegrationTestWebAppFactory factory) : DraftsIntegrationTest(factory)
{
  [Fact]
  public async Task Create_Campaign_ShouldSucceedAsync()
  {
    // Arrange
    var name = Faker.Commerce.Department();
    var slug = Faker.Lorem.Slug();
    var command = new CreateCampaignCommand
    {
      Name = name,
      Slug = slug
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.Should().NotBeNull();
    result.IsSuccess.Should().BeTrue();
    result.Value.Should().NotBeNullOrEmpty();
  }

  [Fact]
  public async Task Create_Campaign_WithExistingSlug_ShouldFailAsync()
  {
    // Arrange
    var name = Faker.Commerce.Department();
    var slug = Faker.Lorem.Slug();
    var command1 = new CreateCampaignCommand
    {
      Name = name,
      Slug = slug
    };

    await Sender.Send(command1);

    var command2 = new CreateCampaignCommand
    {
      Name = Faker.Commerce.Department(),
      Slug = slug
    };

    // Act
    var result = await Sender.Send(command2);

    // Assert
    result.Should().NotBeNull();
    result.IsFailure.Should().BeTrue();
  }

  [Fact]
  public async Task Create_Campaign_WithEmptyName_ShouldFailAsync()
  {
    // Arrange
    var command = new CreateCampaignCommand
    {
      Name = string.Empty,
      Slug = Faker.Lorem.Slug()
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.Should().NotBeNull();
    result.IsFailure.Should().BeTrue();
  }

  [Fact]
  public async Task Create_Campaign_WithEmptySlug_ShouldFailAsync()
  {
    // Arrange
    var command = new CreateCampaignCommand
    {
      Name = Faker.Commerce.Department(),
      Slug = string.Empty
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.Should().NotBeNull();
    result.IsFailure.Should().BeTrue();
  }
}
