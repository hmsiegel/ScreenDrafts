namespace ScreenDrafts.Modules.Reporting.IntegrationTests.Honorifics;

/// <summary>The honorific counts distinct drafts: a multi-part draft counts once.</summary>
public sealed class UpdateDrafterHonorificsPerDraftTests(ReportingIntegrationTestWebAppFactory factory)
  : ReportingIntegrationTest(factory)
{
  private static readonly Guid _drafterId = new(42, 0, 0, new byte[8]);

  private Task<Result> SendAsync(Guid draftId, string part, int policy = 0, bool hasRelease = false) =>
    Sender.Send(
      new UpdateDrafterHonorificsCommand
      {
        DrafterIdValue = _drafterId,
        DraftId = draftId,
        DraftPartPublicId = part,
        CanonicalPolicyValue = policy,
        HasMainFeedRelease = hasRelease,
      },
      TestContext.Current.CancellationToken);

  private Task<DrafterHonorificEntity?> HonorificAsync() =>
    DbContext
      .DrafterHonorifics.AsNoTracking()
      .FirstOrDefaultAsync(h => h.DrafterIdValue == _drafterId, TestContext.Current.CancellationToken);

  private static Guid Draft(int n) => new(n, 5, 0, new byte[8]);

  [Fact]
  public async Task Handle_ShouldCountOnce_WhenFivePartsBelongToOneDraftAsync()
  {
    for (var part = 1; part <= 5; part++)
    {
      await SendAsync(Draft(1), $"dp_one_{part}");
    }

    var honorific = await HonorificAsync();

    honorific!.AppearanceCount.Should().Be(1);
    honorific.Honorific.Should().Be(DrafterHonorific.None);
    (await DbContext.DrafterCanonicalAppearances.CountAsync(a => a.DrafterIdValue == _drafterId, TestContext.Current.CancellationToken))
      .Should().Be(5, "every part is still recorded as an appearance row");
  }

  [Fact]
  public async Task Handle_ShouldNotRaiseTheHonorific_WhenAnotherPartOfACountedDraftArrivesAsync()
  {
    for (var draft = 1; draft <= 5; draft++)
    {
      await SendAsync(Draft(draft), $"dp_d{draft}_1");
    }

    var before = await HonorificAsync();
    before!.Honorific.Should().Be(DrafterHonorific.AllStar);
    before.AppearanceCount.Should().Be(5);

    // A second part of draft 5 must change nothing.
    await SendAsync(Draft(5), "dp_d5_2");

    var after = await HonorificAsync();
    after!.AppearanceCount.Should().Be(5);
    after.Honorific.Should().Be(DrafterHonorific.AllStar);

    (await DbContext.DraftersHonorificHistory.CountAsync(h => h.DrafterIdValue == _drafterId, TestContext.Current.CancellationToken))
      .Should().Be(1, "only the move from None to All-Star is recorded");
  }

  [Fact]
  public async Task Handle_ShouldReachAllStar_WhenFiveDifferentDraftsAreCountedAsync()
  {
    for (var draft = 1; draft <= 5; draft++)
    {
      // Two parts each: the count is still five.
      await SendAsync(Draft(draft), $"dp_e{draft}_1");
      await SendAsync(Draft(draft), $"dp_e{draft}_2");
    }

    var honorific = await HonorificAsync();

    honorific!.AppearanceCount.Should().Be(5);
    honorific.Honorific.Should().Be(DrafterHonorific.AllStar);
  }

  [Fact]
  public async Task Handle_ShouldOnlyCountPartsWithAMainFeedRelease_WhenPolicyIsOnMainFeedAsync()
  {
    await SendAsync(Draft(1), "dp_f1", policy: 2, hasRelease: false);
    await SendAsync(Draft(2), "dp_f2", policy: 2, hasRelease: true);

    (await HonorificAsync())!.AppearanceCount.Should().Be(1);
  }
}
