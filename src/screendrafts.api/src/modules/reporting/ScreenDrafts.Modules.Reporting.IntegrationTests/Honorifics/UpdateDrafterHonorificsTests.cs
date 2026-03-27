namespace ScreenDrafts.Modules.Reporting.IntegrationTests.Honorifics;

public sealed class UpdateDrafterHonorificsTests(ReportingIntegrationTestWebAppFactory factory)
  : ReportingIntegrationTest(factory)
{
  private static readonly Faker _faker = new();

  // -------------------------------------------------------------------------
  // Happy path — first appearance
  // -------------------------------------------------------------------------

  [Fact]
  public async Task Handle_ShouldInsertCanonicalAppearance_WhenDrafterAppearesForFirstTimeAsync()
  {
    // Arrange
    var drafterId = Guid.NewGuid();
    var command = BuildCommand(drafterId, _faker.Random.AlphaNumeric(10));

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.IsSuccess.Should().BeTrue();

    var appearance = await DbContext.DrafterCanonicalAppearances
      .FirstOrDefaultAsync(a => a.DrafterIdValue == drafterId);

    appearance.Should().NotBeNull();
    appearance!.DraftPartPublicId.Should().Be(command.DraftPartPublicId);
  }

  // -------------------------------------------------------------------------
  // Idempotency — duplicate appearance is ignored
  // -------------------------------------------------------------------------

  [Fact]
  public async Task Handle_ShouldBeIdempotent_WhenSameDrafterAndPartAppearTwiceAsync()
  {
    // Arrange
    var drafterId = Guid.NewGuid();
    var partPublicId = _faker.Random.AlphaNumeric(10);
    var command = BuildCommand(drafterId, partPublicId);

    // Act
    await Sender.Send(command);
    var result = await Sender.Send(command); // second call with same drafter + part

    // Assert
    result.IsSuccess.Should().BeTrue();

    var count = await DbContext.DrafterCanonicalAppearances
      .CountAsync(a => a.DrafterIdValue == drafterId);

    count.Should().Be(1, "duplicate appearances must be ignored");
  }

  // -------------------------------------------------------------------------
  // Honorific progression — AllStar at 5 appearances
  // -------------------------------------------------------------------------

  [Fact]
  public async Task Handle_ShouldSetAllStarHonorific_WhenDrafterReachesFiveAppearancesAsync()
  {
    // Arrange
    var drafterId = Guid.NewGuid();
    await SendAppearancesAsync(drafterId, count: 5, canonicalPolicyValue: 0);

    // Assert
    var honorific = await DbContext.DrafterHonorifics
      .FirstOrDefaultAsync(h => h.DrafterIdValue == drafterId);

    honorific.Should().NotBeNull();
    honorific!.Honorific.Should().Be(DrafterHonorific.AllStar);
    honorific.AppearanceCount.Should().Be(5);
  }

  // -------------------------------------------------------------------------
  // Honorific progression — HallOfFame at 10 appearances
  // -------------------------------------------------------------------------

  [Fact]
  public async Task Handle_ShouldSetHallOfFameHonorific_WhenDrafterReachesTenAppearancesAsync()
  {
    // Arrange
    var drafterId = Guid.NewGuid();
    await SendAppearancesAsync(drafterId, count: 10, canonicalPolicyValue: 0);

    // Assert
    var honorific = await DbContext.DrafterHonorifics
      .FirstOrDefaultAsync(h => h.DrafterIdValue == drafterId);

    honorific.Should().NotBeNull();
    honorific!.Honorific.Should().Be(DrafterHonorific.HallOfFame);
    honorific.AppearanceCount.Should().Be(10);
  }

  // -------------------------------------------------------------------------
  // Honorific stays None — below threshold
  // -------------------------------------------------------------------------

  [Fact]
  public async Task Handle_ShouldKeepNoneHonorific_WhenCountIsBelowFiveAsync()
  {
    // Arrange
    var drafterId = Guid.NewGuid();
    await SendAppearancesAsync(drafterId, count: 4, canonicalPolicyValue: 0);

    // Assert
    var honorific = await DbContext.DrafterHonorifics
      .FirstOrDefaultAsync(h => h.DrafterIdValue == drafterId);

    honorific.Should().NotBeNull();
    honorific!.Honorific.Should().Be(DrafterHonorific.None);
    honorific.AppearanceCount.Should().Be(4);
  }

  // -------------------------------------------------------------------------
  // OnMainFeed policy — non-main-feed appearances not counted
  // -------------------------------------------------------------------------

  [Fact]
  public async Task Handle_ShouldNotCountAppearance_WhenPolicyIsOnMainFeedButNoReleaseAsync()
  {
    // Arrange
    var drafterId = Guid.NewGuid();

    // 5 appearances WITHOUT main feed release, policy = 2 (OnMainFeed)
    for (var i = 0; i < 5; i++)
    {
      var command = new UpdateDrafterHonorificsCommand
      {
        DrafterIdValue = drafterId,
        DraftPartPublicId = _faker.Random.AlphaNumeric(10),
        CanonicalPolicyValue = 2,
        HasMainFeedRelease = false
      };
      await Sender.Send(command);
    }

    // Assert — appearances are recorded but count is 0 for the policy
    var appearanceCount = await DbContext.DrafterCanonicalAppearances
      .CountAsync(a => a.DrafterIdValue == drafterId);

    appearanceCount.Should().Be(5, "appearances are still recorded");

    var honorific = await DbContext.DrafterHonorifics
      .FirstOrDefaultAsync(h => h.DrafterIdValue == drafterId);

    honorific.Should().NotBeNull();
    honorific!.Honorific.Should().Be(DrafterHonorific.None,
      "appearances without main feed release don't count toward honorific when policy is OnMainFeed");
    honorific.AppearanceCount.Should().Be(0);
  }

  // -------------------------------------------------------------------------
  // OnMainFeed policy — main-feed appearances counted
  // -------------------------------------------------------------------------

  [Fact]
  public async Task Handle_ShouldCountAppearance_WhenPolicyIsOnMainFeedAndHasReleaseAsync()
  {
    // Arrange
    var drafterId = Guid.NewGuid();

    // 5 appearances WITH main feed release, policy = 2 (OnMainFeed)
    for (var i = 0; i < 5; i++)
    {
      var command = new UpdateDrafterHonorificsCommand
      {
        DrafterIdValue = drafterId,
        DraftPartPublicId = _faker.Random.AlphaNumeric(10),
        CanonicalPolicyValue = 2,
        HasMainFeedRelease = true
      };
      await Sender.Send(command);
    }

    // Assert
    var honorific = await DbContext.DrafterHonorifics
      .FirstOrDefaultAsync(h => h.DrafterIdValue == drafterId);

    honorific.Should().NotBeNull();
    honorific!.Honorific.Should().Be(DrafterHonorific.AllStar);
    honorific.AppearanceCount.Should().Be(5);
  }

  // -------------------------------------------------------------------------
  // History — written when honorific changes
  // -------------------------------------------------------------------------

  [Fact]
  public async Task Handle_ShouldWriteHistory_WhenHonorificChangesAsync()
  {
    // Arrange
    var drafterId = Guid.NewGuid();

    // 4 appearances: honorific stays None
    await SendAppearancesAsync(drafterId, count: 4, canonicalPolicyValue: 0);

    // 1 more appearance to trigger AllStar
    await Sender.Send(BuildCommand(drafterId, _faker.Random.AlphaNumeric(10)));

    // Assert — history row should exist for the AllStar transition
    var history = await DbContext.DraftersHonorificHistory
      .Where(h => h.DrafterIdValue == drafterId)
      .ToListAsync();

    history.Should().NotBeEmpty("a history record is written when the honorific changes");
  }

  // -------------------------------------------------------------------------
  // Helpers
  // -------------------------------------------------------------------------

  private async Task SendAppearancesAsync(Guid drafterId, int count, int canonicalPolicyValue)
  {
    for (var i = 0; i < count; i++)
    {
      var command = BuildCommand(drafterId, _faker.Random.AlphaNumeric(10), canonicalPolicyValue);
      await Sender.Send(command);
    }
  }

  private static UpdateDrafterHonorificsCommand BuildCommand(
    Guid drafterId,
    string draftPartPublicId,
    int canonicalPolicyValue = 0,
    bool hasMainFeedRelease = false)
  {
    return new UpdateDrafterHonorificsCommand
    {
      DrafterIdValue = drafterId,
      DraftPartPublicId = draftPartPublicId,
      CanonicalPolicyValue = canonicalPolicyValue,
      HasMainFeedRelease = hasMainFeedRelease
    };
  }
}
