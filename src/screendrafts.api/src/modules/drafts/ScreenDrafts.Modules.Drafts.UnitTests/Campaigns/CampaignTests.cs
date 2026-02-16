namespace ScreenDrafts.Modules.Drafts.UnitTests.Campaigns;

public class CampaignTests : BaseTest
{
  [Fact]
  public void Create_ShouldReturnSuccessResult_WhenValidParametersAreProvided()
  {
    // Arrange
    var slug = Faker.Internet.DomainWord();
    var name = Faker.Lorem.Word();
    var publicId = Faker.Random.AlphaNumeric(10);

    // Act
    var result = Campaign.Create(slug, name, publicId);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Slug.Should().Be(slug);
    result.Value.Name.Should().Be(name);
    result.Value.PublicId.Should().Be(publicId);
    result.Value.IsDeleted.Should().BeFalse();
  }

  [Fact]
  public void Rename_ShouldUpdateName_WhenValidNameProvided()
  {
    // Arrange
    var campaign = CreateCampaign();
    var newName = Faker.Lorem.Word();

    // Act
    var result = campaign.Rename(newName);

    // Assert
    result.IsSuccess.Should().BeTrue();
    campaign.Name.Should().Be(newName);
    campaign.UpdatedAtUtc.Should().NotBeNull();
  }

  [Fact]
  public void ChangeSlug_ShouldUpdateSlug_WhenValidSlugProvided()
  {
    // Arrange
    var campaign = CreateCampaign();
    var newSlug = Faker.Internet.DomainWord();

    // Act
    var result = campaign.ChangeSlug(newSlug);

    // Assert
    result.IsSuccess.Should().BeTrue();
    campaign.Slug.Should().Be(newSlug);
    campaign.UpdatedAtUtc.Should().NotBeNull();
  }

  [Fact]
  public void SoftDelete_ShouldMarkCampaignAsDeleted()
  {
    // Arrange
    var campaign = CreateCampaign();

    // Act
    var result = campaign.SoftDelete();

    // Assert
    result.IsSuccess.Should().BeTrue();
    campaign.IsDeleted.Should().BeTrue();
    campaign.UpdatedAtUtc.Should().NotBeNull();
  }

  [Fact]
  public void SoftDelete_ShouldSucceed_WhenAlreadyDeleted()
  {
    // Arrange
    var campaign = CreateCampaign();
    campaign.SoftDelete();

    // Act
    var result = campaign.SoftDelete();

    // Assert
    result.IsSuccess.Should().BeTrue();
    campaign.IsDeleted.Should().BeTrue();
  }

  [Fact]
  public void Restore_ShouldMarkCampaignAsNotDeleted()
  {
    // Arrange
    var campaign = CreateCampaign();
    campaign.SoftDelete();

    // Act
    var result = campaign.Restore();

    // Assert
    result.IsSuccess.Should().BeTrue();
    campaign.IsDeleted.Should().BeFalse();
    campaign.UpdatedAtUtc.Should().NotBeNull();
  }

  [Fact]
  public void Restore_ShouldSucceed_WhenNotDeleted()
  {
    // Arrange
    var campaign = CreateCampaign();

    // Act
    var result = campaign.Restore();

    // Assert
    result.IsSuccess.Should().BeTrue();
    campaign.IsDeleted.Should().BeFalse();
  }

  private static Campaign CreateCampaign()
  {
    return Campaign.Create(
      slug: Faker.Internet.DomainWord(),
      name: Faker.Lorem.Word(),
      publicId: Faker.Random.AlphaNumeric(10)).Value;
  }
}
