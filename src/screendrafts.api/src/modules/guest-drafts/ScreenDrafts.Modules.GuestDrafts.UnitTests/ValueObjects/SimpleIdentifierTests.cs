using ScreenDrafts.Modules.GuestDrafts.Domain.Drafts.ValueObjects;

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
    DraftParticipantId.CreateUnique().Should().NotBe(DraftParticipantId.CreateUnique());
  }

  [Fact]
  public void GuestDraftParticipantId_Create_ShouldSetValue()
  {
    var value = Guid.NewGuid();
    DraftParticipantId.Create(value).Value.Should().Be(value);
  }

  [Fact]
  public void GuestDraftParticipantId_FromString_ShouldRoundTrip()
  {
    var original = DraftParticipantId.CreateUnique();
    DraftParticipantId.FromString(original.Value.ToString()).Should().Be(original);
  }

  // ── GuestDraftPickId ─────────────────────────────────────────────────────

  [Fact]
  public void GuestDraftPickId_CreateUnique_ShouldGenerateDifferentValues()
  {
    PickId.CreateUnique().Should().NotBe(PickId.CreateUnique());
  }

  [Fact]
  public void GuestDraftPickId_Create_ShouldSetValue()
  {
    var value = Guid.NewGuid();
    PickId.Create(value).Value.Should().Be(value);
  }

  [Fact]
  public void GuestDraftPickId_FromString_ShouldRoundTrip()
  {
    var original = PickId.CreateUnique();
    PickId.FromString(original.Value.ToString()).Should().Be(original);
  }

  [Fact]
  public void GuestDraftPickId_Empty_ShouldHaveAnEmptyGuidValue()
  {
    PickId.Empty.Value.Should().Be(Guid.Empty);
  }

  // ── GuestDraftPositionId ─────────────────────────────────────────────────

  [Fact]
  public void GuestDraftPositionId_CreateUnique_ShouldGenerateDifferentValues()
  {
    DraftPositionId.CreateUnique().Should().NotBe(DraftPositionId.CreateUnique());
  }

  [Fact]
  public void GuestDraftPositionId_Create_ShouldSetValue()
  {
    var value = Guid.NewGuid();
    DraftPositionId.Create(value).Value.Should().Be(value);
  }

  [Fact]
  public void GuestDraftPositionId_FromString_ShouldRoundTrip()
  {
    var original = DraftPositionId.CreateUnique();
    DraftPositionId.FromString(original.Value.ToString()).Should().Be(original);
  }

  // ── GuestDraftVetoId ─────────────────────────────────────────────────────

  [Fact]
  public void GuestDraftVetoId_CreateUnique_ShouldGenerateDifferentValues()
  {
    VetoId.CreateUnique().Should().NotBe(VetoId.CreateUnique());
  }

  [Fact]
  public void GuestDraftVetoId_Create_ShouldSetValue()
  {
    var value = Guid.NewGuid();
    VetoId.Create(value).Value.Should().Be(value);
  }

  [Fact]
  public void GuestDraftVetoId_FromString_ShouldRoundTrip()
  {
    var original = VetoId.CreateUnique();
    VetoId.FromString(original.Value.ToString()).Should().Be(original);
  }

  // ── GuestDraftVetoOverrideId ─────────────────────────────────────────────

  [Fact]
  public void GuestDraftVetoOverrideId_CreateUnique_ShouldGenerateDifferentValues()
  {
    VetoOverrideId.CreateUnique().Should().NotBe(VetoOverrideId.CreateUnique());
  }

  [Fact]
  public void GuestDraftVetoOverrideId_Create_ShouldSetValue()
  {
    var value = Guid.NewGuid();
    VetoOverrideId.Create(value).Value.Should().Be(value);
  }

  [Fact]
  public void GuestDraftVetoOverrideId_FromString_ShouldRoundTrip()
  {
    var original = VetoOverrideId.CreateUnique();
    VetoOverrideId.FromString(original.Value.ToString()).Should().Be(original);
  }

  // ── GuestDraftCommissionerOverrideId ─────────────────────────────────────

  [Fact]
  public void GuestDraftCommissionerOverrideId_CreateUnique_ShouldGenerateDifferentValues()
  {
    CommissionerOverrideId.CreateUnique().Should().NotBe(CommissionerOverrideId.CreateUnique());
  }

  [Fact]
  public void GuestDraftCommissionerOverrideId_Create_ShouldSetValue()
  {
    var value = Guid.NewGuid();
    CommissionerOverrideId.Create(value).Value.Should().Be(value);
  }

  [Fact]
  public void GuestDraftCommissionerOverrideId_FromString_ShouldRoundTrip()
  {
    var original = CommissionerOverrideId.CreateUnique();
    CommissionerOverrideId.FromString(original.Value.ToString()).Should().Be(original);
  }

  // ── GuestDraftGameBoardId ────────────────────────────────────────────────

  [Fact]
  public void GuestDraftGameBoardId_CreateUnique_ShouldGenerateDifferentValues()
  {
    GameBoardId.CreateUnique().Should().NotBe(GameBoardId.CreateUnique());
  }

  [Fact]
  public void GuestDraftGameBoardId_Create_ShouldSetValue()
  {
    var value = Guid.NewGuid();
    GameBoardId.Create(value).Value.Should().Be(value);
  }

  [Fact]
  public void GuestDraftGameBoardId_FromString_ShouldRoundTrip()
  {
    var original = GameBoardId.CreateUnique();
    GameBoardId.FromString(original.Value.ToString()).Should().Be(original);
  }
}
