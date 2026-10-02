namespace ScreenDrafts.Modules.Reporting.UnitTests.Drafts;

[System.Diagnostics.CodeAnalysis.SuppressMessage(
  "Design",
  "CA1054:URI parameters should not be strings",
  Justification = "UpdateSpotlightCommand.SpotifyUrl is a string by contract; the handler is what parses it."
)]
public sealed class UpdateSpotlightCommandHandlerTests
{
  private const string PublicId = "spl_abcdefgh12345678";

  private readonly FakeDraftReportingRepository _repository = new();
  private readonly RecordingCacheService _cache = new();

  private UpdateSpotlightCommandHandler CreateHandler() => new(_repository, _cache);

  private static DraftSpotlight BuildSpotlight(
    string description = "Old description",
    Uri? spotifyUrl = null,
    bool active = false,
    bool pinned = false
  )
  {
    var spotlight = DraftSpotlight.Create(PublicId, "d_abcdefgh12345678", description, spotifyUrl);

    if (active)
    {
      spotlight.Activate();
    }

    if (pinned)
    {
      spotlight.Pin();
    }

    return spotlight;
  }

  private static UpdateSpotlightCommand BuildCommand(
    string description = "New description",
    string? spotifyUrl = null,
    string publicId = PublicId
  ) =>
    new()
    {
      PublicId = publicId,
      SpotlightDescription = description,
      SpotifyUrl = spotifyUrl,
    };

  // -------------------------------------------------------------------------
  // Not found
  // -------------------------------------------------------------------------

  [Fact]
  public async Task Handle_ShouldReturnSpotlightNotFound_WhenPublicIdIsUnknownAsync()
  {
    var result = await CreateHandler()
      .Handle(BuildCommand(publicId: "spl_doesnotexist0000"), TestContext.Current.CancellationToken);

    result.IsFailure.Should().BeTrue();
    result
      .Errors.Should()
      .ContainSingle()
      .Which.Should()
      .Be(DraftReportingErrors.SpotlightNotFound("spl_doesnotexist0000"));
  }

  [Fact]
  public async Task Handle_ShouldNotTouchTheCache_WhenPublicIdIsUnknownAsync()
  {
    await CreateHandler()
      .Handle(BuildCommand(publicId: "spl_doesnotexist0000"), TestContext.Current.CancellationToken);

    _cache.Calls.Should().BeEmpty();
  }

  // -------------------------------------------------------------------------
  // Field updates
  // -------------------------------------------------------------------------

  [Fact]
  public async Task Handle_ShouldUpdateDescriptionAndSpotifyUrlAsync()
  {
    var spotlight = BuildSpotlight(spotifyUrl: new Uri("https://open.spotify.com/episode/old"));
    _repository.Seed(spotlight);

    var result = await CreateHandler()
      .Handle(
        BuildCommand("Fresh copy", "https://open.spotify.com/episode/new"),
        TestContext.Current.CancellationToken
      );

    result.IsSuccess.Should().BeTrue();
    spotlight.SpotlightDescription.Should().Be("Fresh copy");
    spotlight.SpotifyUrl.Should().Be(new Uri("https://open.spotify.com/episode/new"));
  }

  [Theory]
  [InlineData(null)]
  [InlineData("")]
  [InlineData("   ")]
  public async Task Handle_ShouldClearTheSpotifyUrl_WhenUrlIsNullOrWhitespaceAsync(string? url)
  {
    var spotlight = BuildSpotlight(spotifyUrl: new Uri("https://open.spotify.com/episode/old"));
    _repository.Seed(spotlight);

    var result = await CreateHandler()
      .Handle(BuildCommand(spotifyUrl: url), TestContext.Current.CancellationToken);

    result.IsSuccess.Should().BeTrue();
    spotlight.SpotifyUrl.Should().BeNull();
  }

  [Fact]
  public async Task Handle_ShouldStoreTheUrlAsAParsedUri_WhenUrlIsNotEmptyAsync()
  {
    var spotlight = BuildSpotlight();
    _repository.Seed(spotlight);

    await CreateHandler()
      .Handle(
        BuildCommand(spotifyUrl: "https://open.spotify.com/episode/abc?si=1"),
        TestContext.Current.CancellationToken
      );

    spotlight.SpotifyUrl.Should().BeOfType<Uri>();
    spotlight.SpotifyUrl.AbsoluteUri.Should().Be("https://open.spotify.com/episode/abc?si=1");
  }

  // -------------------------------------------------------------------------
  // Cache
  // -------------------------------------------------------------------------

  [Fact]
  public async Task Handle_ShouldRemoveTheSpotlightCacheKeyExactlyOnce_WhenSpotlightIsActiveAsync()
  {
    _repository.Seed(BuildSpotlight(active: true, pinned: true));

    var result = await CreateHandler()
      .Handle(BuildCommand(), TestContext.Current.CancellationToken);

    result.IsSuccess.Should().BeTrue();
    _cache.RemovedKeys.Should().ContainSingle().Which.Should().Be(ReportingCacheKeys.SpotlightCacheKey);
    _cache.Calls.Should().ContainSingle();
  }

  [Fact]
  public async Task Handle_ShouldLeaveTheCacheAlone_WhenSpotlightIsInactiveAsync()
  {
    _repository.Seed(BuildSpotlight(active: false));

    var result = await CreateHandler()
      .Handle(BuildCommand(), TestContext.Current.CancellationToken);

    result.IsSuccess.Should().BeTrue();
    _cache.Calls.Should().BeEmpty();
  }

  // -------------------------------------------------------------------------
  // Invariants
  // -------------------------------------------------------------------------

  [Theory]
  [InlineData(true, true)]
  [InlineData(true, false)]
  [InlineData(false, true)]
  [InlineData(false, false)]
  public async Task Handle_ShouldNeverChangeActivationStateAsync(bool active, bool pinned)
  {
    var spotlight = BuildSpotlight(active: active, pinned: pinned);
    _repository.Seed(spotlight);
    var activatedAtBefore = spotlight.ActivatedAtUtc;

    await CreateHandler()
      .Handle(
        BuildCommand("Changed", "https://open.spotify.com/episode/x"),
        TestContext.Current.CancellationToken
      );

    spotlight.IsActive.Should().Be(active);
    spotlight.IsPinned.Should().Be(pinned);
    spotlight.ActivatedAtUtc.Should().Be(activatedAtBefore);
  }
}
