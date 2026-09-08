using ScreenDrafts.Modules.GuestDrafts.Domain.Drafts.Entities;

namespace ScreenDrafts.Modules.GuestDrafts.UnitTests.Entities;

public class GuestDraftVetoOverrideTests : GuestDraftsBaseTest
{
  [Fact]
  public void Create_ShouldReturnSuccessResult_WhenValidParametersAreProvided()
  {
    // Arrange
    var veto = GuestDraftVetoFactory.CreateVeto().Value;
    var issuer = veto.TargetPick.PlayedByParticipant;

    // Act
    var result = VetoOverride.Create(veto, issuer);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Veto.Should().Be(veto);
    result.Value.VetoId.Should().Be(veto.Id);
    result.Value.IssuedByParticipant.Should().Be(issuer);
    result.Value.IssuedByParticipantId.Should().Be(issuer.Id);
  }

  [Fact]
  public void Create_ShouldThrowArgumentNullException_WhenVetoIsNull()
  {
    // Arrange
    var veto = GuestDraftVetoFactory.CreateVeto().Value;
    Veto? nullVeto = null;

    // Act
    Action act = () => VetoOverride.Create(nullVeto!, veto.TargetPick.PlayedByParticipant);

    // Assert
    act.Should().Throw<ArgumentNullException>();
  }

  [Fact]
  public void Create_ShouldThrowArgumentNullException_WhenIssuedByParticipantIsNull()
  {
    // Arrange
    var veto = GuestDraftVetoFactory.CreateVeto().Value;
    DraftParticipant? issuer = null;

    // Act
    Action act = () => VetoOverride.Create(veto, issuer!);

    // Assert
    act.Should().Throw<ArgumentNullException>();
  }

  [Fact]
  public void Create_ShouldAcceptNote_WhenProvided()
  {
    // Arrange
    var veto = GuestDraftVetoFactory.CreateVeto().Value;
    var note = "Overridden after a rewatch";

    // Act
    var result = VetoOverride.Create(veto, veto.TargetPick.PlayedByParticipant, note: note);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Note.Should().Be(note);
  }

  [Fact]
  public void Create_ShouldSetSpentFromFungiblePool_WhenProvided()
  {
    // Arrange
    var veto = GuestDraftVetoFactory.CreateVeto().Value;

    // Act
    var result = VetoOverride.Create(
      veto,
      veto.TargetPick.PlayedByParticipant,
      spentFromFungiblePool: true
    );

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.SpentFromFungiblePool.Should().BeTrue();
  }
}
