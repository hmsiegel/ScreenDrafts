using ScreenDrafts.Modules.Drafts.Domain.DraftParts.Enums;
using ScreenDrafts.Modules.Drafts.Features.Campaigns.Create;
using ScreenDrafts.Modules.Drafts.Features.Categories.Create;
using ScreenDrafts.Modules.Drafts.Features.Drafts.Search;
using ScreenDrafts.Modules.Drafts.Features.Drafts.SetCampaign;
using ScreenDrafts.Modules.Drafts.Features.Drafts.SetCategories;

namespace ScreenDrafts.Modules.Drafts.IntegrationTests.Drafts;

public sealed class SearchDraftsTests(DraftsIntegrationTestWebAppFactory factory)
  : DraftsIntegrationTest(factory)
{
  // ─────────────────────────────────────────────────────────────────────────
  // Empty list
  // ─────────────────────────────────────────────────────────────────────────

  [Fact]
  public async Task SearchDrafts_WithNoDrafts_ShouldReturnEmptyAsync()
  {
    // Act
    var result = await Sender.Send(new SearchDraftsQuery { Page = 1, PageSize = 10 }, TestContext.Current.CancellationToken);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Items.Should().BeEmpty();
    result.Value.TotalCount.Should().Be(0);
  }

  // ─────────────────────────────────────────────────────────────────────────
  // Basic list
  // ─────────────────────────────────────────────────────────────────────────

  [Fact]
  public async Task SearchDrafts_WithNoFilter_ShouldReturnAllDraftsAsync()
  {
    // Arrange
    await CreateDraftAsync();
    await CreateDraftAsync();
    await CreateDraftAsync();

    // Act
    var result = await Sender.Send(new SearchDraftsQuery { Page = 1, PageSize = 10 }, TestContext.Current.CancellationToken);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Items.Should().HaveCount(3);
    result.Value.TotalCount.Should().Be(3);
  }

  // ─────────────────────────────────────────────────────────────────────────
  // Name filter
  // ─────────────────────────────────────────────────────────────────────────

  [Fact]
  public async Task SearchDrafts_ByName_ShouldReturnMatchingDraftsAsync()
  {
    // Arrange
    var uniquePrefix = "UniqueSearch_" + Faker.Random.AlphaNumeric(8);
    var matchId = await CreateDraftAsync(title: uniquePrefix + "_Draft");
    await CreateDraftAsync(title: "SomethingCompletelyDifferent");

    // Act
    var result = await Sender.Send(new SearchDraftsQuery { Name = uniquePrefix }, TestContext.Current.CancellationToken);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.TotalCount.Should().Be(1);
    result.Value.Items.Single().PublicId.Should().Be(matchId);
  }

  [Fact]
  public async Task SearchDrafts_ByName_ShouldBeCaseInsensitiveAsync()
  {
    // Arrange
    var title = "CaseSensitiveTitle_" + Faker.Random.AlphaNumeric(6);
    await CreateDraftAsync(title: title);

    // Act
    var result = await Sender.Send(new SearchDraftsQuery { Name = title.ToUpperInvariant() }, TestContext.Current.CancellationToken);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.TotalCount.Should().Be(1);
    result.Value.Items.Single().Title.Should().Be(title);
  }

  [Fact]
  public async Task SearchDrafts_ByName_ShouldMatchPartiallyAsync()
  {
    // Arrange
    var suffix = "PartialKey_" + Faker.Random.AlphaNumeric(6);
    await CreateDraftAsync(title: "Prefix_" + suffix);

    // Act
    var result = await Sender.Send(new SearchDraftsQuery { Name = suffix }, TestContext.Current.CancellationToken);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.TotalCount.Should().Be(1);
    result.Value.Items.Single().Title.Should().Contain(suffix);
  }

  [Fact]
  public async Task SearchDrafts_ByName_WithNoMatch_ShouldReturnEmptyAsync()
  {
    // Arrange
    await CreateDraftAsync();

    // Act
    var result = await Sender.Send(new SearchDraftsQuery { Name = "zzz_no_match_" + Faker.Random.AlphaNumeric(12) }, TestContext.Current.CancellationToken);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Items.Should().BeEmpty();
    result.Value.TotalCount.Should().Be(0);
  }

  // ─────────────────────────────────────────────────────────────────────────
  // Campaign filter
  // ─────────────────────────────────────────────────────────────────────────

  [Fact]
  public async Task SearchDrafts_ByCampaign_ShouldReturnOnlyDraftsWithThatCampaignAsync()
  {
    // Arrange
    var campaignId = await CreateCampaignAsync();
    var matchId = await CreateDraftAsync();
    await CreateDraftAsync(); // no campaign

    await Sender.Send(new SetCampaignDraftCommand { DraftId = matchId, CampaignId = campaignId }, TestContext.Current.CancellationToken);

    // Act
    var result = await Sender.Send(new SearchDraftsQuery { CampaignPublicId = campaignId }, TestContext.Current.CancellationToken);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.TotalCount.Should().Be(1);
    result.Value.Items.Single().PublicId.Should().Be(matchId);
    result.Value.Items.Single().CampaignPublicId.Should().Be(campaignId);
  }

  [Fact]
  public async Task SearchDrafts_ByCampaign_WithNoMatch_ShouldReturnEmptyAsync()
  {
    // Arrange
    await CreateDraftAsync(); // draft with no campaign
    var unusedCampaignId = await CreateCampaignAsync();

    // Act
    var result = await Sender.Send(new SearchDraftsQuery { CampaignPublicId = unusedCampaignId }, TestContext.Current.CancellationToken);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Items.Should().BeEmpty();
  }

  // ─────────────────────────────────────────────────────────────────────────
  // Category filter
  // ─────────────────────────────────────────────────────────────────────────

  [Fact]
  public async Task SearchDrafts_ByCategory_ShouldReturnOnlyDraftsWithThatCategoryAsync()
  {
    // Arrange
    var categoryId = await CreateCategoryAsync();
    var matchId = await CreateDraftAsync();
    await CreateDraftAsync(); // no category

    await Sender.Send(new SetCategoriesDraftCommand { DraftId = matchId, CategoryIds = [categoryId] }, TestContext.Current.CancellationToken);

    // Act
    var result = await Sender.Send(new SearchDraftsQuery { CategoryPublicId = categoryId }, TestContext.Current.CancellationToken);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.TotalCount.Should().Be(1);
    result.Value.Items.Single().PublicId.Should().Be(matchId);
  }

  [Fact]
  public async Task SearchDrafts_ByCategory_WithNoMatch_ShouldReturnEmptyAsync()
  {
    // Arrange
    await CreateDraftAsync();
    var unusedCategoryId = await CreateCategoryAsync();

    // Act
    var result = await Sender.Send(new SearchDraftsQuery { CategoryPublicId = unusedCategoryId }, TestContext.Current.CancellationToken);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Items.Should().BeEmpty();
  }

  // ─────────────────────────────────────────────────────────────────────────
  // DraftType filter
  // ─────────────────────────────────────────────────────────────────────────

  [Fact]
  public async Task SearchDrafts_ByDraftType_ShouldReturnOnlyMatchingTypeAsync()
  {
    // Arrange
    await CreateDraftAsync(draftType: DraftType.Standard);
    await CreateDraftAsync(draftType: DraftType.MiniMega);

    // Act
    var result = await Sender.Send(new SearchDraftsQuery { DraftType = DraftType.Standard.Value }, TestContext.Current.CancellationToken);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.TotalCount.Should().Be(1);
    result.Value.Items.Single().DraftType.Should().Be(DraftType.Standard.Value);
  }

  // ─────────────────────────────────────────────────────────────────────────
  // Pagination
  // ─────────────────────────────────────────────────────────────────────────

  [Fact]
  public async Task SearchDrafts_WithPagination_ShouldReturnCorrectPageAsync()
  {
    // Arrange – create 5 drafts
    for (var i = 0; i < 5; i++)
    {
      await CreateDraftAsync();
    }

    // Act
    var result = await Sender.Send(new SearchDraftsQuery { Page = 2, PageSize = 2 }, TestContext.Current.CancellationToken);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Items.Should().HaveCount(2);
    result.Value.TotalCount.Should().Be(5);
    result.Value.Page.Should().Be(2);
    result.Value.TotalPages.Should().Be(3);
    result.Value.HasPreviousPage.Should().BeTrue();
    result.Value.HasNextPage.Should().BeTrue();
  }

  [Fact]
  public async Task SearchDrafts_PageSizeCappedAt100Async()
  {
    // Arrange
    await CreateDraftAsync();

    // Act
    var result = await Sender.Send(new SearchDraftsQuery { Page = 1, PageSize = 1000 }, TestContext.Current.CancellationToken);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.PageSize.Should().Be(100);
  }

  // ─────────────────────────────────────────────────────────────────────────
  // Response shape
  // ─────────────────────────────────────────────────────────────────────────

  [Fact]
  public async Task SearchDrafts_ShouldPopulateResponseFieldsAsync()
  {
    // Arrange
    const string title = "Shape Test Draft";
    var draftId = await CreateDraftAsync(title: title);
    var campaignId = await CreateCampaignAsync();
    await Sender.Send(new SetCampaignDraftCommand { DraftId = draftId, CampaignId = campaignId }, TestContext.Current.CancellationToken);

    // Act
    var result = await Sender.Send(new SearchDraftsQuery { Name = title }, TestContext.Current.CancellationToken);

    // Assert
    result.IsSuccess.Should().BeTrue();
    var item = result.Value.Items.Single();
    item.PublicId.Should().Be(draftId);
    item.Title.Should().Be(title);
    item.DraftType.Should().Be(DraftType.Standard.Value);
    item.CampaignPublicId.Should().Be(campaignId);
    item.CampaignName.Should().NotBeNullOrEmpty();
    item.SeriesPublicId.Should().NotBeNullOrEmpty();
    item.SeriesName.Should().NotBeNullOrEmpty();
  }

  [Fact]
  public async Task SearchDrafts_WithNoCampaign_CampaignFieldsShouldBeNullAsync()
  {
    // Arrange
    const string title = "No Campaign Draft";
    await CreateDraftAsync(title: title);

    // Act
    var result = await Sender.Send(new SearchDraftsQuery { Name = title }, TestContext.Current.CancellationToken);

    // Assert
    result.IsSuccess.Should().BeTrue();
    var item = result.Value.Items.Single();
    item.CampaignPublicId.Should().BeNull();
    item.CampaignName.Should().BeNull();
  }

  // ─────────────────────────────────────────────────────────────────────────
  // Ordering
  // ─────────────────────────────────────────────────────────────────────────

  [Fact]
  public async Task SearchDrafts_ShouldReturnResultsOrderedByTitleAscAsync()
  {
    // Arrange – use a shared prefix to isolate ordering
    var prefix = "Order_" + Faker.Random.AlphaNumeric(6) + "_";
    await CreateDraftAsync(title: prefix + "C");
    await CreateDraftAsync(title: prefix + "A");
    await CreateDraftAsync(title: prefix + "B");

    // Act
    var result = await Sender.Send(new SearchDraftsQuery { Name = prefix }, TestContext.Current.CancellationToken);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Items.Should().HaveCount(3);
    result.Value.Items[0].Title.Should().EndWith("A");
    result.Value.Items[1].Title.Should().EndWith("B");
    result.Value.Items[2].Title.Should().EndWith("C");
  }

  // ─────────────────────────────────────────────────────────────────────────
  // Helpers
  // ─────────────────────────────────────────────────────────────────────────

  private async Task<string> CreateDraftAsync(string? title = null, DraftType? draftType = null)
  {
    var seriesId = await CreateSeriesAsync();
    var result = await Sender.Send(new CreateDraftCommand
    {
      Title = title ?? Faker.Company.CompanyName() + Faker.Random.AlphaNumeric(6),
      DraftType = (draftType ?? DraftType.Standard).Value,
      SeriesId = seriesId
    }, TestContext.Current.CancellationToken);
    return result.Value;
  }

  private async Task<Guid> CreateSeriesAsync()
  {
    var result = await Sender.Send(new CreateSeriesCommand
    {
      Name = Faker.Company.CompanyName() + Faker.Random.AlphaNumeric(6),
      Kind = SeriesKind.Regular.Value,
      CanonicalPolicy = CanonicalPolicy.Always.Value,
      ContinuityScope = ContinuityScope.None.Value,
      ContinuityDateRule = ContinuityDateRule.AnyChannelFirstRelease.Value,
      AllowedDraftTypes = (int)DraftTypeMask.All,
      DefaultDraftType = DraftType.Standard.Value
    }, TestContext.Current.CancellationToken);
    return result.Value;
  }

  private async Task<string> CreateCampaignAsync()
  {
    var result = await Sender.Send(new CreateCampaignCommand
    {
      Name = Faker.Commerce.Department() + Faker.Random.AlphaNumeric(6),
      Slug = Faker.Internet.DomainWord() + Faker.Random.AlphaNumeric(6)
    }, TestContext.Current.CancellationToken);
    return result.Value;
  }

  private async Task<string> CreateCategoryAsync()
  {
    var result = await Sender.Send(new CreateCategoryCommand
    {
      Name = Faker.Commerce.Categories(1)[0] + Faker.Random.AlphaNumeric(8),
      Description = Faker.Lorem.Sentence()
    }, TestContext.Current.CancellationToken);
    return result.Value;
  }
}
