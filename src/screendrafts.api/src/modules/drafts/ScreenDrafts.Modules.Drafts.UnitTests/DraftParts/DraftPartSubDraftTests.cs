namespace ScreenDrafts.Modules.Drafts.UnitTests.DraftParts;

public sealed class DraftPartSubDraftTests : DraftsBaseTest
{
  // ========================================
  // DraftPart.AddSubDraft Tests
  // ========================================

  [Fact]
  public void AddSubDraft_OnSpeedDraftPart_ShouldSucceed()
  {
    // Arrange
    var draftPart = CreateSpeedDraftPart();
    var publicId = $"sd_{Faker.Random.AlphaNumeric(21)}";

    // Act
    var result = draftPart.AddSubDraft(0, publicId);

    // Assert
    result.IsSuccess.Should().BeTrue();
    draftPart.SubDrafts.Should().ContainSingle();
    draftPart.SubDrafts.First().Index.Should().Be(0);
    draftPart.SubDrafts.First().PublicId.Should().Be(publicId);
  }

  [Fact]
  public void AddSubDraft_OnStandardDraftPart_ShouldFail()
  {
    // Arrange
    var draftPart = CreateDraftPart();
    var publicId = $"sd_{Faker.Random.AlphaNumeric(21)}";

    // Act
    var result = draftPart.AddSubDraft(0, publicId);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(SubDraftErrors.SubDraftsOnlyAllowedForSpeedDrafts);
  }

  [Fact]
  public void AddSubDraft_WithDuplicateIndex_ShouldFail()
  {
    // Arrange
    var draftPart = CreateSpeedDraftPart();
    var publicId1 = $"sd_{Faker.Random.AlphaNumeric(21)}";
    var publicId2 = $"sd_{Faker.Random.AlphaNumeric(21)}";
    draftPart.AddSubDraft(0, publicId1);

    // Act
    var result = draftPart.AddSubDraft(0, publicId2);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(DraftPartErrors.SubDraftIndexAlreadyExists(0));
  }

  [Fact]
  public void AddSubDraft_MultipleDistinctIndices_ShouldSucceed()
  {
    // Arrange
    var draftPart = CreateSpeedDraftPart();

    // Act
    draftPart.AddSubDraft(0, $"sd_{Faker.Random.AlphaNumeric(21)}");
    draftPart.AddSubDraft(1, $"sd_{Faker.Random.AlphaNumeric(21)}");
    var result = draftPart.AddSubDraft(2, $"sd_{Faker.Random.AlphaNumeric(21)}");

    // Assert
    result.IsSuccess.Should().BeTrue();
    draftPart.SubDrafts.Should().HaveCount(3);
  }

  // ========================================
  // DraftPart.StartingVetoesForSubDraft Tests
  // ========================================

  [Fact]
  public void StartingVetoesForSubDraft_ForFirstSubDraft_ShouldReturnOne()
  {
    // Arrange
    var draftPart = CreateSpeedDraftPart();
    draftPart.AddSubDraft(0, $"sd_{Faker.Random.AlphaNumeric(21)}");
    var vetoes = Array.Empty<(SubDraftId SubDraftId, bool IsOverridden)>();

    // Act
    var starting = draftPart.StartingVetoesForSubDraft(0, vetoes);

    // Assert
    starting.Should().Be(1);
  }

  [Fact]
  public void StartingVetoesForSubDraft_NonSpeedDraft_ShouldReturnZero()
  {
    // Arrange
    var draftPart = CreateDraftPart();
    var vetoes = Array.Empty<(SubDraftId SubDraftId, bool IsOverridden)>();

    // Act
    var starting = draftPart.StartingVetoesForSubDraft(1, vetoes);

    // Assert
    starting.Should().Be(0);
  }

  [Fact]
  public void StartingVetoesForSubDraft_WithUnusedVetoFromPrevious_ShouldCarryOverToNext()
  {
    // Arrange
    var draftPart = CreateSpeedDraftPart();
    draftPart.AddSubDraft(0, $"sd_{Faker.Random.AlphaNumeric(21)}");
    draftPart.AddSubDraft(1, $"sd_{Faker.Random.AlphaNumeric(21)}");

    // No vetoes used in sub-draft 0 → 1 unused veto should carry to sub-draft 1
    var vetoes = Array.Empty<(SubDraftId SubDraftId, bool IsOverridden)>();

    // Act
    var starting = draftPart.StartingVetoesForSubDraft(1, vetoes);

    // Assert
    // Sub-draft 0 had 1 starting veto, used 0 → remainder = 1
    // Sub-draft 1 starts with 1 (new) + 1 (carry) = 2
    starting.Should().Be(2);
  }

