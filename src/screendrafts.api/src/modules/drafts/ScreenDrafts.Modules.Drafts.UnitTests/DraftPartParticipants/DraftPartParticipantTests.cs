namespace ScreenDrafts.Modules.Drafts.UnitTests.DraftPartParticipants;

public class DraftPartParticipantTests : DraftsBaseTest
{
  [Fact]
  public void Create_ShouldReturnDraftPartParticipant_WhenValidParametersProvided()
  {
    // Arrange
    var draftPart = CreateDraftPart();
    var drafter = CreateDrafter();
    var participantId = CreateParticipantId(drafter);

    // Act
    var participant = DraftPartParticipant.Create(draftPart, participantId);

    // Assert
    participant.Should().NotBeNull();
    participant.DraftPart.Should().Be(draftPart);
    participant.DraftPartId.Should().Be(draftPart.Id);
    participant.ParticipantId.Should().Be(participantId);
    participant.ParticipantIdValue.Should().Be(participantId.Value);
    participant.ParticipantKindValue.Should().Be(participantId.Kind);
  }

  [Fact]
  public void Create_ShouldThrowArgumentNullException_WhenDraftPartIsNull()
  {
    // Arrange
    var drafter = CreateDrafter();
    var participantId = CreateParticipantId(drafter);

    // Act
    Action act = () => DraftPartParticipant.Create(null!, participantId);

    // Assert
    act.Should().Throw<ArgumentNullException>();
  }

  [Fact]
  public void ParticipantId_ShouldReconstructFromStoredValues()
  {
    // Arrange
    var draftPart = CreateDraftPart();
    var drafter = CreateDrafter();
    var participantId = CreateParticipantId(drafter);

    // Act
    var participant = DraftPartParticipant.Create(draftPart, participantId);

    // Assert
    participant.ParticipantId.Value.Should().Be(participant.ParticipantIdValue);
    participant.ParticipantId.Kind.Should().Be(participant.ParticipantKindValue);
  }

  [Fact]
  public void StartingVetoes_ShouldDefaultToOne()
  {
    // Arrange
    var draftPart = CreateDraftPart();
    var drafter = CreateDrafter();
    var participantId = CreateParticipantId(drafter);

    // Act
    var participant = DraftPartParticipant.Create(draftPart, participantId);

    // Assert
    participant.StartingVetoes.Should().Be(1);
    participant.TotalVetoes.Should().Be(1);
  }

  [Fact]
  public void AddRollover_ShouldIncrementVetoCount_WhenIsVetoTrue()
  {
    // Arrange
    var draftPart = CreateDraftPart();
    var drafter = CreateDrafter();
    var participantId = CreateParticipantId(drafter);
    var participant = DraftPartParticipant.Create(draftPart, participantId);

    // Act
    participant.AddRollover(isVeto: true);

    // Assert
    participant.RolloverVeto.Should().Be(1);
    participant.TotalVetoes.Should().Be(2);
  }

  [Fact]
  public void AddRollover_ShouldIncrementOverrideCount_WhenIsVetoFalse()
  {
    // Arrange
    var draftPart = CreateDraftPart();
    var drafter = CreateDrafter();
    var participantId = CreateParticipantId(drafter);
    var participant = DraftPartParticipant.Create(draftPart, participantId);

    // Act
    participant.AddRollover(isVeto: false);

    // Assert
    participant.RolloverVetoOverride.Should().Be(1);
    participant.TotalVetoOverrides.Should().Be(1);
  }

  [Fact]
  public void AddTriviaAward_ShouldIncrementVetoCount_WhenIsVetoTrue()
  {
    // Arrange
    var draftPart = CreateDraftPart();
    var drafter = CreateDrafter();
    var participantId = CreateParticipantId(drafter);
    var participant = DraftPartParticipant.Create(draftPart, participantId);

    // Act
    participant.AddTriviaAward(isVeto: true);

    // Assert
    participant.TriviaVetoes.Should().Be(1);
    participant.TotalVetoes.Should().Be(2);
  }

