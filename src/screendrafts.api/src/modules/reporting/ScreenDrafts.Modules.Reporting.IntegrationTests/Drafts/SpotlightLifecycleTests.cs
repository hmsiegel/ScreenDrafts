namespace ScreenDrafts.Modules.Reporting.IntegrationTests.Drafts;

/// <summary>
/// ActivateSpotlightCommandHandler, DeactivateSpotlightCommandHandler, and
/// DeleteSpotlightCommandHandler had no coverage at all before this file. Together
/// they enforce the module's single-active-spotlight invariant: activating one
/// spotlight must deactivate/unpin whatever was previously active, in the same unit
/// of work -- that cross-aggregate rule is the main thing worth pinning down here,
/// not the individual CRUD-shaped branches.
/// </summary>
public sealed class SpotlightLifecycleTests(ReportingIntegrationTestWebAppFactory factory)
  : ReportingIntegrationTest(factory)
{
  private static readonly Faker _faker = new();

  private async Task<DraftSpotlight> SeedSpotlightAsync()
  {
    var spotlight = DraftSpotlight.Create(
      publicId: $"spl_{_faker.Random.AlphaNumeric(15)}",
      draftPublicId: $"d_{_faker.Random.AlphaNumeric(15)}",
      spotlightDescription: _faker.Lorem.Sentence(),
      spotifyUrl: null
    );
    DbContext.DraftSpotlights.Add(spotlight);
    await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
    return spotlight;
  }

  // ── Activate ──────────────────────────────────────────────────────────────

  [Fact]
  public async Task Activate_WhenNoOtherSpotlightIsActive_ShouldActivateAndPinAsync()
  {
    // Arrange
    var spotlight = await SeedSpotlightAsync();

    // Act
    var result = await Sender.Send(
      new ActivateSpotlightCommand { PublicId = spotlight.PublicId },
      TestContext.Current.CancellationToken
    );

    // Assert
    result.IsSuccess.Should().BeTrue();
    var reloaded = await DbContext.DraftSpotlights.SingleAsync(
      s => s.PublicId == spotlight.PublicId,
      TestContext.Current.CancellationToken
    );
    reloaded.IsActive.Should().BeTrue();
    reloaded.IsPinned.Should().BeTrue();
  }

  [Fact]
  public async Task Activate_WhenAnotherSpotlightIsAlreadyActive_ShouldDeactivateAndUnpinTheOldOneAsync()
  {
    // Arrange -- regression for the single-active-spotlight invariant
    var previouslyActive = await SeedSpotlightAsync();
    (
      await Sender.Send(
        new ActivateSpotlightCommand { PublicId = previouslyActive.PublicId },
        TestContext.Current.CancellationToken
      )
    )
      .IsSuccess.Should()
      .BeTrue();

    var toActivate = await SeedSpotlightAsync();

    // Act
    var result = await Sender.Send(
      new ActivateSpotlightCommand { PublicId = toActivate.PublicId },
      TestContext.Current.CancellationToken
    );

    // Assert
    result.IsSuccess.Should().BeTrue();
    var oldOne = await DbContext.DraftSpotlights.SingleAsync(
      s => s.PublicId == previouslyActive.PublicId,
      TestContext.Current.CancellationToken
    );
    oldOne.IsActive.Should().BeFalse();
    oldOne.IsPinned.Should().BeFalse();
    var newOne = await DbContext.DraftSpotlights.SingleAsync(
      s => s.PublicId == toActivate.PublicId,
      TestContext.Current.CancellationToken
    );
    newOne.IsActive.Should().BeTrue();
    newOne.IsPinned.Should().BeTrue();
  }

  [Fact]
  public async Task Activate_WhenAlreadyActive_ShouldBeANoOpAsync()
  {
    // Arrange
    var spotlight = await SeedSpotlightAsync();
    await Sender.Send(
      new ActivateSpotlightCommand { PublicId = spotlight.PublicId },
      TestContext.Current.CancellationToken
    );

    // Act
    var result = await Sender.Send(
      new ActivateSpotlightCommand { PublicId = spotlight.PublicId },
      TestContext.Current.CancellationToken
    );

    // Assert
    result.IsSuccess.Should().BeTrue();
  }

  [Fact]
  public async Task Activate_WhenSpotlightDoesNotExist_ShouldFailAsync()
  {
    // Arrange
    var publicId = $"spl_{_faker.Random.AlphaNumeric(15)}";

    // Act
    var result = await Sender.Send(
      new ActivateSpotlightCommand { PublicId = publicId },
      TestContext.Current.CancellationToken
    );

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().Contain(e => e.Code == DraftReportingErrors.SpotlightNotFound(publicId).Code);
  }

  // ── Deactivate ────────────────────────────────────────────────────────────

  [Fact]
  public async Task Deactivate_WhenActive_ShouldDeactivateAndUnpinAsync()
  {
    // Arrange
    var spotlight = await SeedSpotlightAsync();
    await Sender.Send(
      new ActivateSpotlightCommand { PublicId = spotlight.PublicId },
      TestContext.Current.CancellationToken
    );

    // Act
    var result = await Sender.Send(
      new DeactivateSpotlightCommand { PublicId = spotlight.PublicId },
      TestContext.Current.CancellationToken
    );

    // Assert
    result.IsSuccess.Should().BeTrue();
    var reloaded = await DbContext.DraftSpotlights.SingleAsync(
      s => s.PublicId == spotlight.PublicId,
      TestContext.Current.CancellationToken
    );
    reloaded.IsActive.Should().BeFalse();
    reloaded.IsPinned.Should().BeFalse();
  }

  [Fact]
  public async Task Deactivate_WhenAlreadyInactive_ShouldBeANoOpAsync()
  {
    // Arrange
    var spotlight = await SeedSpotlightAsync();

    // Act
    var result = await Sender.Send(
      new DeactivateSpotlightCommand { PublicId = spotlight.PublicId },
      TestContext.Current.CancellationToken
    );

    // Assert
    result.IsSuccess.Should().BeTrue();
  }

  [Fact]
  public async Task Deactivate_WhenSpotlightDoesNotExist_ShouldFailAsync()
  {
    // Arrange
    var publicId = $"spl_{_faker.Random.AlphaNumeric(15)}";

    // Act
    var result = await Sender.Send(
      new DeactivateSpotlightCommand { PublicId = publicId },
      TestContext.Current.CancellationToken
    );

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().Contain(e => e.Code == DraftReportingErrors.SpotlightNotFound(publicId).Code);
  }

  // ── Delete ────────────────────────────────────────────────────────────────

  [Fact]
  public async Task Delete_WhenInactive_ShouldRemoveTheSpotlightAsync()
  {
    // Arrange
    var spotlight = await SeedSpotlightAsync();

    // Act
    var result = await Sender.Send(
      new DeleteSpotlightCommand { PublicId = spotlight.PublicId },
      TestContext.Current.CancellationToken
    );

    // Assert
    result.IsSuccess.Should().BeTrue();
    var reloaded = await DbContext.DraftSpotlights.SingleOrDefaultAsync(
      s => s.PublicId == spotlight.PublicId,
      TestContext.Current.CancellationToken
    );
    reloaded.Should().BeNull();
  }

  [Fact]
  public async Task Delete_WhenActive_ShouldFailAsync()
  {
    // Arrange -- regression: an active spotlight must be deactivated first, since
    // deleting it out from under the single-active-spotlight invariant would leave
    // nothing active with no record of how that happened.
    var spotlight = await SeedSpotlightAsync();
    await Sender.Send(
      new ActivateSpotlightCommand { PublicId = spotlight.PublicId },
      TestContext.Current.CancellationToken
    );

    // Act
    var result = await Sender.Send(
      new DeleteSpotlightCommand { PublicId = spotlight.PublicId },
      TestContext.Current.CancellationToken
    );

    // Assert
    result.IsFailure.Should().BeTrue();
    result
      .Errors.Should()
      .Contain(e => e.Code == DraftReportingErrors.CannotDeleteActiveSpotlight.Code);
    var reloaded = await DbContext.DraftSpotlights.SingleOrDefaultAsync(
      s => s.PublicId == spotlight.PublicId,
      TestContext.Current.CancellationToken
    );
    reloaded.Should().NotBeNull();
  }

  [Fact]
  public async Task Delete_WhenSpotlightDoesNotExist_ShouldFailAsync()
  {
    // Arrange
    var publicId = $"spl_{_faker.Random.AlphaNumeric(15)}";

    // Act
    var result = await Sender.Send(
      new DeleteSpotlightCommand { PublicId = publicId },
      TestContext.Current.CancellationToken
    );

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().Contain(e => e.Code == DraftReportingErrors.SpotlightNotFound(publicId).Code);
  }
}
