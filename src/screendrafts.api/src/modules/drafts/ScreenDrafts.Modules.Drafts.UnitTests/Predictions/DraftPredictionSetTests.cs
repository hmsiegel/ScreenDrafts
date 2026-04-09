namespace ScreenDrafts.Modules.Drafts.UnitTests.Predictions;

public sealed class DraftPredictionSetTests : DraftsBaseTest
{
  // ========================================
  // Creation Tests
  // ========================================

  [Fact]
  public void Create_ShouldReturnSuccess_WithCorrectProperties()
  {
    // Arrange
    var draftPart = CreateDraftPart();
    var season = PredictionFactory.CreateSeason();
    var contestant = PredictionFactory.CreateContestant();
    var publicId = Faker.Random.AlphaNumeric(10);

    // Act
    var result = DraftPredictionSet.Create(
      publicId: publicId,
      season: season,
      draftPart: draftPart,
      contestant: contestant,
      submittedByPerson: null,
      sourceKind: PredictionSourceKind.UI);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.PublicId.Should().Be(publicId);
    result.Value.Season.Should().Be(season);
    result.Value.DraftPart.Should().Be(draftPart);
    result.Value.Contestant.Should().Be(contestant);
    result.Value.IsLocked.Should().BeFalse();
    result.Value.Entries.Should().BeEmpty();
    result.Value.SourceKind.Should().Be(PredictionSourceKind.UI);
  }

  [Fact]
  public void Create_ShouldRaisePredictionSetSubmittedDomainEvent()
  {
    // Arrange
    var draftPart = CreateDraftPart();
    var season = PredictionFactory.CreateSeason();
    var contestant = PredictionFactory.CreateContestant();

    // Act
    var set = DraftPredictionSet.Create(
      publicId: Faker.Random.AlphaNumeric(10),
      season: season,
      draftPart: draftPart,
      contestant: contestant,
      submittedByPerson: null,
      sourceKind: PredictionSourceKind.UI).Value;

    // Assert
    var domainEvent = AssertDomainEventWasPublished<PredictionSetSubmittedDomainEvent>(set);
    domainEvent.SetId.Should().Be(set.Id);
    domainEvent.ContestantId.Should().Be(set.ContestantId);
    domainEvent.DraftPartId.Should().Be(set.DraftPartId);
    domainEvent.SeasonId.Should().Be(set.SeasonId);
  }

  [Fact]
  public void Create_ShouldThrow_WhenSeasonIsNull()
  {
    // Arrange
    var draftPart = CreateDraftPart();
    var contestant = PredictionFactory.CreateContestant();

    // Act
    Action act = () => DraftPredictionSet.Create(
      publicId: Faker.Random.AlphaNumeric(10),
      season: null!,
      draftPart: draftPart,
      contestant: contestant,
      submittedByPerson: null,
      sourceKind: PredictionSourceKind.UI);

    // Assert
    act.Should().Throw<ArgumentNullException>();
  }

  // ========================================
  // ReplaceEntries Tests
  // ========================================

  [Fact]
  public void ReplaceEntries_ShouldSucceed_WhenSetIsUnlocked()
  {
    // Arrange
    var draftPart = CreateDraftPart();
    var season = PredictionFactory.CreateSeason();
    var contestant = PredictionFactory.CreateContestant();
    var set = PredictionFactory.CreateSet(draftPart, season, contestant);

    var entries = new[]
    {
      PredictionEntry.Create(set, "m_001", "Movie 1"),
      PredictionEntry.Create(set, "m_002", "Movie 2"),
    };

    // Act
    var result = set.ReplaceEntries(entries);

    // Assert
    result.IsSuccess.Should().BeTrue();
    set.Entries.Should().HaveCount(2);
  }

  [Fact]
  public void ReplaceEntries_ShouldFail_WhenSetIsLocked()
  {
    // Arrange
    var draftPart = CreateDraftPart();
    var season = PredictionFactory.CreateSeason();
    var contestant = PredictionFactory.CreateContestant();
    var rules = PredictionFactory.CreateUnorderedAllRule(draftPart, requiredCount: 2);
    var set = PredictionFactory.CreateSet(draftPart, season, contestant);

    set.Lock(rules.ToSnapshot(), DateTime.UtcNow);

    var newEntries = new[] { PredictionEntry.Create(set, "m_003", "Movie 3") };

    // Act
    var result = set.ReplaceEntries(newEntries);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().Contain(PredictionErrors.SetAlreadyLocked);
  }

  // ========================================
  // Lock Tests
  // ========================================

