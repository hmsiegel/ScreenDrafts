namespace ScreenDrafts.Modules.Reporting.UnitTests.Drafts;

public sealed class DraftSpotlightTests
{
  private static DraftSpotlight BuildSpotlight(
    string spotlightPublicId = "s_abc",
    string draftPublicId = "d_abc",
    string description = "A great draft",
    Uri? spotifyUrl = null
  ) => DraftSpotlight.Create(spotlightPublicId, draftPublicId, description, spotifyUrl);

  // -------------------------------------------------------------------------
  // Create — initial state
  // -------------------------------------------------------------------------

  [Fact]
  public void Create_ShouldSetDraftPublicId()
  {
    var spotlight = BuildSpotlight(draftPublicId: "d_xyz");

    spotlight.DraftPublicId.Should().Be("d_xyz");
  }

  [Fact]
  public void Create_ShouldSetSpotlightDescription()
  {
    var spotlight = BuildSpotlight(description: "Absolute banger episode");

    spotlight.SpotlightDescription.Should().Be("Absolute banger episode");
  }

  [Fact]
  public void Create_ShouldSetSpotifyUrl()
  {
    var uri = new Uri("https://open.spotify.com/episode/abc123");
    var spotlight = BuildSpotlight(spotifyUrl: uri);

    spotlight.SpotifyUrl.Should().Be(uri);
  }

  [Fact]
  public void Create_ShouldAllowNullSpotifyUrl()
  {
    var spotlight = BuildSpotlight(spotifyUrl: null);

    spotlight.SpotifyUrl.Should().BeNull();
  }

  [Fact]
  public void Create_ShouldStartInactive()
  {
    var spotlight = BuildSpotlight();

    spotlight.IsActive.Should().BeFalse();
  }

  [Fact]
  public void Create_ShouldStartUnpinned()
  {
    var spotlight = BuildSpotlight();

    spotlight.IsPinned.Should().BeFalse();
  }

  [Fact]
  public void Create_ShouldHaveNullActivatedAtUtc()
  {
    var spotlight = BuildSpotlight();

    spotlight.ActivatedAtUtc.Should().BeNull();
  }

  // -------------------------------------------------------------------------
  // Activate
  // -------------------------------------------------------------------------

  [Fact]
  public void Activate_ShouldSetIsActiveToTrue()
  {
    var spotlight = BuildSpotlight();

    spotlight.Activate();

    spotlight.IsActive.Should().BeTrue();
  }

  [Fact]
  public void Activate_ShouldSetActivatedAtUtc()
  {
    var before = DateTime.UtcNow;
    var spotlight = BuildSpotlight();

    spotlight.Activate();

    spotlight.ActivatedAtUtc.Should().NotBeNull();
    spotlight.ActivatedAtUtc!.Value.Should().BeOnOrAfter(before);
  }

  // -------------------------------------------------------------------------
  // Deactivate
  // -------------------------------------------------------------------------

  [Fact]
  public void Deactivate_ShouldSetIsActiveToFalse()
  {
    var spotlight = BuildSpotlight();
    spotlight.Activate();

    spotlight.Deactivate();

    spotlight.IsActive.Should().BeFalse();
  }

  // -------------------------------------------------------------------------
  // Pin / Unpin
  // -------------------------------------------------------------------------

  [Fact]
  public void Pin_ShouldSetIsPinnedToTrue()
  {
    var spotlight = BuildSpotlight();

    spotlight.Pin();

    spotlight.IsPinned.Should().BeTrue();
  }

  [Fact]
  public void Unpin_ShouldSetIsPinnedToFalse()
  {
    var spotlight = BuildSpotlight();
    spotlight.Pin();

    spotlight.Unpin();

    spotlight.IsPinned.Should().BeFalse();
  }

  // -------------------------------------------------------------------------
  // UpdateDescription
  // -------------------------------------------------------------------------

  [Fact]
  public void UpdateDescription_ShouldReplaceDescription()
  {
    var spotlight = BuildSpotlight(description: "Old description");

    spotlight.UpdateDescription("New description");

    spotlight.SpotlightDescription.Should().Be("New description");
  }

  // -------------------------------------------------------------------------
  // UpdateSpotifyUrl
  // -------------------------------------------------------------------------

  [Fact]
  public void UpdateSpotifyUrl_ShouldReplaceUrl()
  {
    var spotlight = BuildSpotlight(spotifyUrl: new Uri("https://open.spotify.com/episode/old"));
    var newUrl = new Uri("https://open.spotify.com/episode/new");

    spotlight.UpdateSpotifyUrl(newUrl);

    spotlight.SpotifyUrl.Should().Be(newUrl);
  }

  [Fact]
  public void UpdateSpotifyUrl_ShouldAllowSettingNull()
  {
    var spotlight = BuildSpotlight(spotifyUrl: new Uri("https://open.spotify.com/episode/abc"));

    spotlight.UpdateSpotifyUrl(null);

    spotlight.SpotifyUrl.Should().BeNull();
  }
}
