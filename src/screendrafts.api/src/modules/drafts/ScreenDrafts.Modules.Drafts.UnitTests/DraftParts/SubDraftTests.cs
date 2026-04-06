namespace ScreenDrafts.Modules.Drafts.UnitTests.DraftParts;

public sealed class SubDraftTests : DraftsBaseTest
{
  // ========================================
  // SubDraft.Create Tests
  // ========================================

  [Fact]
  public void Create_WithValidParameters_ShouldSucceed()
  {
    // Arrange
    var draftPartId = DraftPartId.CreateUnique();
    var publicId = $"sd_{Faker.Random.AlphaNumeric(21)}";
    var index = 0;

    // Act
    var result = SubDraft.Create(index, draftPartId, publicId);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Index.Should().Be(index);
    result.Value.PublicId.Should().Be(publicId);
    result.Value.DraftPartId.Should().Be(draftPartId);
    result.Value.Status.Should().Be(SubDraftStatus.Pending);
    result.Value.SubjectKind.Should().BeNull();
    result.Value.SubjectName.Should().BeNull();
  }

  [Fact]
  public void Create_WithNegativeIndex_ShouldFail()
  {
    // Arrange
    var draftPartId = DraftPartId.CreateUnique();
    var publicId = $"sd_{Faker.Random.AlphaNumeric(21)}";

    // Act
    var result = SubDraft.Create(-1, draftPartId, publicId);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(DraftPartErrors.PartIndexIsOutOfRange);
  }

  [Fact]
  public void Create_WithIndexZero_ShouldSucceed()
  {
    // Arrange
    var draftPartId = DraftPartId.CreateUnique();
    var publicId = $"sd_{Faker.Random.AlphaNumeric(21)}";

    // Act
    var result = SubDraft.Create(0, draftPartId, publicId);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Index.Should().Be(0);
  }

  [Fact]
  public void Create_ShouldCreateWithItsOwnGameBoard()
  {
    // Arrange
    var draftPartId = DraftPartId.CreateUnique();
    var publicId = $"sd_{Faker.Random.AlphaNumeric(21)}";

    // Act
    var result = SubDraft.Create(0, draftPartId, publicId);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.GameBoard.Should().NotBeNull();
  }

  // ========================================
  // SubDraft.SetSubject Tests
  // ========================================

  [Fact]
  public void SetSubject_WithValidParameters_ShouldSucceed()
  {
    // Arrange
    var subDraft = CreateSubDraft();

    // Act
    var result = subDraft.SetSubject(SubjectKind.Actor, "Tom Hanks");

    // Assert
    result.IsSuccess.Should().BeTrue();
    subDraft.SubjectKind.Should().Be(SubjectKind.Actor);
    subDraft.SubjectName.Should().Be("Tom Hanks");
  }

  [Fact]
  public void SetSubject_WithEmptyName_ShouldFail()
  {
    // Arrange
    var subDraft = CreateSubDraft();

    // Act
    var result = subDraft.SetSubject(SubjectKind.Director, string.Empty);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(SubDraftErrors.SubjectNameCannotBeEmpty);
  }

  [Fact]
  public void SetSubject_WithWhitespaceName_ShouldFail()
  {
    // Arrange
    var subDraft = CreateSubDraft();

    // Act
    var result = subDraft.SetSubject(SubjectKind.Word, "   ");

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(SubDraftErrors.SubjectNameCannotBeEmpty);
  }

  [Fact]
  public void SetSubject_ShouldTrimSubjectName()
  {
    // Arrange
    var subDraft = CreateSubDraft();

    // Act
    var result = subDraft.SetSubject(SubjectKind.Actor, "  Steven Spielberg  ");

    // Assert
    result.IsSuccess.Should().BeTrue();
    subDraft.SubjectName.Should().Be("Steven Spielberg");
  }

  [Fact]
  public void SetSubject_CanBeCalledMultipleTimes_ShouldUpdateSubject()
  {
    // Arrange
    var subDraft = CreateSubDraft();
    subDraft.SetSubject(SubjectKind.Actor, "Tom Hanks");

    // Act
    var result = subDraft.SetSubject(SubjectKind.Director, "Christopher Nolan");

    // Assert
    result.IsSuccess.Should().BeTrue();
    subDraft.SubjectKind.Should().Be(SubjectKind.Director);
    subDraft.SubjectName.Should().Be("Christopher Nolan");
  }

  // ========================================
  // SubDraft.Activate Tests
  // ========================================

  [Fact]
  public void Activate_FromPendingStatus_ShouldSucceedAndSetActiveStatus()
  {
    // Arrange
    var subDraft = CreateSubDraft(); // Pending by default

    // Act
    var result = subDraft.Activate();

    // Assert
    result.IsSuccess.Should().BeTrue();
    subDraft.Status.Should().Be(SubDraftStatus.Active);
  }