  [Fact]
  public void Lock_ShouldSucceed_WhenSetIsUnlocked()
  {
    // Arrange
    var draftPart = CreateDraftPart();
    var season = PredictionFactory.CreateSeason();
    var contestant = PredictionFactory.CreateContestant();
    var rules = PredictionFactory.CreateUnorderedAllRule(draftPart);
    var set = PredictionFactory.CreateSet(draftPart, season, contestant);
    var now = DateTime.UtcNow;

    // Act
    var result = set.Lock(rules.ToSnapshot(), now);

    // Assert
    result.IsSuccess.Should().BeTrue();
    set.IsLocked.Should().BeTrue();
    set.LockedAtUtc.Should().Be(now);
    set.RulesSnapshot.Should().NotBeNull();
    set.RulesSnapshot!.Mode.Should().Be(PredictionMode.UnorderedAll);
  }

  [Fact]
  public void Lock_ShouldFail_WhenSetIsAlreadyLocked()
  {
    // Arrange
    var draftPart = CreateDraftPart();
    var season = PredictionFactory.CreateSeason();
    var contestant = PredictionFactory.CreateContestant();
    var rules = PredictionFactory.CreateUnorderedAllRule(draftPart);
    var set = PredictionFactory.CreateSet(draftPart, season, contestant);

    set.Lock(rules.ToSnapshot(), DateTime.UtcNow);

    // Act
    var result = set.Lock(rules.ToSnapshot(), DateTime.UtcNow);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().Contain(PredictionErrors.SetAlreadyLocked);
  }

  [Fact]
  public void Lock_ShouldRaisePredictionSetLockedDomainEvent()
  {
    // Arrange
    var draftPart = CreateDraftPart();
    var season = PredictionFactory.CreateSeason();
    var contestant = PredictionFactory.CreateContestant();
    var rules = PredictionFactory.CreateUnorderedAllRule(draftPart);
    var set = PredictionFactory.CreateSet(draftPart, season, contestant);
    var now = DateTime.UtcNow;

    // Act
    set.Lock(rules.ToSnapshot(), now);

    // Assert
    var domainEvent = AssertDomainEventWasPublished<PredictionSetLockedDomainEvent>(set);
    domainEvent.SetId.Should().Be(set.Id);
    domainEvent.ContestantId.Should().Be(set.ContestantId);
    domainEvent.DraftPartId.Should().Be(set.DraftPartId);
    domainEvent.LockedAtUtc.Should().Be(now);
  }

  // ========================================
  // AttachSurrogate Tests
  // ========================================

  [Fact]
  public void AttachSurrogate_ShouldSucceed_WhenPrimarySetMatchesSurrogate()
  {
    // Arrange
    var draftPart = CreateDraftPart();
    var season = PredictionFactory.CreateSeason();
    var primaryContestant = PredictionFactory.CreateContestant();
    var surrogateContestant = PredictionFactory.CreateContestant();
    var primarySet = PredictionFactory.CreateSet(draftPart, season, primaryContestant);
    var surrogateSet = PredictionFactory.CreateSet(draftPart, season, surrogateContestant);

    var assignment = SurrogateAssignment.Create(
      primarySet: primarySet,
      surrogateSet: surrogateSet,
      mergePolicy: MergePolicy.UseHigherScore);

    // Act
    var result = primarySet.AttachSurrogate(assignment);

    // Assert
    result.IsSuccess.Should().BeTrue();
    primarySet.Surrogates.Should().HaveCount(1);
  }

  [Fact]
  public void AttachSurrogate_ShouldFail_WhenPrimarySetDoesNotMatch()
  {
    // Arrange
    var draftPart = CreateDraftPart();
    var season = PredictionFactory.CreateSeason();
    var contestant1 = PredictionFactory.CreateContestant();
    var contestant2 = PredictionFactory.CreateContestant();
    var contestant3 = PredictionFactory.CreateContestant();
    var set1 = PredictionFactory.CreateSet(draftPart, season, contestant1);
    var set2 = PredictionFactory.CreateSet(draftPart, season, contestant2);
    var set3 = PredictionFactory.CreateSet(draftPart, season, contestant3);

    // Assignment says set1 is primary, but we try to attach to set2
    var assignment = SurrogateAssignment.Create(
      primarySet: set1,
      surrogateSet: set3,
      mergePolicy: MergePolicy.UseHigherScore);

    // Act
    var result = set2.AttachSurrogate(assignment);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().Contain(PredictionErrors.SurrogatePrimarySetMismatch);
  }

  [Fact]
  public void AttachSurrogate_ShouldThrow_WhenSurrogateIsNull()
  {
    // Arrange
    var draftPart = CreateDraftPart();
    var season = PredictionFactory.CreateSeason();
    var contestant = PredictionFactory.CreateContestant();
    var set = PredictionFactory.CreateSet(draftPart, season, contestant);

    // Act
    Action act = () => set.AttachSurrogate(null!);

    // Assert
    act.Should().Throw<ArgumentNullException>();
  }
}
