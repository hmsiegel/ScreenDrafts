namespace ScreenDrafts.Modules.Drafts.UnitTests.DraftParts;

public class DraftPartZoomSessionTests : DraftsBaseTest
{
  // ========================================
  // SetZoomSessionName — happy path
  // ========================================

  [Fact]
  public void SetZoomSessionName_ShouldReturnSuccess_WhenNoActiveSession()
  {
    // Arrange
    var draftPart = CreateDraftPart();

    // Act
    var result = draftPart.SetZoomSessionName("screendrafts-dp-abc123");

    // Assert
    result.IsSuccess.Should().BeTrue();
  }

  [Fact]
  public void SetZoomSessionName_ShouldPersistSessionName_WhenNoActiveSession()
  {
    // Arrange
    var draftPart = CreateDraftPart();
    var sessionName = "screendrafts-dp-abc123";

    // Act
    draftPart.SetZoomSessionName(sessionName);

    // Assert
    draftPart.ZoomSessionName.Should().Be(sessionName);
  }

  [Fact]
  public void SetZoomSessionName_ShouldUpdateUpdatedAtUtc_WhenSuccessful()
  {
    // Arrange
    var draftPart = CreateDraftPart();
    var before = DateTime.UtcNow.AddSeconds(-1);

    // Act
    draftPart.SetZoomSessionName("screendrafts-dp-abc123");

    // Assert
    draftPart.UpdatedAtUtc.Should().NotBeNull();
    draftPart.UpdatedAtUtc!.Value.Should().BeAfter(before);
  }

  // ========================================
  // SetZoomSessionName — guard clauses
  // ========================================

  [Fact]
  public void SetZoomSessionName_ShouldReturnFailure_WhenSessionNameIsNull()
  {
    // Arrange
    var draftPart = CreateDraftPart();

    // Act
    var result = draftPart.SetZoomSessionName(null!);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(DraftPartErrors.InvalidZoomSessionName);
  }

  [Fact]
  public void SetZoomSessionName_ShouldReturnFailure_WhenSessionNameIsEmpty()
  {
    // Arrange
    var draftPart = CreateDraftPart();

    // Act
    var result = draftPart.SetZoomSessionName(string.Empty);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(DraftPartErrors.InvalidZoomSessionName);
  }

  [Fact]
  public void SetZoomSessionName_ShouldReturnFailure_WhenSessionNameIsWhitespace()
  {
    // Arrange
    var draftPart = CreateDraftPart();

    // Act
    var result = draftPart.SetZoomSessionName("   ");

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(DraftPartErrors.InvalidZoomSessionName);
  }

  [Fact]
  public void SetZoomSessionName_ShouldReturnFailure_WhenSessionAlreadyActive()
  {
    // Arrange
    var draftPart = CreateDraftPart();
    draftPart.SetZoomSessionName("screendrafts-first");

    // Act
    var result = draftPart.SetZoomSessionName("screendrafts-second");

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(DraftPartErrors.ZoomSessionAlreadyActive);
  }

  [Fact]
  public void SetZoomSessionName_ShouldNotOverwriteExistingSession_WhenSessionAlreadyActive()
  {
    // Arrange
    var draftPart = CreateDraftPart();
    var original = "screendrafts-original";
    draftPart.SetZoomSessionName(original);

    // Act
    draftPart.SetZoomSessionName("screendrafts-replacement");

    // Assert
    draftPart.ZoomSessionName.Should().Be(original);
  }

  // ========================================
  // ClearZoomSessionName — happy path
  // ========================================

  [Fact]
  public void ClearZoomSessionName_ShouldReturnSuccess_WhenSessionIsActive()
  {
    // Arrange
    var draftPart = CreateDraftPart();
    draftPart.SetZoomSessionName("screendrafts-dp-abc123");

    // Act
    var result = draftPart.ClearZoomSessionName();

    // Assert
    result.IsSuccess.Should().BeTrue();
  }

  [Fact]
  public void ClearZoomSessionName_ShouldNullifySessionName_WhenSessionIsActive()
  {
    // Arrange
    var draftPart = CreateDraftPart();
    draftPart.SetZoomSessionName("screendrafts-dp-abc123");

    // Act
    draftPart.ClearZoomSessionName();

    // Assert
    draftPart.ZoomSessionName.Should().BeNull();
  }

  [Fact]
  public void ClearZoomSessionName_ShouldUpdateUpdatedAtUtc_WhenSuccessful()
  {
    // Arrange
    var draftPart = CreateDraftPart();
    draftPart.SetZoomSessionName("screendrafts-dp-abc123");
    var before = DateTime.UtcNow.AddSeconds(-1);

    // Act
    draftPart.ClearZoomSessionName();

    // Assert
    draftPart.UpdatedAtUtc.Should().NotBeNull();
    draftPart.UpdatedAtUtc!.Value.Should().BeAfter(before);
  }

  // ========================================
  // ClearZoomSessionName — guard clauses
  // ========================================

  [Fact]
  public void ClearZoomSessionName_ShouldReturnFailure_WhenNoActiveSession()
  {
    // Arrange
    var draftPart = CreateDraftPart();

    // Act
    var result = draftPart.ClearZoomSessionName();

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(DraftPartErrors.NoActiveZoomSession);
  }

  // ========================================
  // Round-trip: set → clear → set again
  // ========================================

  [Fact]
  public void SetZoomSessionName_ShouldSucceed_AfterClearingPreviousSession()
  {
    // Arrange
    var draftPart = CreateDraftPart();
    draftPart.SetZoomSessionName("screendrafts-first");
    draftPart.ClearZoomSessionName();

    // Act
    var result = draftPart.SetZoomSessionName("screendrafts-second");

    // Assert
    result.IsSuccess.Should().BeTrue();
    draftPart.ZoomSessionName.Should().Be("screendrafts-second");
  }
}
