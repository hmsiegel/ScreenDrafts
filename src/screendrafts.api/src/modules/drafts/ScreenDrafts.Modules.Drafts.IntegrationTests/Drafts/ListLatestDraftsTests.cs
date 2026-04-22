namespace ScreenDrafts.Modules.Drafts.IntegrationTests.Drafts;

public sealed class ListLatestDraftsTests(DraftsIntegrationTestWebAppFactory factory)
  : DraftsIntegrationTest(factory)
{
  // -------------------------------------------------------------------------
  // Happy path — empty
  // -------------------------------------------------------------------------

  [Fact]
  public async Task ListLatestDrafts_WhenNoCompletedDrafts_ShouldReturnEmptyAsync()
  {
    // Arrange
    var query = new ListLatestDraftsQuery { IncludePatreonOnly = false };

    // Act
    var result = await Sender.Send(query, TestContext.Current.CancellationToken);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Drafts.Should().BeEmpty();
  }

  // -------------------------------------------------------------------------
  // Happy path — returns completed drafts with main feed release
  // -------------------------------------------------------------------------

  [Fact]
  public async Task ListLatestDrafts_WhenCompletedDraftWithMainFeedRelease_ShouldReturnItAsync()
  {
    // Arrange
    var (draftPartPublicId, internalId) = await CreateDraftPartAsync();
    await SetStatusCompletedAsync(internalId);
    await AddMainFeedReleaseAsync(draftPartPublicId, new DateOnly(2025, 1, 1));

    var query = new ListLatestDraftsQuery { IncludePatreonOnly = false };

    // Act
    var result = await Sender.Send(query, TestContext.Current.CancellationToken);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Drafts.Should().HaveCount(1);
  }

  // -------------------------------------------------------------------------
  // Filter — no main feed release excluded when IncludePatreonOnly=false
  // -------------------------------------------------------------------------

  [Fact]
  public async Task ListLatestDrafts_WhenCompletedDraftHasNoMainFeedRelease_ShouldNotReturnItAsync()
  {
    // Arrange — completed but no release record at all
    var (_, internalId) = await CreateDraftPartAsync();
    await SetStatusCompletedAsync(internalId);

    var query = new ListLatestDraftsQuery { IncludePatreonOnly = false };

    // Act
    var result = await Sender.Send(query, TestContext.Current.CancellationToken);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Drafts.Should().BeEmpty();
  }

  // -------------------------------------------------------------------------
  // IncludePatreonOnly — returns completed drafts without requiring main feed release
  // -------------------------------------------------------------------------

  [Fact]
  public async Task ListLatestDrafts_WhenIncludePatreonOnly_ShouldReturnDraftsWithoutMainFeedReleaseAsync()
  {
    // Arrange — completed but no release record
    var (_, internalId) = await CreateDraftPartAsync();
    await SetStatusCompletedAsync(internalId);

    var query = new ListLatestDraftsQuery { IncludePatreonOnly = true };

    // Act
    var result = await Sender.Send(query, TestContext.Current.CancellationToken);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Drafts.Should().HaveCount(1);
  }

  // -------------------------------------------------------------------------
  // Ordering — newest release first
  // -------------------------------------------------------------------------

  [Fact]
  public async Task ListLatestDrafts_ShouldOrderByReleaseDateDescendingAsync()
  {
    // Arrange
    var (part1PublicId, internalId1) = await CreateDraftPartAsync();
    await SetStatusCompletedAsync(internalId1);
    await AddMainFeedReleaseAsync(part1PublicId, new DateOnly(2025, 1, 1));

    var (part2PublicId, internalId2) = await CreateDraftPartAsync();
    await SetStatusCompletedAsync(internalId2);
    await AddMainFeedReleaseAsync(part2PublicId, new DateOnly(2025, 6, 1));

    var query = new ListLatestDraftsQuery { IncludePatreonOnly = false };

    // Act
    var result = await Sender.Send(query, TestContext.Current.CancellationToken);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Drafts.Should().HaveCount(2);
    result.Value.Drafts.First().DraftPartPublicId.Should().Be(part2PublicId);
    result.Value.Drafts.Last().DraftPartPublicId.Should().Be(part1PublicId);
  }

  // -------------------------------------------------------------------------
  // Limit — returns at most 10
  // -------------------------------------------------------------------------

  [Fact]
  public async Task ListLatestDrafts_ShouldReturnAtMost10Async()
  {
    // Arrange — create 12 completed drafts with main feed releases
    for (int i = 1; i <= 12; i++)
    {
      var (draftPartPublicId, internalId) = await CreateDraftPartAsync();
      await SetStatusCompletedAsync(internalId);
      await AddMainFeedReleaseAsync(draftPartPublicId, new DateOnly(2025, 1, i));
    }

    var query = new ListLatestDraftsQuery { IncludePatreonOnly = false };

    // Act
    var result = await Sender.Send(query, TestContext.Current.CancellationToken);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Drafts.Should().HaveCount(10);
  }

  // -------------------------------------------------------------------------
  // Helpers
  // -------------------------------------------------------------------------

  private async Task<(string PublicId, Guid InternalId)> CreateDraftPartAsync()
  {
    var seriesId = await CreateSeriesAsync();
    var draftPublicId = await CreateDraftWithPartAsync(seriesId);
    var internalId = await GetFirstDraftPartIdAsync(draftPublicId);
    var draftPart = await DbContext.DraftParts.FirstAsync(dp => dp.Id == DraftPartId.Create(internalId), TestContext.Current.CancellationToken);
    return (draftPart.PublicId, internalId);
  }

  private async Task SetStatusCompletedAsync(Guid draftPartInternalId)
  {
    await DbContext.Database.ExecuteSqlRawAsync(
      "UPDATE drafts.draft_parts SET status = 3 WHERE id = {0}", draftPartInternalId);
  }

  private async Task AddMainFeedReleaseAsync(string draftPartPublicId, DateOnly releaseDate)
  {
    var result = await Sender.Send(new SetReleaseDateCommand
    {
      DraftPartId = draftPartPublicId,
      ReleaseDate = releaseDate,
      ReleaseChannel = ReleaseChannel.MainFeed
    }, TestContext.Current.CancellationToken);

    result.IsSuccess.Should().BeTrue();
  }

  private async Task<Guid> CreateSeriesAsync()
  {
    var result = await Sender.Send(new CreateSeriesCommand
    {
      Name = Faker.Company.CompanyName(),
      Kind = SeriesKind.Regular.Value,
      CanonicalPolicy = CanonicalPolicy.Always.Value,
      ContinuityScope = ContinuityScope.None.Value,
      ContinuityDateRule = ContinuityDateRule.AnyChannelFirstRelease.Value,
      AllowedDraftTypes = (int)DraftTypeMask.All,
      DefaultDraftType = DraftType.Standard.Value
    }, TestContext.Current.CancellationToken);

    return result.Value;
  }

  private async Task<string> CreateDraftWithPartAsync(Guid seriesId)
  {
    var draftResult = await Sender.Send(new CreateDraftCommand
    {
      Title = Faker.Company.CompanyName(),
      DraftType = DraftType.Standard.Value,
      SeriesId = seriesId,
    }, TestContext.Current.CancellationToken);

    var draftPublicId = draftResult.Value;
    await Sender.Send(new CreateDraftPartCommand
    {
      DraftPublicId = draftPublicId,
      PartIndex = 1,
      MinimumPosition = 1,
      MaximumPosition = 7,
    }, TestContext.Current.CancellationToken);

    return draftPublicId;
  }
}