  [Fact]
  public void Activate_FromActiveStatus_ShouldFail()
  {
    // Arrange
    var subDraft = CreateSubDraft();
    subDraft.Activate();

    // Act
    var result = subDraft.Activate();

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(SubDraftErrors.CannotActivateSubDraft);
  }

  [Fact]
  public void Activate_FromCompletedStatus_ShouldFail()
  {
    // Arrange
    var subDraft = CreateSubDraft();
    subDraft.Activate();
    subDraft.Complete();

    // Act
    var result = subDraft.Activate();

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(SubDraftErrors.CannotActivateSubDraft);
  }

  // ========================================
  // SubDraft.Complete Tests
  // ========================================

  [Fact]
  public void Complete_FromActiveStatus_ShouldSucceedAndSetCompletedStatus()
  {
    // Arrange
    var subDraft = CreateSubDraft();
    subDraft.Activate();

    // Act
    var result = subDraft.Complete();

    // Assert
    result.IsSuccess.Should().BeTrue();
    subDraft.Status.Should().Be(SubDraftStatus.Completed);
  }

  [Fact]
  public void Complete_FromPendingStatus_ShouldFail()
  {
    // Arrange
    var subDraft = CreateSubDraft();

    // Act
    var result = subDraft.Complete();

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(SubDraftErrors.CannotCompleteSubDraft);
  }

  [Fact]
  public void Complete_FromCompletedStatus_ShouldFail()
  {
    // Arrange
    var subDraft = CreateSubDraft();
    subDraft.Activate();
    subDraft.Complete();

    // Act
    var result = subDraft.Complete();

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(SubDraftErrors.CannotCompleteSubDraft);
  }

  // ========================================
  // SubDraft.ComputeVetoRemainder Tests
  // ========================================

  [Fact]
  public void ComputeVetoRemainder_WithNoVetoes_ShouldReturnStartingVetoes()
  {
    // Arrange
    var subDraft = CreateSubDraft();
    var startingVetoes = 1;
    var vetoes = Array.Empty<(SubDraftId SubDraftId, bool IsOverridden)>();

    // Act
    var remainder = subDraft.ComputeVetoRemainder(startingVetoes, vetoes);

    // Assert
    remainder.Should().Be(1);
  }

  [Fact]
  public void ComputeVetoRemainder_WithOneVetoUsedByThisSubDraft_ShouldReturnZero()
  {
    // Arrange
    var subDraft = CreateSubDraft();
    var startingVetoes = 1;
    var vetoes = new[] { (SubDraftId: subDraft.Id, IsOverridden: false) };

    // Act
    var remainder = subDraft.ComputeVetoRemainder(startingVetoes, vetoes);

    // Assert
    remainder.Should().Be(0);
  }

  [Fact]
  public void ComputeVetoRemainder_WithVetoUsedByDifferentSubDraft_ShouldNotCountIt()
  {
    // Arrange
    var subDraft = CreateSubDraft();
    var otherSubDraftId = SubDraftId.CreateUnique();
    var startingVetoes = 1;
    var vetoes = new[] { (SubDraftId: otherSubDraftId, IsOverridden: false) };

    // Act
    var remainder = subDraft.ComputeVetoRemainder(startingVetoes, vetoes);

    // Assert
    remainder.Should().Be(1);
  }

  [Fact]
  public void ComputeVetoRemainder_WithMoreVetoesThanUsed_ShouldReturnPositiveRemainder()
  {
    // Arrange
    var subDraft = CreateSubDraft();
    var startingVetoes = 3;
    var vetoes = new[]
    {
      (SubDraftId: subDraft.Id, IsOverridden: false),
    };

    // Act
    var remainder = subDraft.ComputeVetoRemainder(startingVetoes, vetoes);

    // Assert
    remainder.Should().Be(2);
  }

  [Fact]
  public void ComputeVetoRemainder_WithMoreUsedThanAvailable_ShouldReturnZero()
  {
    // Arrange
    var subDraft = CreateSubDraft();
    var startingVetoes = 0;
    var vetoes = new[]
    {
      (SubDraftId: subDraft.Id, IsOverridden: false),
    };

    // Act
    var remainder = subDraft.ComputeVetoRemainder(startingVetoes, vetoes);

    // Assert
    remainder.Should().Be(0);
  }

  // ========================================
  // Helpers
  // ========================================

  private static SubDraft CreateSubDraft(int index = 0)
  {
    var draftPartId = DraftPartId.CreateUnique();
    var publicId = $"sd_{Faker.Random.AlphaNumeric(21)}";
    return SubDraft.Create(index, draftPartId, publicId).Value;
  }
}
