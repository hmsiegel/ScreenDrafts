namespace ScreenDrafts.Modules.Drafts.UnitTests.DraftParts;

public class DraftPartTests : DraftsBaseTest
{
  // ========================================
  // Creation Tests
  // ========================================

  [Fact]
  public void Create_ShouldReturnSuccessResult_WhenValidParametersAreProvided()
  {
    // Arrange
    var draftId = DraftId.CreateUnique();
    var partIndex = 1;
    var gameplay = CreateGameplaySnapshot();

    // Act
    var result = DraftPart.Create(draftId, partIndex, gameplay);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.DraftId.Should().Be(draftId);
    result.Value.PartIndex.Should().Be(partIndex);
    result.Value.MinPosition.Should().Be(gameplay.MinPosition);
    result.Value.MaxPosition.Should().Be(gameplay.MaxPosition);
  }

  [Fact]
  public void Create_ShouldReturnFailure_WhenDraftIdIsNull()
  {
    // Arrange
    var partIndex = 1;
    var gameplay = CreateGameplaySnapshot();

    // Act
    var result = DraftPart.Create(null!, partIndex, gameplay);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(DraftErrors.DraftIsRequired);
  }

  [Fact]
  public void Create_ShouldReturnFailure_WhenPartIndexIsZeroOrLess()
  {
    // Arrange
    var draftId = DraftId.CreateUnique();
    var gameplay = CreateGameplaySnapshot();

    // Act
    var result = DraftPart.Create(draftId, 0, gameplay);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(DraftErrors.PartIndexMustBeGreaterThanZero);
  }

  // ========================================
  // Participant Management Tests
  // ========================================

  [Fact]
  public void AddParticipant_ShouldSucceed_WhenParticipantIsValid()
  {
    // Arrange
    var draftPart = CreateDraftPart();
    var drafter = CreateDrafter();
    var participantId = CreateParticipantId(drafter);

    // Act
    var result = draftPart.AddParticipant(participantId);

    // Assert
    result.IsSuccess.Should().BeTrue();
    draftPart.Participants.Should().Contain(participantId);
    draftPart.TotalParticipants.Should().Be(1);
  }

  [Fact]
  public void AddParticipant_ShouldIncrementDrafterCount_WhenDrafterIsAdded()
  {
    // Arrange
    var draftPart = CreateDraftPart();
    var drafter = CreateDrafter();
    var participantId = CreateParticipantId(drafter);

    // Act
    draftPart.AddParticipant(participantId);

    // Assert
    draftPart.TotalDrafters.Should().Be(1);
    draftPart.TotalDrafterTeams.Should().Be(0);
  }

  [Fact]
  public void AddParticipant_ShouldIncrementTeamCount_WhenTeamIsAdded()
  {
    // Arrange
    var draftPart = CreateDraftPart();
    var team = CreateDrafterTeam();
    var participantId = CreateParticipantId(team);

    // Act
    draftPart.AddParticipant(participantId);

    // Assert
    draftPart.TotalDrafterTeams.Should().Be(1);
    draftPart.TotalDrafters.Should().Be(0);
  }

  [Fact]
  public void AddParticipant_ShouldFail_WhenParticipantAlreadyAdded()
  {
    // Arrange
    var draftPart = CreateDraftPart();
    var drafter = CreateDrafter();
    var participantId = CreateParticipantId(drafter);
    draftPart.AddParticipant(participantId);

    // Act
    var result = draftPart.AddParticipant(participantId);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(DraftPartErrors.ParticipantAlreadyAdded(participantId.Value));
  }

  [Fact]
  public void RemoveParticipant_ShouldSucceed_WhenParticipantExists()
  {
    // Arrange
    var draftPart = CreateDraftPart();
    var drafter = CreateDrafter();
    var participantId = CreateParticipantId(drafter);
    draftPart.AddParticipant(participantId);

    // Act
    var result = draftPart.RemoveParticipant(participantId);

    // Assert
    result.IsSuccess.Should().BeTrue();
    draftPart.Participants.Should().NotContain(participantId);
    draftPart.TotalParticipants.Should().Be(0);
  }

  [Fact]
  public void RemoveParticipant_ShouldFail_WhenParticipantDoesNotExist()
  {
    // Arrange
    var draftPart = CreateDraftPart();
    var drafter = CreateDrafter();
    var participantId = CreateParticipantId(drafter);

    // Act
    var result = draftPart.RemoveParticipant(participantId);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(DraftPartErrors.ParticipantNotFound(participantId.Value));
  }

  [Fact]
  public void HasParticipant_ShouldReturnTrue_WhenParticipantExists()
  {
    // Arrange
    var draftPart = CreateDraftPart();
    var drafter = CreateDrafter();
    var participantId = CreateParticipantId(drafter);
    draftPart.AddParticipant(participantId);

    // Act
    var hasParticipant = draftPart.HasParticipant(participantId);

    // Assert
    hasParticipant.Should().BeTrue();
  }

