using ScreenDrafts.Modules.Drafts.Features.Campaigns.Create;
using ScreenDrafts.Modules.Drafts.Features.Campaigns.Delete;
using ScreenDrafts.Modules.Drafts.Features.Campaigns.List;

namespace ScreenDrafts.Modules.Drafts.IntegrationTests.Campaigns;

public sealed class ListCampaignsTests(DraftsIntegrationTestWebAppFactory factory) : DraftsIntegrationTest(factory)
{
  [Fact]
  public async Task List_Campaigns_ShouldReturnAllNonDeletedCampaignsAsync()
  {
    // Arrange
    var campaign1 = new CreateCampaignCommand
    {
      Name = Faker.Commerce.Department(),
      Slug = Faker.Lorem.Slug()
    };
    await Sender.Send(campaign1);

    var campaign2 = new CreateCampaignCommand
    {
      Name = Faker.Commerce.Department(),
      Slug = Faker.Lorem.Slug()
    };
    await Sender.Send(campaign2);

    var query = new ListCampaignsQuery(IncludeDeleted: false);

    // Act
    var result = await Sender.Send(query);

    // Assert
    result.Should().NotBeNull();
    result.IsSuccess.Should().BeTrue();
    result.Value.Items.Should().HaveCountGreaterThanOrEqualTo(2);
    result.Value.Items.Should().OnlyContain(c => !c.IsDeleted);
  }

  [Fact]
  public async Task List_Campaigns_WithIncludeDeleted_ShouldReturnAllCampaignsAsync()
  {
    // Arrange
    var campaign1Command = new CreateCampaignCommand
    {
      Name = Faker.Commerce.Department(),
      Slug = Faker.Lorem.Slug()
    };
    await Sender.Send(campaign1Command);

    var campaign2Command = new CreateCampaignCommand
    {
      Name = Faker.Commerce.Department(),
      Slug = Faker.Lorem.Slug()
    };
    var campaign2Result = await Sender.Send(campaign2Command);

    var deleteCommand = new DeleteCampaignCommand(campaign2Result.Value);
    await Sender.Send(deleteCommand);

    var query = new ListCampaignsQuery(IncludeDeleted: true);

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
  public async Task List_Campaigns_WithoutIncludeDeleted_ShouldOnlyReturnNonDeletedCampaignsAsync()
  {
    // Arrange
    var campaign1Command = new CreateCampaignCommand
    {
      Name = Faker.Commerce.Department(),
      Slug = Faker.Lorem.Slug()
    };
    await Sender.Send(campaign1Command);

    var campaign2Command = new CreateCampaignCommand
    {
      Name = Faker.Commerce.Department(),
      Slug = Faker.Lorem.Slug()
    };
    var campaign2Result = await Sender.Send(campaign2Command);

    var deleteCommand = new DeleteCampaignCommand(campaign2Result.Value);
    await Sender.Send(deleteCommand);

    var query = new ListCampaignsQuery(IncludeDeleted: false);

    // Act
    var result = await Sender.Send(query);

    // Assert
    result.Should().NotBeNull();
    result.IsSuccess.Should().BeTrue();
    result.Value.Items.Should().NotContain(c => c.IsDeleted);
  }

  [Fact]
  public async Task List_Campaigns_EmptyDatabase_ShouldReturnEmptyCollectionAsync()
  {
    // Arrange
    var query = new ListCampaignsQuery(IncludeDeleted: false);

    // Act
    var result = await Sender.Send(query);

    // Assert
    result.Should().NotBeNull();
    result.IsSuccess.Should().BeTrue();
    result.Value.Items.Should().BeEmpty();
  }
}
