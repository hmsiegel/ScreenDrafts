namespace ScreenDrafts.Modules.Drafts.UnitTests.VetoOverrides;

public class VetoOverrideTests : DraftsBaseTest
{
  [Fact]
  public void Create_ShouldReturnSuccessResult_WhenValidParametersAreProvided()
  {
    // Arrange
    var veto = VetoFactory.CreateVeto().Value;
    var drafter = CreateDrafter();
    var participantId = CreateParticipantId(drafter);
    var draftPartParticipant = CreateDraftPartParticipant(veto.DraftPart, drafter);

    // Act
    var result = VetoOverride.Create(veto, draftPartParticipant);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Veto.Should().Be(veto);
    result.Value.IssuedByParticipant.Should().Be(draftPartParticipant);
    result.Value.IssuedByParticipant.ParticipantId.Should().Be(participantId);
  }

  [Fact]
  public void Create_ShouldRaiseDomainEvent_WhenVetoOverrideIsCreated()
  {
    // Arrange
    var veto = VetoFactory.CreateVeto().Value;
    var drafter = CreateDrafter();
    var participantId = CreateParticipantId(drafter);
    var draftPartParticipant = CreateDraftPartParticipant(veto.DraftPart, drafter);

    // Act
    var vetoOverride = VetoOverride.Create(veto, draftPartParticipant);

    // Assert
    var domainEvent = AssertDomainEventWasPublished<VetoOverrideCreatedDomainEvent>(vetoOverride.Value);
    domainEvent.VetoOverrideId.Should().Be(vetoOverride.Value.Id.Value);
    domainEvent.VetoId.Should().Be(veto.Id.Value);
    domainEvent.IssuedBy.Should().Be(participantId);
  }

  [Fact]
  public void Create_ShouldThrowArgumentNullException_WhenVetoIsNull()
  {
    // Arrange
    Veto? veto = null;
    var draftPart = CreateDraftPart();
    var drafter = CreateDrafter();
    var draftPartParticipant = CreateDraftPartParticipant(draftPart, drafter);

    // Act
    Action act = () => VetoOverride.Create(veto!, draftPartParticipant);

    // Assert
    act.Should().Throw<ArgumentNullException>();
  }

  [Fact]
  public void Create_ShouldThrowArgumentNullException_WhenIssuedByParticipantIsNull()
  {
    // Arrange
    var veto = VetoFactory.CreateVeto().Value;
    DraftPartParticipant? draftPartParticipant = null;

    // Act
    Action act = () => VetoOverride.Create(veto, draftPartParticipant!);

    // Assert
    act.Should().Throw<ArgumentNullException>();
  }

  [Fact]
  public void Create_ShouldReturnFailure_WhenIssuedByHasNoValue()
  {
    // Arrange
    var veto = VetoFactory.CreateVeto().Value;
    var participantId = new ParticipantId(Guid.Empty, ParticipantKind.Drafter);
    var draftPartParticipant = DraftPartParticipant.Create(veto.DraftPart, participantId);

    // Act
    var result = VetoOverride.Create(veto, draftPartParticipant);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(VetoOverrideErrors.IssuedByMustBeProvided);
  }

  [Fact]
  public void VetoId_ShouldMatchVeto_WhenCreated()
  {
    // Arrange
    var veto = VetoFactory.CreateVeto().Value;
    var drafter = CreateDrafter();
    var draftPartParticipant = CreateDraftPartParticipant(veto.DraftPart, drafter);

    // Act
    var vetoOverride = VetoOverride.Create(veto, draftPartParticipant).Value;

    // Assert
    vetoOverride.VetoId.Should().Be(veto.Id);
  }
}