  [Fact]
  public void HasParticipant_ShouldReturnFalse_WhenParticipantDoesNotExist()
  {
    // Arrange
    var draftPart = CreateDraftPart();
    var drafter = CreateDrafter();
    var participantId = CreateParticipantId(drafter);

    // Act
    var hasParticipant = draftPart.HasParticipant(participantId);

    // Assert
    hasParticipant.Should().BeFalse();
  }

  [Fact]
  public void SetParticipants_ShouldReplaceAllParticipants_WithNewList()
  {
    // Arrange
    var draftPart = CreateDraftPart();
    var drafter1 = CreateDrafter();
    var drafter2 = CreateDrafter();
    var participantId1 = CreateParticipantId(drafter1);
    var participantId2 = CreateParticipantId(drafter2);
    
    draftPart.AddParticipant(participantId1);

    var newParticipants = new List<ParticipantId> { participantId2 };

    // Act
    var result = draftPart.SetParticipants(newParticipants);

    // Assert
    result.IsSuccess.Should().BeTrue();
    draftPart.Participants.Should().Contain(participantId2);
    draftPart.Participants.Should().NotContain(participantId1);
    draftPart.TotalParticipants.Should().Be(1);
  }

  [Fact]
  public void SetParticipants_ShouldClearAllParticipants_WhenEmptyListProvided()
  {
    // Arrange
    var draftPart = CreateDraftPart();
    var drafter = CreateDrafter();
    var participantId = CreateParticipantId(drafter);
    draftPart.AddParticipant(participantId);

    var emptyList = new List<ParticipantId>();

    // Act
    var result = draftPart.SetParticipants(emptyList);

    // Assert
    result.IsSuccess.Should().BeTrue();
    draftPart.Participants.Should().BeEmpty();
    draftPart.TotalParticipants.Should().Be(0);
  }

  [Fact]
  public void TotalParticipants_ShouldReflectMultipleTypes()
  {
    // Arrange
    var draftPart = CreateDraftPart();
    var drafter1 = CreateDrafter();
    var drafter2 = CreateDrafter();
    var team = CreateDrafterTeam();
    
    var participantId1 = CreateParticipantId(drafter1);
    var participantId2 = CreateParticipantId(drafter2);
    var teamId = CreateParticipantId(team);

    // Act
    draftPart.AddParticipant(participantId1);
    draftPart.AddParticipant(participantId2);
    draftPart.AddParticipant(teamId);

    // Assert
    draftPart.TotalParticipants.Should().Be(3);
    draftPart.TotalDrafters.Should().Be(2);
    draftPart.TotalDrafterTeams.Should().Be(1);
  }

  // ========================================
  // Release Management Tests
  // ========================================

