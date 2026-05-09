namespace ScreenDrafts.Modules.Reporting.UnitTests.Drafts;

public sealed class DraftSummaryTests
{
  private static DraftSummary BuildSummary(
    Guid? draftId = null,
    string? draftPublicId = null,
    string? draftPartPublicId = null,
    string? title = null,
    int totalParts = 1,
    int totalPicks = 5,
    bool isPatreon = false,
    int? episodeNumber = null
  ) =>
    DraftSummary.Create(
      draftId: draftId ?? Guid.NewGuid(),
      draftPublicId: draftPublicId ?? "d_abc123",
      draftPartPublicId: draftPartPublicId ?? "dp_abc123",
      title: title ?? "Test Draft",
      draftType: "Standard",
      partIndex: 1,
      totalParts: totalParts,
      totalPicks: totalPicks,
      isPatreon: isPatreon,
      episodeNumber: episodeNumber,
      isComplete: false,
      completedAtUtc: null,
      createdAtUtc: DateTime.UtcNow
    );

  // -------------------------------------------------------------------------
  // Create — initial state
  // -------------------------------------------------------------------------

  [Fact]
  public void Create_ShouldSetAllProperties_WhenCreated()
  {
    var draftId = Guid.NewGuid();
    const string publicId = "d_abc";
    const string partPublicId = "dp_abc";
    const string title = "My Draft";

    var summary = DraftSummary.Create(
      draftId: draftId,
      draftPublicId: publicId,
      draftPartPublicId: partPublicId,
      title: title,
      draftType: "Standard",
      partIndex: 2,
      totalParts: 3,
      totalPicks: 10,
      isPatreon: true,
      episodeNumber: 42,
      isComplete: false,
      completedAtUtc: null,
      createdAtUtc: DateTime.UtcNow
    );

    summary.DraftId.Should().Be(draftId);
    summary.DraftPublicId.Should().Be(publicId);
    summary.DraftPartPublicId.Should().Be(partPublicId);
    summary.Title.Should().Be(title);
    summary.DraftType.Should().Be("Standard");
    summary.PartIndex.Should().Be(2);
    summary.TotalParts.Should().Be(3);
    summary.TotalPicks.Should().Be(10);
    summary.IsPatreon.Should().BeTrue();
    summary.EpisodeNumber.Should().Be(42);
    summary.IsComplete.Should().BeFalse();
    summary.CompletedAtUtc.Should().BeNull();
  }

  [Fact]
  public void Create_ShouldHaveNullEpisodeNumber_WhenNotProvided()
  {
    var summary = BuildSummary(episodeNumber: null);

    summary.EpisodeNumber.Should().BeNull();
  }

  // -------------------------------------------------------------------------
  // Update
  // -------------------------------------------------------------------------

  [Fact]
  public void Update_ShouldChangeTotalPartsAndPicks()
  {
    var summary = BuildSummary(totalParts: 1, totalPicks: 5);

    summary.Update(totalParts: 3, totalPicks: 21, episodeNumber: null, isPatreon: false);

    summary.TotalParts.Should().Be(3);
    summary.TotalPicks.Should().Be(21);
  }

  [Fact]
  public void Update_ShouldSetEpisodeNumber_WhenValueProvided()
  {
    var summary = BuildSummary(episodeNumber: null);

    summary.Update(totalParts: 1, totalPicks: 5, episodeNumber: 7, isPatreon: false);

    summary.EpisodeNumber.Should().Be(7);
  }

  [Fact]
  public void Update_ShouldNotOverwriteEpisodeNumber_WhenNullPassed()
  {
    var summary = BuildSummary(episodeNumber: 5);

    summary.Update(totalParts: 1, totalPicks: 5, episodeNumber: null, isPatreon: false);

    summary.EpisodeNumber.Should().Be(5);
  }

  [Fact]
  public void Update_ShouldChangeIsPatreon()
  {
    var summary = BuildSummary(isPatreon: false);

    summary.Update(totalParts: 1, totalPicks: 5, episodeNumber: null, isPatreon: true);

    summary.IsPatreon.Should().BeTrue();
  }

  // -------------------------------------------------------------------------
  // MarkComplete
  // -------------------------------------------------------------------------

  [Fact]
  public void MarkComplete_ShouldSetIsCompleteToTrue()
  {
    var summary = BuildSummary();

    summary.MarkComplete();

    summary.IsComplete.Should().BeTrue();
  }

  [Fact]
  public void MarkComplete_ShouldSetCompletedAtUtc()
  {
    var before = DateTime.UtcNow;
    var summary = BuildSummary();

    summary.MarkComplete();

    summary.CompletedAtUtc.Should().NotBeNull();
    summary.CompletedAtUtc!.Value.Should().BeOnOrAfter(before);
  }

  // -------------------------------------------------------------------------
  // SetEpisodeNumber
  // -------------------------------------------------------------------------

  [Fact]
  public void SetEpisodeNumber_ShouldUpdateEpisodeNumber()
  {
    var summary = BuildSummary(episodeNumber: null);

    summary.SetEpisodeNumber(99);

    summary.EpisodeNumber.Should().Be(99);
  }

  [Fact]
  public void SetEpisodeNumber_ShouldOverwriteExistingEpisodeNumber()
  {
    var summary = BuildSummary(episodeNumber: 10);

    summary.SetEpisodeNumber(20);

    summary.EpisodeNumber.Should().Be(20);
  }
}
