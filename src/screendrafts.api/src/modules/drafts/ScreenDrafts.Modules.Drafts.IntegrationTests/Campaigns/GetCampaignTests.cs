using ScreenDrafts.Modules.Drafts.Features.Campaigns.Create;
using ScreenDrafts.Modules.Drafts.Features.Campaigns.Get;

namespace ScreenDrafts.Modules.Drafts.IntegrationTests.Campaigns;

public sealed class GetCampaignTests(DraftsIntegrationTestWebAppFactory factory) : DraftsIntegrationTest(factory)
{
  [Fact]
  public async Task Get_Campaign_ByPublicId_ShouldSucceedAsync()
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

    var query = new GetCampaignQuery(publicId);

    // Act
    var result = await Sender.Send(query);

    // Assert
    result.Should().NotBeNull();
    result.IsSuccess.Should().BeTrue();
    result.Value.Should().NotBeNull();
    result.Value.PublicId.Should().Be(publicId);
    result.Value.Name.Should().Be(name);
    result.Value.Slug.Should().Be(slug);
    result.Value.IsDeleted.Should().BeFalse();
  }

  [Fact]
  public async Task Get_Campaign_WithNonExistentPublicId_ShouldFailAsync()
  {
    // Arrange
    var nonExistentPublicId = Faker.Random.AlphaNumeric(10);
    var query = new GetCampaignQuery(nonExistentPublicId);

    // Act
    var result = await Sender.Send(query);

    // Assert
    result.Should().NotBeNull();
    result.IsFailure.Should().BeTrue();
  }
}
