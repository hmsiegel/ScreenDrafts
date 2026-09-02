namespace ScreenDrafts.Modules.GuestDrafts.UnitTests.Entities;

public class GuestDraftVetoTests : GuestDraftsBaseTest
{
  [Fact]
  public void Create_ShouldReturnSuccessResult_WhenValidParametersAreProvided()
  {
    // Arrange
    var (guestDraft, owner, other) = CreateInProgressStandardGuestDraft();
    var pick = CreatePick(guestDraft, owner);

    // Act
    var result = GuestDraftVeto.Create(pick, other);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.TargetPick.Should().Be(pick);
    result.Value.TargetPickId.Should().Be(pick.Id);
    result.Value.IssuedByParticipant.Should().Be(other);
    result.Value.IssuedByParticipantId.Should().Be(other.Id);
  }

  [Fact]
  public void Create_ShouldThrowArgumentNullException_WhenPickIsNull()
  {
    // Arrange
    var (_, owner, _) = CreateInProgressStandardGuestDraft();
    GuestDraftPick? pick = null;

    // Act
    Action act = () => GuestDraftVeto.Create(pick!, owner);

    // Assert
    act.Should().Throw<ArgumentNullException>();
  }

  [Fact]
  public void Create_ShouldThrowArgumentNullException_WhenIssuedByParticipantIsNull()
  {
    // Arrange
    var (guestDraft, owner, _) = CreateInProgressStandardGuestDraft();
    var pick = CreatePick(guestDraft, owner);
    GuestDraftParticipant? issuedByParticipant = null;

    // Act
    Action act = () => GuestDraftVeto.Create(pick, issuedByParticipant!);

    // Assert
    act.Should().Throw<ArgumentNullException>();
  }

  [Fact]
  public void Create_ShouldAcceptNote_WhenProvided()
  {
    // Arrange
    var (guestDraft, owner, other) = CreateInProgressStandardGuestDraft();
    var pick = CreatePick(guestDraft, owner);
    var note = "Vetoed for pacing reasons";

    // Act
    var result = GuestDraftVeto.Create(pick, other, note: note);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Note.Should().Be(note);
  }

  [Fact]
  public void Create_ShouldSetSequenceToOne_ForTheFirstVetoOnAPick()
  {
    // Arrange
    var (guestDraft, owner, other) = CreateInProgressStandardGuestDraft();
    var pick = CreatePick(guestDraft, owner);

    // Act
    var veto = GuestDraftVeto.Create(pick, other).Value;

    // Assert
    veto.Sequence.Should().Be(1);
  }

  [Fact]
  public void Create_ShouldIncrementSequence_WhenThePickAlreadyHasAVeto()
  {
    // Arrange -- go through the aggregate once so the pick already carries a veto
    var (guestDraft, owner, other) = CreateInProgressStandardGuestDraft();
    var pickId = guestDraft.PlayPick(Faker.Random.AlphaNumeric(10), 7, 1, owner.Id.Value).Value;
    guestDraft.ApplyVeto(pickId, other.Id.Value);
    var pick = guestDraft.Picks.Single(p => p.Id == pickId);

    // Act
    var secondVeto = GuestDraftVeto.Create(pick, other);

    // Assert
    secondVeto.Value.Sequence.Should().Be(2);
  }

  [Fact]
  public void Create_ShouldSetOccurredOnToUtcNow_WhenCreated()
  {
    // Arrange & Act
    var veto = GuestDraftVetoFactory.CreateVeto().Value;

    // Assert
    veto.OccurredOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
  }

  [Fact]
  public void Override_ShouldSucceed_WhenVetoIsNotAlreadyOverridden()
  {
    // Arrange
    var veto = GuestDraftVetoFactory.CreateVeto().Value;
    var overrideIssuer = veto.TargetPick.PlayedByParticipant;

    // Act
    var result = veto.Override(overrideIssuer);

    // Assert
    result.IsSuccess.Should().BeTrue();
    veto.IsOverridden.Should().BeTrue();
    veto.VetoOverride.Should().NotBeNull();
    veto.VetoOverride.IssuedByParticipant.Should().Be(overrideIssuer);
  }

  [Fact]
  public void Override_ShouldReturnFailure_WhenVetoIsAlreadyOverridden()
  {
    // Arrange
    var veto = GuestDraftVetoFactory.CreateVeto().Value;
    var overrideIssuer = veto.TargetPick.PlayedByParticipant;
    veto.Override(overrideIssuer);

    // Act
    var result = veto.Override(overrideIssuer);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(GuestDraftErrors.VetoOverrideAlreadyUsed);
  }
}
