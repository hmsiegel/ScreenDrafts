namespace ScreenDrafts.Modules.GuestDrafts.UnitTests.ValueObjects;

/// <summary>
/// GuestDraftParticipantId, GuestDraftPickId, GuestDraftPositionId, GuestDraftVetoId,
/// GuestDraftVetoOverrideId, GuestDraftCommissionerOverrideId, and GuestDraftGameBoardId
/// are all plain records with the same shape (CreateUnique / Create / FromString), so
/// they're covered together here rather than in seven near-identical files.
/// </summary>
public class SimpleIdentifierTests
{
  // ── GuestDraftParticipantId ──────────────────────────────────────────────

  [Fact]
  public void GuestDraftParticipantId_CreateUnique_ShouldGenerateDifferentValues()
  {
    GuestDraftParticipantId.CreateUnique().Should().NotBe(GuestDraftParticipantId.CreateUnique());
  }

  [Fact]
  public void GuestDraftParticipantId_Create_ShouldSetValue()
  {
    var value = Guid.NewGuid();
    GuestDraftParticipantId.Create(value).Value.Should().Be(value);
  }

  [Fact]
  public void GuestDraftParticipantId_FromString_ShouldRoundTrip()
  {
    var original = GuestDraftParticipantId.CreateUnique();
    GuestDraftParticipantId.FromString(original.Value.ToString()).Should().Be(original);
  }

  // ── GuestDraftPickId ─────────────────────────────────────────────────────

  [Fact]
  public void GuestDraftPickId_CreateUnique_ShouldGenerateDifferentValues()
  {
    GuestDraftPickId.CreateUnique().Should().NotBe(GuestDraftPickId.CreateUnique());
  }

  [Fact]
  public void GuestDraftPickId_Create_ShouldSetValue()
  {
    var value = Guid.NewGuid();
    GuestDraftPickId.Create(value).Value.Should().Be(value);
  }

  [Fact]
  public void GuestDraftPickId_FromString_ShouldRoundTrip()
  {
    var original = GuestDraftPickId.CreateUnique();
    GuestDraftPickId.FromString(original.Value.ToString()).Should().Be(original);
  }

  [Fact]
  public void GuestDraftPickId_Empty_ShouldHaveAnEmptyGuidValue()
  {
    GuestDraftPickId.Empty.Value.Should().Be(Guid.Empty);
  }

  // ── GuestDraftPositionId ─────────────────────────────────────────────────

  [Fact]
  public void GuestDraftPositionId_CreateUnique_ShouldGenerateDifferentValues()
  {
    GuestDraftPositionId.CreateUnique().Should().NotBe(GuestDraftPositionId.CreateUnique());
  }

  [Fact]
  public void GuestDraftPositionId_Create_ShouldSetValue()
  {
    var value = Guid.NewGuid();
    GuestDraftPositionId.Create(value).Value.Should().Be(value);
  }

  [Fact]
  public void GuestDraftPositionId_FromString_ShouldRoundTrip()
  {
    var original = GuestDraftPositionId.CreateUnique();
    GuestDraftPositionId.FromString(original.Value.ToString()).Should().Be(original);
  }

  // ── GuestDraftVetoId ─────────────────────────────────────────────────────

  [Fact]
  public void GuestDraftVetoId_CreateUnique_ShouldGenerateDifferentValues()
  {
    GuestDraftVetoId.CreateUnique().Should().NotBe(GuestDraftVetoId.CreateUnique());
  }

  [Fact]
  public void GuestDraftVetoId_Create_ShouldSetValue()
  {
    var value = Guid.NewGuid();
    GuestDraftVetoId.Create(value).Value.Should().Be(value);
  }

  [Fact]
  public void GuestDraftVetoId_FromString_ShouldRoundTrip()
  {
    var original = GuestDraftVetoId.CreateUnique();
    GuestDraftVetoId.FromString(original.Value.ToString()).Should().Be(original);
  }

  // ── GuestDraftVetoOverrideId ─────────────────────────────────────────────

  [Fact]
  public void GuestDraftVetoOverrideId_CreateUnique_ShouldGenerateDifferentValues()
  {
    GuestDraftVetoOverrideId.CreateUnique().Should().NotBe(GuestDraftVetoOverrideId.CreateUnique());
  }

  [Fact]
  public void GuestDraftVetoOverrideId_Create_ShouldSetValue()
  {
    var value = Guid.NewGuid();
    GuestDraftVetoOverrideId.Create(value).Value.Should().Be(value);
  }

  [Fact]
  public void GuestDraftVetoOverrideId_FromString_ShouldRoundTrip()
  {
    var original = GuestDraftVetoOverrideId.CreateUnique();
    GuestDraftVetoOverrideId.FromString(original.Value.ToString()).Should().Be(original);
  }

  // ── GuestDraftCommissionerOverrideId ─────────────────────────────────────

  [Fact]
  public void GuestDraftCommissionerOverrideId_CreateUnique_ShouldGenerateDifferentValues()
  {
    GuestDraftCommissionerOverrideId.CreateUnique().Should().NotBe(GuestDraftCommissionerOverrideId.CreateUnique());
  }

  [Fact]
  public void GuestDraftCommissionerOverrideId_Create_ShouldSetValue()
  {
    var value = Guid.NewGuid();
    GuestDraftCommissionerOverrideId.Create(value).Value.Should().Be(value);
  }

  [Fact]
  public void GuestDraftCommissionerOverrideId_FromString_ShouldRoundTrip()
  {
    var original = GuestDraftCommissionerOverrideId.CreateUnique();
    GuestDraftCommissionerOverrideId.FromString(original.Value.ToString()).Should().Be(original);
  }

  // ── GuestDraftGameBoardId ────────────────────────────────────────────────

  [Fact]
  public void GuestDraftGameBoardId_CreateUnique_ShouldGenerateDifferentValues()
  {
    GuestDraftGameBoardId.CreateUnique().Should().NotBe(GuestDraftGameBoardId.CreateUnique());
  }

  [Fact]
  public void GuestDraftGameBoardId_Create_ShouldSetValue()
  {
    var value = Guid.NewGuid();
    GuestDraftGameBoardId.Create(value).Value.Should().Be(value);
  }

  [Fact]
  public void GuestDraftGameBoardId_FromString_ShouldRoundTrip()
  {
    var original = GuestDraftGameBoardId.CreateUnique();
    GuestDraftGameBoardId.FromString(original.Value.ToString()).Should().Be(original);
  }
}