  [Fact]
  public void AddTriviaAward_ShouldIncrementOverrideCount_WhenIsVetoFalse()
  {
    // Arrange
    var draftPart = CreateDraftPart();
    var drafter = CreateDrafter();
    var participantId = CreateParticipantId(drafter);
    var participant = DraftPartParticipant.Create(draftPart, participantId);

    // Act
    participant.AddTriviaAward(isVeto: false);

    // Assert
    participant.TriviaVetoOverrides.Should().Be(1);
    participant.TotalVetoOverrides.Should().Be(1);
  }

  [Fact]
  public void AddCommissionerOverride_ShouldIncrementCount()
  {
    // Arrange
    var draftPart = CreateDraftPart();
    var drafter = CreateDrafter();
    var participantId = CreateParticipantId(drafter);
    var participant = DraftPartParticipant.Create(draftPart, participantId);

    // Act
    participant.AddCommissionerOverride();

    // Assert
    participant.CommissionerOverrides.Should().Be(1);
  }

  [Fact]
  public void CanUseVeto_ShouldReturnTrue_WhenVetoesRemaining()
  {
    // Arrange
    var draftPart = CreateDraftPart();
    var drafter = CreateDrafter();
    var participantId = CreateParticipantId(drafter);
    var participant = DraftPartParticipant.Create(draftPart, participantId);

    // Act
    var canUse = participant.CanUseVeto();

    // Assert
    canUse.Should().BeTrue();
  }

  [Fact]
  public void CanUseVeto_ShouldReturnFalse_WhenNoVetoesRemaining()
  {
    // Arrange
    var draftPart = CreateDraftPart();
    var drafter = CreateDrafter();
    var participantId = CreateParticipantId(drafter);
    var participant = DraftPartParticipant.Create(draftPart, participantId);
    participant.SpendVeto();

    // Act
    var canUse = participant.CanUseVeto();

    // Assert
    canUse.Should().BeFalse();
  }

  [Fact]
  public void SpendVeto_ShouldIncrementVetoesUsed()
  {
    // Arrange
    var draftPart = CreateDraftPart();
    var drafter = CreateDrafter();
    var participantId = CreateParticipantId(drafter);
    var participant = DraftPartParticipant.Create(draftPart, participantId);

    // Act
    participant.SpendVeto();

    // Assert
    participant.VetoesUsed.Should().Be(1);
  }

  [Fact]
  public void SpendVeto_ShouldThrowException_WhenNoVetoesRemaining()
  {
    // Arrange
    var draftPart = CreateDraftPart();
    var drafter = CreateDrafter();
    var participantId = CreateParticipantId(drafter);
    var participant = DraftPartParticipant.Create(draftPart, participantId);
    participant.SpendVeto();

    // Act
    Action act = () => participant.SpendVeto();

    // Assert
    act.Should().Throw<InvalidOperationException>()
      .WithMessage("No remaining vetoes.");
  }

  [Fact]
  public void CanUseVetoOverride_ShouldReturnTrue_WhenOverridesRemaining()
  {
    // Arrange
    var draftPart = CreateDraftPart();
    var drafter = CreateDrafter();
    var participantId = CreateParticipantId(drafter);
    var participant = DraftPartParticipant.Create(draftPart, participantId);
    participant.AddRollover(isVeto: false);

    // Act
    var canUse = participant.CanUseVetoOverride(maxOverrides: 2);

    // Assert
    canUse.Should().BeTrue();
  }

  [Fact]
  public void CanUseVetoOverride_ShouldReturnFalse_WhenMaxOverridesIsZero()
  {
    // Arrange
    var draftPart = CreateDraftPart();
    var drafter = CreateDrafter();
    var participantId = CreateParticipantId(drafter);
    var participant = DraftPartParticipant.Create(draftPart, participantId);
    participant.AddRollover(isVeto: false);

    // Act
    var canUse = participant.CanUseVetoOverride(maxOverrides: 0);

    // Assert
    canUse.Should().BeFalse();
  }

