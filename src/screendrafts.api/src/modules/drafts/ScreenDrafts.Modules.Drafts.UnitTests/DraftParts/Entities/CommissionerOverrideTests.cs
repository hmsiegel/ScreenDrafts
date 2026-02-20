namespace ScreenDrafts.Modules.Drafts.UnitTests.DraftParts.Entities;

public class CommissionerOverrideTests : DraftsBaseTest
{
  [Fact]
  public void Create_ShouldReturnSuccessResult_WhenValidParametersAreProvided()
  {
    // Arrange
    var pick = PickFactory.CreatePick().Value;

    // Act
    var result = CommissionerOverride.Create(pick);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Pick.Should().Be(pick);
    result.Value.PickId.Should().Be(pick.Id);
  }

  [Fact]
  public void Create_ShouldRaiseDomainEvent_WhenCommissionerOverrideIsCreated()
  {
    // Arrange
    var pick = PickFactory.CreatePick().Value;

    // Act
    var commissionerOverride = CommissionerOverride.Create(pick).Value;

    // Assert
    var domainEvent = AssertDomainEventWasPublished<CommissionerOverrideCreatedDomainEvent>(commissionerOverride);
    domainEvent.CommissionerOverrideId.Should().Be(commissionerOverride.Id);
    domainEvent.PickId.Should().Be(pick.Id.Value);
  }

  [Fact]
  public void Create_ShouldReturnFailure_WhenPickIsNull()
  {
    // Arrange
    Pick? pick = null;

    // Act
    var result = CommissionerOverride.Create(pick!);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(CommissionerOverrideErrors.PickRequired);
  }

  [Fact]
  public void PickId_ShouldMatchPick_WhenCreated()
  {
    // Arrange
    var pick = PickFactory.CreatePick().Value;

    // Act
    var commissionerOverride = CommissionerOverride.Create(pick).Value;

    // Assert
    commissionerOverride.PickId.Should().Be(pick.Id);
    commissionerOverride.Pick.Should().Be(pick);
  }

  [Fact]
  public void DraftPart_ShouldReturnCorrectDraftPart_FromPick()
  {
    // Arrange
    var pick = PickFactory.CreatePick().Value;

    // Act
    var commissionerOverride = CommissionerOverride.Create(pick).Value;

    // Assert
    commissionerOverride.Pick.DraftPart.Should().Be(pick.DraftPart);
    commissionerOverride.Pick.DraftPartId.Should().Be(pick.DraftPartId);
  }
}