  [Fact]
  public void StartingVetoesForSubDraft_WithUsedVetoFromPrevious_ShouldNotCarryOver()
  {
    // Arrange
    var draftPart = CreateSpeedDraftPart();
    draftPart.AddSubDraft(0, $"sd_{Faker.Random.AlphaNumeric(21)}");
    draftPart.AddSubDraft(1, $"sd_{Faker.Random.AlphaNumeric(21)}");

    var firstSubDraft = draftPart.SubDrafts.First(s => s.Index == 0);
    // 1 veto used in sub-draft 0
    var vetoes = new[] { (SubDraftId: firstSubDraft.Id, IsOverridden: false) };

    // Act
    var starting = draftPart.StartingVetoesForSubDraft(1, vetoes);

    // Assert
    // Sub-draft 0 had 1 starting veto, used 1 → remainder = 0
    // Sub-draft 1 starts with 1 (new) + 0 (carry) = 1
    starting.Should().Be(1);
  }

  // ========================================
  // DraftPart.AdvanceSubDraft Tests
  // ========================================

  [Fact]
  public void AdvanceSubDraft_OnActiveSubDraft_ShouldCompleteItAndReturnZeroWhenNoNext()
  {
    // Arrange
    var draftPart = CreateSpeedDraftPart();
    draftPart.AddSubDraft(0, $"sd_{Faker.Random.AlphaNumeric(21)}");
    var subDraft = draftPart.SubDrafts.First();
    subDraft.Activate();
    var vetoes = Array.Empty<(SubDraftId SubDraftId, bool IsOverridden)>();

    // Act
    var result = draftPart.AdvanceSubDraft(subDraft.Id, vetoes);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Should().Be(0); // No next sub-draft
    subDraft.Status.Should().Be(SubDraftStatus.Completed);
  }

  [Fact]
  public void AdvanceSubDraft_WithNextSubDraft_ShouldReturnRemainder()
  {
    // Arrange
    var draftPart = CreateSpeedDraftPart();
    draftPart.AddSubDraft(0, $"sd_{Faker.Random.AlphaNumeric(21)}");
    draftPart.AddSubDraft(1, $"sd_{Faker.Random.AlphaNumeric(21)}");

    var firstSubDraft = draftPart.SubDrafts.First(s => s.Index == 0);
    firstSubDraft.Activate();
    // No vetoes used → 1 unused veto carries over
    var vetoes = Array.Empty<(SubDraftId SubDraftId, bool IsOverridden)>();

    // Act
    var result = draftPart.AdvanceSubDraft(firstSubDraft.Id, vetoes);

    // Assert
    result.IsSuccess.Should().BeTrue();
    firstSubDraft.Status.Should().Be(SubDraftStatus.Completed);
    // Next sub-draft starts with 1 + 1 (carry) = 2 starting vetoes, 0 used → remainder = 2
    result.Value.Should().Be(2);
  }

  [Fact]
  public void AdvanceSubDraft_OnNonSpeedDraftPart_ShouldFail()
  {
    // Arrange
    var draftPart = CreateDraftPart();
    var fakeSubDraftId = SubDraftId.CreateUnique();
    var vetoes = Array.Empty<(SubDraftId SubDraftId, bool IsOverridden)>();

    // Act
    var result = draftPart.AdvanceSubDraft(fakeSubDraftId, vetoes);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(SubDraftErrors.SubDraftsOnlyAllowedForSpeedDrafts);
  }

  [Fact]
  public void AdvanceSubDraft_WithSubDraftNotFound_ShouldFail()
  {
    // Arrange
    var draftPart = CreateSpeedDraftPart();
    var unknownId = SubDraftId.CreateUnique();
    var vetoes = Array.Empty<(SubDraftId SubDraftId, bool IsOverridden)>();

    // Act
    var result = draftPart.AdvanceSubDraft(unknownId, vetoes);

    // Assert
    result.IsFailure.Should().BeTrue();
  }

  [Fact]
  public void AdvanceSubDraft_WhenSubDraftNotActive_ShouldFail()
  {
    // Arrange
    var draftPart = CreateSpeedDraftPart();
    draftPart.AddSubDraft(0, $"sd_{Faker.Random.AlphaNumeric(21)}");
    var subDraft = draftPart.SubDrafts.First();
    // subDraft is still Pending (not activated)
    var vetoes = Array.Empty<(SubDraftId SubDraftId, bool IsOverridden)>();

    // Act
    var result = draftPart.AdvanceSubDraft(subDraft.Id, vetoes);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(SubDraftErrors.CannotCompleteSubDraft);
  }

  // ========================================
  // Helpers
  // ========================================

  private static DraftPart CreateSpeedDraftPart()
  {
    var draftId = DraftId.CreateUnique();
    var series = CreateSeries();
    var gameplay = DraftPartGamePlaySnapshot.Create(
      minPosition: 1,
      maxPosition: 7,
      draftType: DraftType.SpeedDraft,
      seriesId: series.Id).Value;

    return DraftPart.Create(
      draftId: draftId,
      draftPublicId: Faker.Random.AlphaNumeric(10),
      partIndex: 1,
      gameplay: gameplay,
      publicId: Faker.Random.AlphaNumeric(10)).Value;
  }
}