  [Fact]
  public void SpendVetoOverride_ShouldIncrementOverridesUsed()
  {
    // Arrange
    var draftPart = CreateDraftPart();
    var drafter = CreateDrafter();
    var participantId = CreateParticipantId(drafter);
    var participant = DraftPartParticipant.Create(draftPart, participantId);
    participant.AddRollover(isVeto: false);

    // Act
    participant.SpendVetoOverride(maxOverrides: 2);

    // Assert
    participant.VetoOverridesUsed.Should().Be(1);
  }

  [Fact]
  public void SpendVetoOverride_ShouldThrowException_WhenNoOverridesRemaining()
  {
    // Arrange
    var draftPart = CreateDraftPart();
    var drafter = CreateDrafter();
    var participantId = CreateParticipantId(drafter);
    var participant = DraftPartParticipant.Create(draftPart, participantId);

    // Act
    Action act = () => participant.SpendVetoOverride(maxOverrides: 2);

    // Assert
    act.Should().Throw<InvalidOperationException>()
      .WithMessage("No remaining veto overrides.");
  }

  [Fact]
  public void VetoesRollingOver_ShouldReturnOne_WhenVetoesRemaining()
  {
    // Arrange
    var draftPart = CreateDraftPart();
    var drafter = CreateDrafter();
    var participantId = CreateParticipantId(drafter);
    var participant = DraftPartParticipant.Create(draftPart, participantId);

    // Act & Assert
    participant.VetoesRollingOver.Should().Be(1);
  }

  [Fact]
  public void VetoesRollingOver_ShouldReturnZero_WhenNoVetoesRemaining()
  {
    // Arrange
    var draftPart = CreateDraftPart();
    var drafter = CreateDrafter();
    var participantId = CreateParticipantId(drafter);
    var participant = DraftPartParticipant.Create(draftPart, participantId);
    participant.SpendVeto();

    // Act & Assert
    participant.VetoesRollingOver.Should().Be(0);
  }

  [Fact]
  public void VetoOverridesRollingOver_ShouldReturnOne_WhenOverridesRemaining()
  {
    // Arrange
    var draftPart = CreateDraftPart();
    var drafter = CreateDrafter();
    var participantId = CreateParticipantId(drafter);
    var participant = DraftPartParticipant.Create(draftPart, participantId);
    participant.AddRollover(isVeto: false);

    // Act & Assert
    participant.VetoOverridesRollingOver.Should().Be(1);
  }

  [Fact]
  public void Picks_ShouldBeEmptyInitially()
  {
    // Arrange
    var draftPart = CreateDraftPart();
    var drafter = CreateDrafter();
    var participantId = CreateParticipantId(drafter);

    // Act
    var participant = DraftPartParticipant.Create(draftPart, participantId);

    // Assert
    participant.Picks.Should().BeEmpty();
  }

  [Fact]
  public void Vetoes_ShouldBeEmptyInitially()
  {
    // Arrange
    var draftPart = CreateDraftPart();
    var drafter = CreateDrafter();
    var participantId = CreateParticipantId(drafter);

    // Act
    var participant = DraftPartParticipant.Create(draftPart, participantId);

    // Assert
    participant.Vetoes.Should().BeEmpty();
  }

  [Fact]
  public void VetoOverrides_ShouldBeEmptyInitially()
  {
    // Arrange
    var draftPart = CreateDraftPart();
    var drafter = CreateDrafter();
    var participantId = CreateParticipantId(drafter);

    // Act
    var participant = DraftPartParticipant.Create(draftPart, participantId);

    // Assert
    participant.VetoOverrides.Should().BeEmpty();
  }
}
