namespace ScreenDrafts.Modules.Drafts.UnitTests.Picks;

public class PickTests : DraftsBaseTest
{
  [Fact]
  public void Pick_ShouldHaveCorrectInitialState_WhenCreated()
  {
    // Arrange & Act
    var pick = PickFactory.CreatePick().Value;

    // Assert
    pick.Veto.Should().BeNull();
    pick.VetoId.Should().BeNull();
    pick.IsVetoed.Should().BeFalse();
    pick.CommissionerOverride.Should().BeNull();
    pick.IsActiveOnFinalBoard.Should().BeTrue();
  }

  [Fact]
  public void IsActiveOnFinalBoard_ShouldReturnTrue_WhenPickIsNotVetoedOrOverridden()
  {
    // Arrange
    var pick = PickFactory.CreatePick().Value;

    // Act & Assert
    pick.IsActiveOnFinalBoard.Should().BeTrue();
  }

  [Fact]
  public void PlayedByParticipant_ShouldNotBeNull_WhenCreated()
  {
    // Arrange & Act
    var pick = PickFactory.CreatePick().Value;

    // Assert
    pick.PlayedByParticipant.Should().NotBeNull();
    pick.PlayedByParticipantId.Should().NotBeNull();
    pick.PlayedByParticipant.ParticipantIdValue.Should().NotBeEmpty();
  }

  [Fact]
  public void Pick_ShouldHaveValidMovie_WhenCreated()
  {
    // Arrange & Act
    var pick = PickFactory.CreatePick().Value;

    // Assert
    pick.Movie.Should().NotBeNull();
    pick.MovieId.Should().NotBe(Guid.Empty);
  }

  [Fact]
  public void Pick_ShouldHaveValidDraftPart_WhenCreated()
  {
    // Arrange & Act
    var pick = PickFactory.CreatePick().Value;

    // Assert
    pick.DraftPart.Should().NotBeNull();
    pick.DraftPartId.Should().NotBeNull();
  }

  [Fact]
  public void Pick_ShouldHaveValidPosition_WhenCreated()
  {
    // Arrange & Act
    var pick = PickFactory.CreatePick().Value;

    // Assert
    pick.Position.Should().BeGreaterThan(0);
  }

  [Fact]
  public void Pick_ShouldHaveValidPlayOrder_WhenCreated()
  {
    // Arrange & Act
    var pick = PickFactory.CreatePick().Value;

    // Assert
    pick.PlayOrder.Should().BeGreaterThan(0);
  }

  [Fact]
  public void History_ShouldBeEmpty_WhenPickIsCreated()
  {
    // Arrange & Act
    var pick = PickFactory.CreatePick().Value;

    // Assert
    pick.History.Should().BeEmpty();
  }

  [Fact]
  public void PlayedByParticipantId_ShouldMatchDraftPartParticipantId()
  {
    // Arrange & Act
    var pick = PickFactory.CreatePick().Value;

    // Assert
    pick.PlayedByParticipantId.Should().Be(pick.PlayedByParticipant.Id);
  }

  [Fact]
  public void ParticipantId_ShouldHaveCorrectKind()
  {
    // Arrange & Act
    var pick = PickFactory.CreatePick().Value;

    // Assert
    pick.PlayedByParticipant.ParticipantId.Kind.Should().NotBeNull();
    pick.PlayedByParticipant.ParticipantId.Value.Should().NotBe(Guid.Empty);
  }
}

