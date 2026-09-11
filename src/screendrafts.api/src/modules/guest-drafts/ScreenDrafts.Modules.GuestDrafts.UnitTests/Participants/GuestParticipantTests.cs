using ScreenDrafts.Modules.GuestDrafts.Domain.Drafters;
using ScreenDrafts.Modules.GuestDrafts.Domain.DrafterTeams;

namespace ScreenDrafts.Modules.GuestDrafts.UnitTests.Participants;

public class GuestParticipantTests : GuestDraftsBaseTest
{
  [Fact]
  public void From_GuestDrafterId_ShouldProduceDrafterKind()
  {
    // Arrange
    var id = DrafterId.CreateUnique();

    // Act
    var participant = Participant.From(id);

    // Assert
    participant.Value.Should().Be(id.Value);
    participant.Kind.Should().Be(ParticipantKind.Drafter);
    participant.IsDrafter.Should().BeTrue();
    participant.IsTeam.Should().BeFalse();
  }

  [Fact]
  public void From_GuestDrafterTeamId_ShouldProduceTeamKind()
  {
    // Arrange
    var id = DrafterTeamId.CreateUnique();

    // Act
    var participant = Participant.From(id);

    // Assert
    participant.Value.Should().Be(id.Value);
    participant.Kind.Should().Be(ParticipantKind.Team);
    participant.IsTeam.Should().BeTrue();
    participant.IsDrafter.Should().BeFalse();
  }

  [Fact]
  public void HasNoValue_ShouldReturnTrue_WhenValueIsEmpty()
  {
    // Arrange
    var participant = new Participant(Guid.Empty, ParticipantKind.Drafter);

    // Act & Assert
    participant.HasNoValue.Should().BeTrue();
  }

  [Fact]
  public void HasNoValue_ShouldReturnFalse_WhenValueIsNotEmpty()
  {
    // Arrange
    var participant = Participant.From(DrafterId.CreateUnique());

    // Act & Assert
    participant.HasNoValue.Should().BeFalse();
  }

  [Fact]
  public void AsGuestDrafterId_ShouldReturnTheId_WhenKindIsDrafter()
  {
    // Arrange
    var id = DrafterId.CreateUnique();
    var participant = Participant.From(id);

    // Act & Assert
    participant.AsGuestDrafterId().Should().Be(id);
  }

  [Fact]
  public void AsGuestDrafterId_ShouldThrow_WhenKindIsTeam()
  {
    // Arrange
    var participant = Participant.From(DrafterTeamId.CreateUnique());

    // Act
    Action act = () => participant.AsGuestDrafterId();

    // Assert
    act.Should().Throw<ScreenDraftsException>();
  }

  [Fact]
  public void AsGuestDrafterTeamId_ShouldReturnTheId_WhenKindIsTeam()
  {
    // Arrange
    var id = DrafterTeamId.CreateUnique();
    var participant = Participant.From(id);

    // Act & Assert
    participant.AsGuestDrafterTeamId().Should().Be(id);
  }

  [Fact]
  public void AsGuestDrafterTeamId_ShouldThrow_WhenKindIsDrafter()
  {
    // Arrange
    var participant = Participant.From(DrafterId.CreateUnique());

    // Act
    Action act = () => participant.AsGuestDrafterTeamId();

    // Assert
    act.Should().Throw<ScreenDraftsException>();
  }

  [Fact]
  public void Validate_ShouldReturnFailure_WhenValueIsEmpty()
  {
    // Arrange
    var participant = new Participant(Guid.Empty, ParticipantKind.Drafter);

    // Act
    var result = participant.Validate();

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(ParticipantErrors.EmptyValue);
  }

  [Fact]
  public void Validate_ShouldSucceed_WhenValueAndKindAreValid()
  {
    // Arrange
    var participant = Participant.From(DrafterId.CreateUnique());

    // Act
    var result = participant.Validate();

    // Assert
    result.IsSuccess.Should().BeTrue();
  }
}
