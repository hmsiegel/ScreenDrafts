namespace ScreenDrafts.Modules.Drafts.UnitTests.Drafters.ValueObjects;

public class ParticipantIdTests : DraftsBaseTest
{
  [Fact]
  public void Constructor_ShouldCreateParticipantId_WhenValidParametersProvided()
  {
    // Arrange
    var id = Guid.NewGuid();
    var kind = ParticipantKind.Drafter;

    // Act
    var participantId = new ParticipantId(id, kind);

    // Assert
    participantId.Value.Should().Be(id);
    participantId.Kind.Should().Be(kind);
  }

  [Fact]
  public void HasValue_ShouldReturnTrue_WhenGuidIsNotEmpty()
  {
    // Arrange
    var id = Guid.NewGuid();
    var kind = ParticipantKind.Drafter;
    var participantId = new ParticipantId(id, kind);

    // Act
    var hasValue = participantId.HasNoValue;

    // Assert
    hasValue.Should().NotBe(true);
  }

  [Fact]
  public void HasNoValue_ShouldReturnTrue_WhenGuidIsEmpty()
  {
    // Arrange
    var id = Guid.Empty;
    var kind = ParticipantKind.Drafter;
    var participantId = new ParticipantId(id, kind);

    // Act
    var hasNoValue = participantId.HasNoValue;

    // Assert
    hasNoValue.Should().BeTrue();
  }

  [Fact]
  public void HasValue_ShouldReturnFalse_WhenGuidIsEmpty()
  {
    // Arrange
    var id = Guid.Empty;
    var kind = ParticipantKind.Drafter;
    var participantId = new ParticipantId(id, kind);

    // Act
    var hasValue = participantId.HasNoValue;

    // Assert
    hasValue.Should().BeTrue();
  }

  [Fact]
  public void Equality_ShouldReturnTrue_WhenValueAndKindMatch()
  {
    // Arrange
    var id = Guid.NewGuid();
    var kind = ParticipantKind.Drafter;
    var participantId1 = new ParticipantId(id, kind);
    var participantId2 = new ParticipantId(id, kind);

    // Act & Assert
    participantId1.Should().Be(participantId2);
    (participantId1 == participantId2).Should().BeTrue();
  }

  [Fact]
  public void Equality_ShouldReturnFalse_WhenValuesDiffer()
  {
    // Arrange
    var kind = ParticipantKind.Drafter;
    var participantId1 = new ParticipantId(Guid.NewGuid(), kind);
    var participantId2 = new ParticipantId(Guid.NewGuid(), kind);

    // Act & Assert
    participantId1.Should().NotBe(participantId2);
    (participantId1 == participantId2).Should().BeFalse();
  }

  [Fact]
  public void Equality_ShouldReturnFalse_WhenKindsDiffer()
  {
    // Arrange
    var id = Guid.NewGuid();
    var participantId1 = new ParticipantId(id, ParticipantKind.Drafter);
    var participantId2 = new ParticipantId(id, ParticipantKind.Team);

    // Act & Assert
    participantId1.Should().NotBe(participantId2);
    (participantId1 == participantId2).Should().BeFalse();
  }

  [Fact]
  public void IsDrafter_ShouldReturnTrue_WhenKindIsDrafter()
  {
    // Arrange
    var participantId = new ParticipantId(Guid.NewGuid(), ParticipantKind.Drafter);

    // Act
    var isDrafter = participantId.IsDrafter;

    // Assert
    isDrafter.Should().BeTrue();
  }

  [Fact]
  public void IsTeam_ShouldReturnTrue_WhenKindIsTeam()
  {
    // Arrange
    var participantId = new ParticipantId(Guid.NewGuid(), ParticipantKind.Team);

    // Act
    var isTeam = participantId.IsTeam;

    // Assert
    isTeam.Should().BeTrue();
  }
}
