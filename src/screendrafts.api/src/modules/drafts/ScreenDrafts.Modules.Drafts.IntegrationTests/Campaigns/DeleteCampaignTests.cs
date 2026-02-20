using ScreenDrafts.Modules.Drafts.Features.Campaigns.Create;
using ScreenDrafts.Modules.Drafts.Features.Campaigns.Delete;
using ScreenDrafts.Modules.Drafts.Features.Campaigns.Get;

namespace ScreenDrafts.Modules.Drafts.IntegrationTests.Campaigns;

public sealed class DeleteCampaignTests(DraftsIntegrationTestWebAppFactory factory) : DraftsIntegrationTest(factory)
{
  [Fact]
  public async Task Delete_Campaign_ShouldSucceedAsync()
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

    // Act
    var result = await Sender.Send(deleteCommand);

    // Assert
    result.Should().NotBeNull();
    result.IsSuccess.Should().BeTrue();

    var getQuery = new GetCampaignQuery(publicId, IncludeDeleted: true);
    var campaign = await Sender.Send(getQuery);
    campaign.Value.IsDeleted.Should().BeTrue();
  }

  [Fact]
  public async Task Delete_Campaign_WithNonExistentPublicId_ShouldFailAsync()
  {
    // Arrange
    var nonExistentPublicId = Faker.Random.AlphaNumeric(10);
    var deleteCommand = new DeleteCampaignCommand(nonExistentPublicId);

    // Act
    var result = await Sender.Send(deleteCommand);

    // Assert
    result.Should().NotBeNull();
    result.IsFailure.Should().BeTrue();
  }

  [Fact]
  public async Task Delete_Campaign_AlreadyDeleted_ShouldSucceedAsync()
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

    var deleteCommand1 = new DeleteCampaignCommand(publicId);
    await Sender.Send(deleteCommand1);

    var deleteCommand2 = new DeleteCampaignCommand(publicId);

    // Act
    var result = await Sender.Send(deleteCommand2);

    // Assert
    result.Should().NotBeNull();
    result.IsSuccess.Should().BeTrue();

    var getQuery = new GetCampaignQuery(publicId, IncludeDeleted: true);
    var campaign = await Sender.Send(getQuery);
    campaign.Value.IsDeleted.Should().BeTrue();
  }
}
