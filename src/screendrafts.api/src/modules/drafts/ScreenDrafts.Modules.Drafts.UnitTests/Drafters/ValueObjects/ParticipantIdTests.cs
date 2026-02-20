using ScreenDrafts.Modules.Drafts.Domain.Participants;

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
    var participantId = new Participant(id, kind);

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
    var participantId = new Participant(id, kind);

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
    var participantId = new Participant(id, kind);

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
    var participantId = new Participant(id, kind);

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
    var participantId1 = new Participant(id, kind);
    var participantId2 = new Participant(id, kind);

    // Act & Assert
    participantId1.Should().Be(participantId2);
    (participantId1 == participantId2).Should().BeTrue();
  }

  [Fact]
  public void Equality_ShouldReturnFalse_WhenValuesDiffer()
  {
    // Arrange
    var kind = ParticipantKind.Drafter;
    var participantId1 = new Participant(Guid.NewGuid(), kind);
    var participantId2 = new Participant(Guid.NewGuid(), kind);

    // Act & Assert
    participantId1.Should().NotBe(participantId2);
    (participantId1 == participantId2).Should().BeFalse();
  }

  [Fact]
  public void Equality_ShouldReturnFalse_WhenKindsDiffer()
  {
    // Arrange
    var id = Guid.NewGuid();
    var participantId1 = new Participant(id, ParticipantKind.Drafter);
    var participantId2 = new Participant(id, ParticipantKind.Team);

    // Act & Assert
    participantId1.Should().NotBe(participantId2);
    (participantId1 == participantId2).Should().BeFalse();
  }

  [Fact]
  public void IsDrafter_ShouldReturnTrue_WhenKindIsDrafter()
  {
    // Arrange
    var participantId = new Participant(Guid.NewGuid(), ParticipantKind.Drafter);

    // Act
    var isDrafter = participantId.IsDrafter;

    // Assert
    isDrafter.Should().BeTrue();
  }

  [Fact]
  public void IsTeam_ShouldReturnTrue_WhenKindIsTeam()
  {
    // Arrange
    var participantId = new Participant(Guid.NewGuid(), ParticipantKind.Team);

    // Act
    var isTeam = participantId.IsTeam;

    // Assert
    isTeam.Should().BeTrue();
  }
}
