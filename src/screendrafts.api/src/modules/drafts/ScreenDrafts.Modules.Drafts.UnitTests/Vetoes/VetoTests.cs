namespace ScreenDrafts.Modules.Drafts.UnitTests.Vetoes;

public class VetoTests : DraftsBaseTest
{
  [Fact]
  public void Create_ShouldReturnSuccessResult_WhenValidParametersAreProvided()
  {
    // Arrange
    var pick = PickFactory.CreatePick().Value;
    var drafter = CreateDrafter();
    var participantId = CreateParticipantId(drafter);
    var issuedByParticipant = CreateDraftPartParticipant(pick.DraftPart, drafter);

    // Act
    var result = Veto.Create(pick, issuedByParticipant);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.TargetPick.Should().Be(pick);
    result.Value.IssuedByParticipant.Should().Be(issuedByParticipant);
    result.Value.IssuedByParticipant.ParticipantId.Should().Be(participantId);
  }

  [Fact]
  public void Create_ShouldRaiseDomainEvent_WhenVetoIsCreated()
  {
    // Arrange
    var pick = PickFactory.CreatePick().Value;
    var drafter = CreateDrafter();
    var issuedByParticipant = CreateDraftPartParticipant(pick.DraftPart, drafter);

    // Act
    var veto = Veto.Create(pick, issuedByParticipant).Value;

    // Assert
    var domainEvent = AssertDomainEventWasPublished<VetoCreatedDomainEvent>(veto);
    domainEvent.VetoId.Should().Be(veto.Id.Value);
    domainEvent.PickId.Should().Be(pick.Id.Value);
  }

  [Fact]
  public void Create_ShouldReturnFailure_WhenPickIsNull()
  {
    // Arrange
    Pick? pick = null;
    var drafter = CreateDrafter();
    var draftPart = CreateDraftPart();
    var issuedByParticipant = CreateDraftPartParticipant(draftPart, drafter);

    // Act
    var result = Veto.Create(pick!, issuedByParticipant);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(VetoErrors.PickMustBeProvided);
  }

  [Fact]
  public void Create_ShouldThrowArgumentNullException_WhenIssuedByParticipantIsNull()
  {
    // Arrange
    var pick = PickFactory.CreatePick().Value;
    DraftPartParticipant? issuedByParticipant = null;

    // Act
    Action act = () => Veto.Create(pick, issuedByParticipant!);

    // Assert
    act.Should().Throw<ArgumentNullException>();
  }

  [Fact]
  public void Create_ShouldAcceptNote_WhenProvided()
  {
    // Arrange
    var pick = PickFactory.CreatePick().Value;
    var drafter = CreateDrafter();
    var issuedByParticipant = CreateDraftPartParticipant(pick.DraftPart, drafter);
    var note = "Test veto note";

    // Act
    var result = Veto.Create(pick, issuedByParticipant, note: note);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Note.Should().Be(note);
  }

  [Fact]
  public void Override_ShouldSucceed_WhenVetoIsNotAlreadyOverridden()
  {
    // Arrange
    var veto = VetoFactory.CreateVeto().Value;
    var drafter = CreateDrafter();
    var participantId = CreateParticipantId(drafter);

    // Act
    var result = veto.Override(participantId);

    // Assert
    result.IsSuccess.Should().BeTrue();
    veto.IsOverridden.Should().BeTrue();
    veto.VetoOverride.Should().NotBeNull();
    veto.VetoOverride!.IssuedByParticipant.ParticipantId.Should().Be(participantId);
  }

  [Fact]
  public void Override_ShouldFail_WhenVetoIsAlreadyOverridden()
  {
    // Arrange
    var veto = VetoFactory.CreateVeto().Value;
    var drafter = CreateDrafter();
    var participantId = CreateParticipantId(drafter);
    veto.Override(participantId);

    // Act
    var result = veto.Override(participantId);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(VetoErrors.VetoOverrideAlreadyUsed);
  }

  [Fact]
  public void DraftPart_ShouldReturnCorrectDraftPart_FromTargetPick()
  {
    // Arrange
    var veto = VetoFactory.CreateVeto().Value;

    // Act & Assert
    veto.DraftPart.Should().Be(veto.TargetPick.DraftPart);
    veto.DraftPartId.Should().Be(veto.TargetPick.DraftPartId);
  }

  [Fact]
  public void OccurredOn_ShouldBeSet_WhenVetoIsCreated()
  {
    // Arrange & Act
    var veto = VetoFactory.CreateVeto().Value;

    // Assert
    veto.OccurredOn.Should().NotBeNull();
    veto.OccurredOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
  }
}

