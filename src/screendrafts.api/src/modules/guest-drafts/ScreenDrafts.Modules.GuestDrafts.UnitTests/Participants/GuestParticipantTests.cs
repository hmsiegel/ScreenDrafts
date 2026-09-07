namespace ScreenDrafts.Modules.GuestDrafts.UnitTests.Participants;

public class GuestParticipantTests : GuestDraftsBaseTest
{
  [Fact]
  public void From_GuestDrafterId_ShouldProduceDrafterKind()
  {
    // Arrange
    var id = GuestDrafterId.CreateUnique();

    // Act
    var participant = GuestParticipant.From(id);

    // Assert
    participant.Value.Should().Be(id.Value);
    participant.Kind.Should().Be(GuestParticipantKind.Drafter);
    participant.IsDrafter.Should().BeTrue();
    participant.IsTeam.Should().BeFalse();
  }

  [Fact]
  public void From_GuestDrafterTeamId_ShouldProduceTeamKind()
  {
    // Arrange
    var id = GuestDrafterTeamId.CreateUnique();

    // Act
    var participant = GuestParticipant.From(id);

    // Assert
    participant.Value.Should().Be(id.Value);
    participant.Kind.Should().Be(GuestParticipantKind.Team);
    participant.IsTeam.Should().BeTrue();
    participant.IsDrafter.Should().BeFalse();
  }

  [Fact]
  public void HasNoValue_ShouldReturnTrue_WhenValueIsEmpty()
  {
    // Arrange
    var participant = new GuestParticipant(Guid.Empty, GuestParticipantKind.Drafter);

    // Act & Assert
    participant.HasNoValue.Should().BeTrue();
  }

  [Fact]
  public void HasNoValue_ShouldReturnFalse_WhenValueIsNotEmpty()
  {
    // Arrange
    var participant = GuestParticipant.From(GuestDrafterId.CreateUnique());

    // Act & Assert
    participant.HasNoValue.Should().BeFalse();
  }

  [Fact]
  public void AsGuestDrafterId_ShouldReturnTheId_WhenKindIsDrafter()
  {
    // Arrange
    var id = GuestDrafterId.CreateUnique();
    var participant = GuestParticipant.From(id);

    // Act & Assert
    participant.AsGuestDrafterId().Should().Be(id);
  }

  [Fact]
  public void AsGuestDrafterId_ShouldThrow_WhenKindIsTeam()
  {
    // Arrange
    var participant = GuestParticipant.From(GuestDrafterTeamId.CreateUnique());

    // Act
    Action act = () => participant.AsGuestDrafterId();

    // Assert
    act.Should().Throw<ScreenDraftsException>();
  }

  [Fact]
  public void AsGuestDrafterTeamId_ShouldReturnTheId_WhenKindIsTeam()
  {
    // Arrange
    var id = GuestDrafterTeamId.CreateUnique();
    var participant = GuestParticipant.From(id);

    // Act & Assert
    participant.AsGuestDrafterTeamId().Should().Be(id);
  }

  [Fact]
  public void AsGuestDrafterTeamId_ShouldThrow_WhenKindIsDrafter()
  {
    // Arrange
    var participant = GuestParticipant.From(GuestDrafterId.CreateUnique());

    // Act
    Action act = () => participant.AsGuestDrafterTeamId();

    // Assert
    act.Should().Throw<ScreenDraftsException>();
  }

  [Fact]
  public void Validate_ShouldReturnFailure_WhenValueIsEmpty()
  {
    // Arrange
    var participant = new GuestParticipant(Guid.Empty, GuestParticipantKind.Drafter);

    // Act
    var result = participant.Validate();

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(GuestParticipantErrors.EmptyValue);
  }

  [Fact]
  public void Validate_ShouldSucceed_WhenValueAndKindAreValid()
  {
    // Arrange
    var participant = GuestParticipant.From(GuestDrafterId.CreateUnique());

    // Act
    var result = participant.Validate();

    // Assert
    result.IsSuccess.Should().BeTrue();
  }
}
