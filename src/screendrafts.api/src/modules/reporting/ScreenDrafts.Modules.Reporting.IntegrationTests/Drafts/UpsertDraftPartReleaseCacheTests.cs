namespace ScreenDrafts.Modules.Reporting.IntegrationTests.Drafts;

public sealed class UpsertDraftPartReleaseCacheTests(ReportingIntegrationTestWebAppFactory factory)
  : ReportingIntegrationTest(factory)
{
  private static readonly DateOnly _release = new(2026, 2, 3);

  private async Task PrimeCacheAsync()
  {
    var cache = GetService<IDistributedCache>();
    await cache.SetStringAsync(ReportingCacheKeys.RecordBookCanonicalCacheKey, "stale", TestContext.Current.CancellationToken);
    await cache.SetStringAsync(ReportingCacheKeys.RecordBookAllCacheKey, "stale", TestContext.Current.CancellationToken);
  }

  private async Task<(string? Canonical, string? All)> ReadCacheAsync()
  {
    var cache = GetService<IDistributedCache>();
    return (
      await cache.GetStringAsync(ReportingCacheKeys.RecordBookCanonicalCacheKey, TestContext.Current.CancellationToken),
      await cache.GetStringAsync(ReportingCacheKeys.RecordBookAllCacheKey, TestContext.Current.CancellationToken));
  }

  private Task<Result> UpsertAsync(string channel, string part = "dp_release1") =>
    Sender.Send(
      new UpsertDraftPartReleaseCommand
      {
        DraftId = Guid.NewGuid(),
        DraftPartPublicId = part,
        ReleaseChannel = channel,
        ReleaseDate = _release,
      },
      TestContext.Current.CancellationToken);

  [Fact]
  public async Task Handle_ShouldClearBothRecordBookCacheKeys_WhenAMainFeedReleaseIsUpsertedAsync()
  {
    await PrimeCacheAsync();

    var result = await UpsertAsync("MainFeed");

    result.IsSuccess.Should().BeTrue();
    (await ReadCacheAsync()).Should().Be((null, null));
  }

  [Fact]
  public async Task Handle_ShouldClearBothKeys_WhenAnExistingMainFeedReleaseIsUpdatedAsync()
  {
    await UpsertAsync("MainFeed");
    await PrimeCacheAsync();

    await UpsertAsync("MainFeed");

    (await ReadCacheAsync()).Should().Be((null, null));
    (await DbContext.DraftPartsReleases.CountAsync(r => r.DraftPartPublicId == "dp_release1", TestContext.Current.CancellationToken))
      .Should().Be(1);
  }

  [Fact]
  public async Task Handle_ShouldLeaveTheRecordBookCache_WhenTheChannelIsNotMainFeedAsync()
  {
    await PrimeCacheAsync();

    var result = await UpsertAsync("Patreon");

    result.IsSuccess.Should().BeTrue();
    (await ReadCacheAsync()).Should().Be(("stale", "stale"));
  }
}
