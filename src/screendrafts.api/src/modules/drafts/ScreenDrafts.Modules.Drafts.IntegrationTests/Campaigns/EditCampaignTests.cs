using ScreenDrafts.Modules.Drafts.Features.Campaigns.Create;
using ScreenDrafts.Modules.Drafts.Features.Campaigns.Edit;
using ScreenDrafts.Modules.Drafts.Features.Campaigns.Get;

namespace ScreenDrafts.Modules.Drafts.IntegrationTests.Campaigns;

public sealed class EditCampaignTests(DraftsIntegrationTestWebAppFactory factory) : DraftsIntegrationTest(factory)
{
  [Fact]
  public async Task Edit_Campaign_Name_ShouldSucceedAsync()
  {
    // Arrange
    var originalName = Faker.Commerce.Department();
    var originalSlug = Faker.Lorem.Slug();
    var createCommand = new CreateCampaignCommand
    {
      Name = originalName,
      Slug = originalSlug
    };

    var createResult = await Sender.Send(createCommand);
    var publicId = createResult.Value;

    var newName = Faker.Commerce.Department();
    var editCommand = new EditCampaignCommand
    {
      PublicId = publicId,
      Name = newName,
      Slug = null
    };

    // Act
    var result = await Sender.Send(editCommand);

    // Assert
    result.Should().NotBeNull();
    result.IsSuccess.Should().BeTrue();

    var getQuery = new GetCampaignQuery(publicId);
    var campaign = await Sender.Send(getQuery);
    campaign.Value.Name.Should().Be(newName);
    campaign.Value.Slug.Should().Be(originalSlug);
  }

  [Fact]
  public async Task Edit_Campaign_Slug_ShouldSucceedAsync()
  {
    // Arrange
    var originalName = Faker.Commerce.Department();
    var originalSlug = Faker.Lorem.Slug();
    var createCommand = new CreateCampaignCommand
    {
      Name = originalName,
      Slug = originalSlug
    };

    var createResult = await Sender.Send(createCommand);
    var publicId = createResult.Value;

    var newSlug = Faker.Lorem.Slug();
    var editCommand = new EditCampaignCommand
    {
      PublicId = publicId,
      Name = null,
      Slug = newSlug
    };

    // Act
    var result = await Sender.Send(editCommand);

    // Assert
    result.Should().NotBeNull();
    result.IsSuccess.Should().BeTrue();

    var getQuery = new GetCampaignQuery(publicId);
    var campaign = await Sender.Send(getQuery);
    campaign.Value.Name.Should().Be(originalName);
    campaign.Value.Slug.Should().Be(newSlug);
  }

  [Fact]
  public async Task Edit_Campaign_BothNameAndSlug_ShouldSucceedAsync()
  {
    // Arrange
    var originalName = Faker.Commerce.Department();
    var originalSlug = Faker.Lorem.Slug();
    var createCommand = new CreateCampaignCommand
    {
      Name = originalName,
      Slug = originalSlug
    };

    var createResult = await Sender.Send(createCommand);
    var publicId = createResult.Value;

    var newName = Faker.Commerce.Department();
    var newSlug = Faker.Lorem.Slug();
    var editCommand = new EditCampaignCommand
    {
      PublicId = publicId,
      Name = newName,
      Slug = newSlug
    };

    // Act
    var result = await Sender.Send(editCommand);

    // Assert
    result.Should().NotBeNull();
    result.IsSuccess.Should().BeTrue();

    var getQuery = new GetCampaignQuery(publicId);
    var campaign = await Sender.Send(getQuery);
    campaign.Value.Name.Should().Be(newName);
    campaign.Value.Slug.Should().Be(newSlug);
  }

  [Fact]
  public async Task Edit_Campaign_WithNonExistentPublicId_ShouldFailAsync()
  {
    // Arrange
    var nonExistentPublicId = Faker.Random.AlphaNumeric(10);
    var editCommand = new EditCampaignCommand
    {
      PublicId = nonExistentPublicId,
      Name = Faker.Commerce.Department(),
      Slug = null
    };

    // Act
    var result = await Sender.Send(editCommand);

    // Assert
    result.Should().NotBeNull();
    result.IsFailure.Should().BeTrue();
  }

  [Fact]
  public async Task Edit_Campaign_WithEmptyName_ShouldFailAsync()
  {
    // Arrange
    var originalName = Faker.Commerce.Department();
    var originalSlug = Faker.Lorem.Slug();
    var createCommand = new CreateCampaignCommand
    {
      Name = originalName,
      Slug = originalSlug
    };

    var createResult = await Sender.Send(createCommand);
    var publicId = createResult.Value;

    var editCommand = new EditCampaignCommand
    {
      PublicId = publicId,
      Name = string.Empty,
      Slug = null
    };

    // Act
    var result = await Sender.Send(editCommand);

    // Assert
    result.Should().NotBeNull();
    result.IsFailure.Should().BeTrue();
  }

  [Fact]
  public async Task Edit_Campaign_WithEmptySlug_ShouldFailAsync()
  {
    // Arrange
    var originalName = Faker.Commerce.Department();
    var originalSlug = Faker.Lorem.Slug();
    var createCommand = new CreateCampaignCommand
    {
      Name = originalName,
      Slug = originalSlug
    };

    var createResult = await Sender.Send(createCommand);
    var publicId = createResult.Value;

    var editCommand = new EditCampaignCommand
    {
      PublicId = publicId,
      Name = null,
      Slug = string.Empty
    };

    // Act
    var result = await Sender.Send(editCommand);

    // Assert
    result.Should().NotBeNull();
    result.IsFailure.Should().BeTrue();
  }

  [Fact]
  public async Task Edit_Campaign_WithNoChanges_ShouldSucceedAsync()
  {
    // Arrange
    var originalName = Faker.Commerce.Department();
    var originalSlug = Faker.Lorem.Slug();
    var createCommand = new CreateCampaignCommand
    {
      Name = originalName,
      Slug = originalSlug
    };

    var createResult = await Sender.Send(createCommand);
    var publicId = createResult.Value;

    var editCommand = new EditCampaignCommand
    {
      PublicId = publicId,
      Name = null,
      Slug = null
    };

    // Act
    var result = await Sender.Send(editCommand);

    // Assert
    result.Should().NotBeNull();
    result.IsSuccess.Should().BeTrue();

    var getQuery = new GetCampaignQuery(publicId);
    var campaign = await Sender.Send(getQuery);
    campaign.Value.Name.Should().Be(originalName);
    campaign.Value.Slug.Should().Be(originalSlug);
  }
}