  [Fact]
  public void AddRelease_ShouldAddReleaseToList()
  {
    // Arrange
    var draftPart = CreateDraftPart();
    var channel = ReleaseChannel.MainFeed;
    var date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30));

    // Act
    var release = draftPart.AddRelease(channel, date);

    // Assert
    release.Should().NotBeNull();
    draftPart.Releases.Should().ContainSingle();
    draftPart.Releases.First().ReleaseChannel.Should().Be(channel);
    draftPart.Releases.First().ReleaseDate.Should().Be(date);
  }

  // ========================================
  // GameBoard Tests
  // ========================================

  [Fact]
  public void SetGameBoard_ShouldAssignGameBoard()
  {
    // Arrange
    var draft = DraftFactory.CreateStandardDraft();
    draft.AddPart(1, 1, 7);
    var draftPart = draft.Parts.First();
    var gameBoard = GameBoard.Create(draftPart).Value;

    // Act
    draftPart.SetGameBoard(gameBoard);

    // Assert
    draftPart.GameBoard.Should().Be(gameBoard);
  }

  // ========================================
  // Position Management Tests
  // ========================================

  [Fact]
  public void SetPartPositions_ShouldSetMinAndMaxPositions()
  {
    // Arrange
    var draftPart = CreateDraftPart();
    var minPosition = 1;
    var maxPosition = 10;

    // Act
    draftPart.SetPartPositions(minPosition, maxPosition);

    // Assert
    draftPart.MinPosition.Should().Be(minPosition);
    draftPart.MaxPosition.Should().Be(maxPosition);
  }

  // ========================================
  // Status Management Tests
  // ========================================

  [Fact]
  public void SetDraftStatus_ShouldUpdateStatus()
  {
    // Arrange
    var draftPart = CreateDraftPart();
    var newStatus = DraftPartStatus.InProgress;

    // Act
    draftPart.SetDraftStatus(newStatus);

    // Assert
    draftPart.Status.Should().Be(newStatus);
  }

  // ========================================
  // Scheduling Tests
  // ========================================

  [Fact]
  public void IsScheduled_ShouldReturnFalse_WhenScheduledForUtcIsInThePast()
  {
    // Arrange
    var draftPart = CreateDraftPart();
    var utcNow = DateTime.UtcNow.AddDays(1);

    // Act
    var isScheduled = draftPart.IsScheduled(utcNow);

    // Assert
    isScheduled.Should().BeFalse();
  }

  [Fact]
  public void IsScheduled_ShouldReturnFalse_WhenPartIsNotScheduled()
  {
    // Arrange
    var draftPart = CreateDraftPart();
    var utcNow = DateTime.UtcNow;

    // Act
    var isScheduled = draftPart.IsScheduled(utcNow);

    // Assert
    isScheduled.Should().BeFalse();
  }

  // ========================================
  // Lifecycle Tests
  // ========================================

  [Fact]
  public void GetLifecycleView_ShouldReturnCreated_WhenStatusIsCreated()
  {
    // Arrange
    var draftPart = CreateDraftPart();
    var utcNow = DateTime.UtcNow;

    // Act
    var view = draftPart.GetLifecycleView(utcNow);

    // Assert
    view.Should().Be(DraftPartLifecycleView.Created);
  }

  [Fact]
  public void GetLifecycleView_ShouldReturnInProgress_WhenStatusIsInProgress()
  {
    // Arrange
    var draftPart = CreateDraftPart();
    draftPart.SetDraftStatus(DraftPartStatus.InProgress);
    var utcNow = DateTime.UtcNow;

    // Act
    var view = draftPart.GetLifecycleView(utcNow);

    // Assert
    view.Should().Be(DraftPartLifecycleView.InProgress);
  }

  // ========================================
  // Pick Management Tests
  // ========================================

  [Fact]
  public void PlayPick_ShouldCreatePick_WhenValidParametersProvided()
  {
    // Arrange
    var draftPart = CreateDraftPart();
    var drafter = DrafterFactory.CreateDrafter();
    var participantId = CreateParticipantId(drafter);
    draftPart.AddParticipant(participantId);
    draftPart.SetDraftStatus(DraftPartStatus.InProgress);

    var movie = MovieFactory.CreateMovie().Value;
    var position = 1;
    var playOrder = 1;
    var policyProvider = CreateMockSeriesPolicyProvider();

    // Act
    var result = draftPart.PlayPick(
      policyProvider,
      draftPart.SeriesId,
      draftPart.DraftType,
      movie,
      position,
      playOrder,
      participantId);

    // Assert
    result.IsSuccess.Should().BeTrue();
    draftPart.Picks.Should().ContainSingle();
    var pick = draftPart.Picks.First();
    pick.Position.Should().Be(position);
    pick.PlayOrder.Should().Be(playOrder);
    pick.Movie.Should().Be(movie);
    pick.PlayedByParticipant.ParticipantId.Should().Be(participantId);
  }

  [Fact]
  public void PlayPick_ShouldReturnFailure_WhenDraftPartNotInProgress()
  {
    // Arrange
    var draftPart = CreateDraftPart();
    var drafter = DrafterFactory.CreateDrafter();
    var participantId = CreateParticipantId(drafter);
    draftPart.AddParticipant(participantId);
    // Not setting status to InProgress

    var movie = MovieFactory.CreateMovie().Value;
    var position = 1;
    var playOrder = 1;
    var policyProvider = CreateMockSeriesPolicyProvider();

    // Act
    var result = draftPart.PlayPick(
      policyProvider,
      draftPart.SeriesId,
      draftPart.DraftType,
      movie,
      position,
      playOrder,
      participantId);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(DraftPartErrors.DraftNotStarted);
  }

  // ========================================
  // Helper Methods
  // ========================================

  private static TestSeriesPolicyProvider CreateMockSeriesPolicyProvider()
  {
    return new TestSeriesPolicyProvider();
  }

  private sealed class TestSeriesPolicyProvider : ISeriesPolicyProvider
  {
    public ContinuityScope GetContinuityScope(SeriesId seriesId) => ContinuityScope.Global;

    public CanonicalPolicy GetCanonicalPolicy(SeriesId seriesId) => CanonicalPolicy.Always;

    public PartBudget GetPartBudget(SeriesId seriesId, DraftType draftType, int partNumber, int totalParticipants)
    {
      return new PartBudget(
        MaxVetoes: 2,
        MaxVetoOverrides: 2,
        MaxCommunityPicks: 1);
    }
  }

  private static DraftPart CreateDraftPart()
  {
    var draftId = DraftId.CreateUnique();
    var partIndex = 1;
    var gameplay = CreateGameplaySnapshot();

    return DraftPart.Create(draftId, partIndex, gameplay).Value;
  }

  private static DraftPartGamePlaySnapshot CreateGameplaySnapshot()
  {
    var series = CreateSeries();
    var result = DraftPartGamePlaySnapshot.Create(
      minPosition: 1,
      maxPosition: 7,
      draftType: DraftType.Standard,
      seriesId: series.Id);

    return result.Value;
  }
}

