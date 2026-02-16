using ScreenDrafts.Modules.Drafts.Features.Campaigns.Create;
using ScreenDrafts.Modules.Drafts.Features.Campaigns.Delete;
using ScreenDrafts.Modules.Drafts.Features.Campaigns.Get;
using ScreenDrafts.Modules.Drafts.Features.Campaigns.Restore;

namespace ScreenDrafts.Modules.Drafts.IntegrationTests.Campaigns;

public sealed class RestoreCampaignTests(DraftsIntegrationTestWebAppFactory factory) : DraftsIntegrationTest(factory)
{
  [Fact]
  public async Task Restore_DeletedCampaign_ShouldSucceedAsync()
  {
    // Arrange
    var name = Faker.Commerce.Department();
    var slug = Faker.Lorem.Slug();
    var createCommand = new CreateCampaignCommand
    {
      Name = name,
      Slug = slug
    };

    var createResult = await Sender.Send(createCommand);
    var publicId = createResult.Value;

    var deleteCommand = new DeleteCampaignCommand(publicId);
    await Sender.Send(deleteCommand);

    var restoreCommand = new RestoreCampaignCommand(publicId);

    // Act
    var result = await Sender.Send(restoreCommand);

    // Assert
    result.Should().NotBeNull();
    result.IsSuccess.Should().BeTrue();

    var getQuery = new GetCampaignQuery(publicId);
    var campaign = await Sender.Send(getQuery);
    campaign.Value.IsDeleted.Should().BeFalse();
  }

  [Fact]
  public async Task Restore_Campaign_WithNonExistentPublicId_ShouldFailAsync()
  {
    // Arrange
    var nonExistentPublicId = Faker.Random.AlphaNumeric(10);
    var restoreCommand = new RestoreCampaignCommand(nonExistentPublicId);

    // Act
    var result = await Sender.Send(restoreCommand);

    // Assert
    result.Should().NotBeNull();
    result.IsFailure.Should().BeTrue();
  }

  [Fact]
  public async Task Restore_Campaign_NotDeleted_ShouldSucceedAsync()
  {
    // Arrange
    var name = Faker.Commerce.Department();
    var slug = Faker.Lorem.Slug();
    var createCommand = new CreateCampaignCommand
    {
      Name = name,
      Slug = slug
    };

    var createResult = await Sender.Send(createCommand);
    var publicId = createResult.Value;

    var restoreCommand = new RestoreCampaignCommand(publicId);

    // Act
    var result = await Sender.Send(restoreCommand);

    // Assert
    result.Should().NotBeNull();
    result.IsSuccess.Should().BeTrue();

    var getQuery = new GetCampaignQuery(publicId);
    var campaign = await Sender.Send(getQuery);
    campaign.Value.IsDeleted.Should().BeFalse();
  }

  [Fact]
  public async Task Restore_Campaign_MultipleRestores_ShouldSucceedAsync()
  {
    // Arrange
    var name = Faker.Commerce.Department();
    var slug = Faker.Lorem.Slug();
    var createCommand = new CreateCampaignCommand
    {
      Name = name,
      Slug = slug
    };

    var createResult = await Sender.Send(createCommand);
    var publicId = createResult.Value;

    var deleteCommand = new DeleteCampaignCommand(publicId);
    await Sender.Send(deleteCommand);

    var restoreCommand1 = new RestoreCampaignCommand(publicId);
    await Sender.Send(restoreCommand1);

    var restoreCommand2 = new RestoreCampaignCommand(publicId);

    // Act
    var result = await Sender.Send(restoreCommand2);

    // Assert
    result.Should().NotBeNull();
    result.IsSuccess.Should().BeTrue();

    var getQuery = new GetCampaignQuery(publicId);
    var campaign = await Sender.Send(getQuery);
    campaign.Value.IsDeleted.Should().BeFalse();
  }
}
