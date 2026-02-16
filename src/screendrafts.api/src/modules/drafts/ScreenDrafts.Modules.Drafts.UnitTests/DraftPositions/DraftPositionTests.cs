namespace ScreenDrafts.Modules.Drafts.UnitTests.DraftPositions;

public class DraftPositionTests : DraftsBaseTest
{
  [Fact]
  public void Create_ShouldReturnSuccessResult_WhenValidParametersAreProvided()
  {
    // Arrange
    var gameBoard = CreateGameBoard();
    var name = "Position A";
    var picks = new Collection<int> { 1, 2, 3 };

    // Act
    var result = DraftPosition.Create(gameBoard, name, picks);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Name.Should().Be(name);
    result.Value.Picks.Should().BeEquivalentTo(picks);
    result.Value.GameBoard.Should().Be(gameBoard);
    result.Value.AssignedTo.Should().BeNull();
  }

  [Fact]
  public void Create_ShouldRaiseDomainEvent_WhenPositionIsCreated()
  {
    // Arrange
    var gameBoard = CreateGameBoard();
    var name = "Position A";
    var picks = new Collection<int> { 1, 2, 3 };

    // Act
    var position = DraftPosition.Create(gameBoard, name, picks).Value;

    // Assert
    var domainEvent = AssertDomainEventWasPublished<DraftPositionCreatedDomainEvent>(position);
    domainEvent.DraftPositionId.Should().Be(position.Id);
  }

  [Fact]
  public void Create_ShouldReturnFailure_WhenNameIsNullOrEmpty()
  {
    // Arrange
    var gameBoard = CreateGameBoard();
    var picks = new Collection<int> { 1, 2, 3 };

    // Act
    var result = DraftPosition.Create(gameBoard, string.Empty, picks);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(DraftPositionErrors.NameIsRequired);
  }

  [Fact]
  public void Create_ShouldReturnFailure_WhenPicksAreEmpty()
  {
    // Arrange
    var gameBoard = CreateGameBoard();
    var name = "Position A";
    var picks = new Collection<int>();

    // Act
    var result = DraftPosition.Create(gameBoard, name, picks);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(DraftPositionErrors.PicksAreRequired);
  }

  [Fact]
  public void Create_ShouldThrowArgumentNullException_WhenGameBoardIsNull()
  {
    // Arrange
    var name = "Position A";
    var picks = new Collection<int> { 1, 2, 3 };

    // Act
    Action act = () => DraftPosition.Create(null!, name, picks);

    // Assert
    act.Should().Throw<ArgumentNullException>();
  }

  [Fact]
  public void Create_ShouldSetBonusFlags_WhenProvided()
  {
    // Arrange
    var gameBoard = CreateGameBoard();
    var name = "Position A";
    var picks = new Collection<int> { 1, 2, 3 };
    var hasBonusVeto = true;
    var hasBonusVetoOverride = true;

    // Act
    var result = DraftPosition.Create(gameBoard, name, picks, hasBonusVeto, hasBonusVetoOverride);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.HasBonusVeto.Should().BeTrue();
    result.Value.HasBonusVetoOverride.Should().BeTrue();
  }

  [Fact]
  public void AssignParticipant_ShouldSucceed_WhenPositionIsNotAssigned()
  {
    // Arrange
    var position = CreateDraftPosition();
    var drafter = CreateDrafter();
    var participantId = CreateParticipantId(drafter);

    // Act
    var result = position.AssignParticipant(participantId);

    // Assert
    result.IsSuccess.Should().BeTrue();
    position.AssignedTo.Should().Be(participantId);
    position.AssignedToId.Should().Be(participantId.Value);
    position.AssignedToKind.Should().Be(participantId.Kind);
  }

  [Fact]
  public void AssignParticipant_ShouldFail_WhenPositionIsAlreadyAssigned()
  {
    // Arrange
    var position = CreateDraftPosition();
    var drafter1 = CreateDrafter();
    var drafter2 = CreateDrafter();
    var participantId1 = CreateParticipantId(drafter1);
    var participantId2 = CreateParticipantId(drafter2);
    position.AssignParticipant(participantId1);

    // Act
    var result = position.AssignParticipant(participantId2);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(DraftPositionErrors.PositionIsAlreadyAssigned);
  }

  [Fact]
  public void AssignParticipant_ShouldRaiseDomainEvent_WhenParticipantIsAssigned()
  {
    // Arrange
    var position = CreateDraftPosition();
    var drafter = CreateDrafter();
    var participantId = CreateParticipantId(drafter);

    // Act
    position.AssignParticipant(participantId);

    // Assert
    var domainEvent = AssertDomainEventWasPublished<DraftPositionAssignedDomainEvent>(position);
    domainEvent.DraftPositionId.Should().Be(position.Id.Value);
    domainEvent.ParticipantId.Should().Be(participantId.Value);
    domainEvent.ParticipantKind.Should().Be(participantId.Kind.Value);
  }

  [Fact]
  public void ClearAssignment_ShouldSucceed_WhenPositionIsAssigned()
  {
    // Arrange
    var position = CreateDraftPosition();
    var drafter = CreateDrafter();
    var participantId = CreateParticipantId(drafter);
    position.AssignParticipant(participantId);

    // Act
    var result = position.ClearAssignment();

    // Assert
    result.IsSuccess.Should().BeTrue();
    position.AssignedTo.Should().BeNull();
    position.AssignedToId.Should().BeNull();
    position.AssignedToKind.Should().BeNull();
  }

  [Fact]
  public void ClearAssignment_ShouldFail_WhenPositionIsNotAssigned()
  {
    // Arrange
    var position = CreateDraftPosition();

    // Act
    var result = position.ClearAssignment();

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(DraftPositionErrors.PositionIsNotAssigned);
  }

  [Fact]
  public void ClearAssignment_ShouldRaiseDomainEvent_WhenAssignmentIsCleared()
  {
    // Arrange
    var position = CreateDraftPosition();
    var drafter = CreateDrafter();
    var participantId = CreateParticipantId(drafter);
    position.AssignParticipant(participantId);

    // Act
    position.ClearAssignment();

    // Assert
    var domainEvent = AssertDomainEventWasPublished<DraftPositionUnassignedDomainEvent>(position);
    domainEvent.DraftPositionId.Should().Be(position.Id.Value);
  }

  [Fact]
  public void AssignedTo_ShouldReturnNull_WhenNotAssigned()
  {
    // Arrange
    var position = CreateDraftPosition();

    // Act & Assert
    position.AssignedTo.Should().BeNull();
    position.AssignedToId.Should().BeNull();
    position.AssignedToKind.Should().BeNull();
  }

  [Fact]
  public void AssignedTo_ShouldReturnParticipantId_WhenAssigned()
  {
    // Arrange
    var position = CreateDraftPosition();
    var drafter = CreateDrafter();
    var participantId = CreateParticipantId(drafter);
    position.AssignParticipant(participantId);

    // Act & Assert
    position.AssignedTo.Should().NotBeNull();
    position.AssignedTo.Should().Be(participantId);
  }

  private static DraftPosition CreateDraftPosition()
  {
    var gameBoard = CreateGameBoard();
    var name = "Position A";
    var picks = new Collection<int> { 1, 2, 3 };

    return DraftPosition.Create(gameBoard, name, picks).Value;
  }

  private static GameBoard CreateGameBoard()
  {
    var draftPart = CreateDraftPart();
    return GameBoard.Create(draftPart).Value;
  }
}
