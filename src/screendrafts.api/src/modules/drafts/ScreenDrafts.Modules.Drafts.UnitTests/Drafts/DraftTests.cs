namespace ScreenDrafts.Modules.Drafts.UnitTests.Drafts;

public class DraftTests : BaseTest
{
  [Fact]
  public void Create_ShouldReturnSuccessResult_WhenValidParametersAreProvided()
  {
    // Arrange
    var title = Title.Create("Test Draft");
    var draftType = DraftType.Standard;
    var totalPicks = 7;
    var totalDrafters = 2;
    var totalDrafterTeams = 0;
    var totalHosts = 2;
    var draftStatus = DraftStatus.Created;
    var episodeType = EpisodeType.MainFeed;

    // Act
    var result = Draft.Create(
      title,
      draftType,
      totalPicks,
      totalDrafters,
      totalDrafterTeams,
      totalHosts,
      draftStatus,
      episodeType);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Title.Should().Be(title);
    result.Value.DraftType.Should().Be(draftType);
    result.Value.TotalPicks.Should().Be(totalPicks);
    result.Value.TotalDrafters.Should().Be(totalDrafters);
    result.Value.TotalDrafterTeams.Should().Be(totalDrafterTeams);
    result.Value.TotalHosts.Should().Be(totalHosts);
    result.Value.DraftStatus.Should().Be(draftStatus);
    result.Value.EpisodeType.Should().Be(episodeType);
  }

  [Fact]
  public void Create_ShouldRaiseDomainEvent_WhenDraftIsCreated()
  {
    var draft = SetupAndStartDraft();

    var domainEvent = AssertDomainEventWasPublished<DraftCreatedDomainEvent>(draft);

    domainEvent.DraftId.Should().Be(draft.Id.Value);
  }

  [Fact]
  public void Create_ShouldReturnFailureResult_WhenTotalDraftersIsLessThanTwo()
  {
    // Arrange
    var title = new Title("Test Title");
    var draftType = DraftType.Standard;
    var totalPicks = 5;
    var totalDrafters = 1;
    var totalDrafterTeams = 0;
    var totalHosts = 2;
    var draftStatus = DraftStatus.Created;
    var episodeType = EpisodeType.MainFeed;

    // Act
    var result = Draft.Create(
      title,
      draftType,
      totalPicks,
      totalDrafters,
      totalDrafterTeams,
      totalHosts,
      draftStatus,
      episodeType);

    // Assert
    result.IsSuccess.Should().BeFalse();
    result.Errors[0].Should().Be(DraftErrors.DraftMustHaveAtLeastTwoParticipants);
  }

  [Fact]
  public void Create_ShouldReturnFailureResult_WhenTotalPicksIsLessThanFour()
  {
    // Arrange
    var title = new Title("Test Title");
    var draftType = DraftType.Standard;
    var totalPicks = 3;
    var totalDrafters = 3;
    var totalDrafterTeams = 0;
    var totalHosts = 2;
    var draftStatus = DraftStatus.Created;
    var episodeType = EpisodeType.MainFeed;

    // Act
    var result = Draft.Create(
      title,
      draftType,
      totalPicks,
      totalDrafters,
      totalDrafterTeams,
      totalHosts,
      draftStatus,
      episodeType);

    // Assert
    result.IsSuccess.Should().BeFalse();
    result.Errors[0].Should().Be(DraftErrors.DraftMustHaveAtLeastFivePicks);
  }

  [Fact]
  public void AddDrafter_ShouldAddDrafterToList_WhenDrafterIsValid()
  {
    // Arrange
    var draft = DraftFactory.CreateStandardDraft().Value;
    var drafter = DrafterFactory.CreateDrafter();

    // Act
    var result = draft.AddDrafter(drafter);

    // Assert
    result.IsSuccess.Should().BeTrue();
    draft.Drafters.Should().ContainSingle();
    draft.Drafters.First().Should().Be(drafter);
  }

  [Fact]
  public void AddDrafter_ShouldRaiseDomainEvent_WhenDrafterIsAdded()
  {
    // Arrange
    var draft = DraftFactory.CreateStandardDraft().Value;
    var drafter = DrafterFactory.CreateDrafter();

    // Act
    draft.AddDrafter(drafter);

    var domainEvent = AssertDomainEventWasPublished<DrafterAddedDomainEvent>(draft);

    // Assert
    domainEvent.DraftId.Should().Be(draft.Id.Value);
    domainEvent.DrafterId.Should().Be(drafter.Id.Value);
  }

  [Fact]
  public void AddDrafter_ShouldReturnFailureResult_WhenDrafterIsNull()
  {
    // Arrange
    var draft = DraftFactory.CreateStandardDraft().Value;

    // Act
    Action act = () => draft.AddDrafter(null!);

    // Assert
    act.Should().Throw<ArgumentNullException>();
  }

  [Fact]
  public void AddDrafter_ShouldReturnFailureResult_WhenTooManyDrafters()
  {
    // Arrange
    var draft = DraftFactory.CreateStandardDraft().Value;
    for (int i = 0; i < draft.TotalDrafters; i++)
    {
      draft.AddDrafter(DrafterFactory.CreateDrafter());
    }
    var extraDrafter = DrafterFactory.CreateDrafter();

    // Act
    var result = draft.AddDrafter(extraDrafter);

    // Assert
    result.IsSuccess.Should().BeFalse();
    result.Errors[0].Should().Be(DraftErrors.TooManyDrafters);
  }

  [Fact]
  public void AddDrafter_ShouldReturnFailureResult_WhenDrafterAlreadyExists()
  {
    // Arrange
    var draft = DraftFactory.CreateStandardDraft().Value;
    var drafter = DrafterFactory.CreateDrafter();
    draft.AddDrafter(drafter);

    // Act
    var result = draft.AddDrafter(drafter);

    // Assert
    result.IsFailure.Should().BeTrue();
    draft.Drafters.Should().ContainSingle();
  }

  [Fact]
  public void AddDrafterTeam_ShouldAddDrafterTeamToList_WhenTeamIsValid()
  {
    // Arrange
    var draft = DraftFactory.CreateStandardDraftWithTeams().Value;
    var drafterTeam = DrafterFactory.CreateDrafterTeam();

    // Act
    var result = draft.AddDrafterTeam(drafterTeam);

    // Assert
    result.IsSuccess.Should().BeTrue();
    draft.DrafterTeams.Should().ContainSingle();
    draft.DrafterTeams.First().Should().Be(drafterTeam);
  }

  [Fact]
  public void AddPick_ShouldReturnSuccessResult_WhenPickIsValid()
  {
    // Arrange
    var draft = SetupAndStartDraft();

    var position = 1;
    var playOrder = 1;
    var movie = MovieFactory.CreateMovie().Value;

    var drafter = draft.Drafters.FirstOrDefault();

    var pick = Pick.Create(position, movie, drafter!, null, draft, playOrder).Value;

    // Act
    var result = draft.AddPick(pick);

    // Assert
    result.IsSuccess.Should().BeTrue();
    draft.Picks.Should().Contain(p => p.Position == position && p.Movie == movie && p.Drafter == drafter);
  }

  [Fact]
  public void AddPick_ShouldRaiseDomainEvent_WhenPickIsAdded()
  {
    // Arrange
    var draft = SetupAndStartDraft();

    var position = 1;
    var playOrder = 1;
    var movie = MovieFactory.CreateMovie().Value;

    var drafter = draft.Drafters.FirstOrDefault();

    var pick = Pick.Create(position, movie, drafter!, null, draft, playOrder).Value;

    // Act
    draft.AddPick(pick);

    var domainEvent = AssertDomainEventWasPublished<PickAddedDomainEvent>(draft);

    var firstPick = draft.Picks.FirstOrDefault(p => p.Position == position)!;

    // Assert
    domainEvent.DrafterId.Should().Be(firstPick.Drafter!.Id.Value);
    domainEvent.DraftId.Should().Be(firstPick.Draft.Id.Value);
  }

  [Fact]
  public void AddPick_ShouldReturnFailureResult_WhenPickPositionAlreadyTaken()
  {
    // Arrange
    var draft = SetupAndStartDraft();

    var movie = MovieFactory.CreateMovie().Value;

    var drafter = draft.Drafters.FirstOrDefault();

    var position = 1;
    var playOrder = 1;
    draft.AddDrafter(drafter!);

    var pick = Pick.Create(position, movie, drafter!, null, draft, playOrder).Value;

    draft.AddPick(pick);

    // Act
    var result = draft.AddPick(pick);

    // Assert
    result.IsSuccess.Should().BeFalse();
    result.Errors[0].Should().Be(DraftErrors.PickPositionAlreadyTaken(position));
  }

  [Fact]
  public void SetPrimaryHost_ShouldReturnSuccessResult_WhenHostIsValid()
  {
    // Arrange
    var draft = DraftFactory.CreateStandardDraft().Value;
    var host = HostsFactory.CreateHost().Value;

    // Act
    var result = draft.SetPrimaryHost(host);

    // Assert
    result.IsSuccess.Should().BeTrue();
    draft.PrimaryHost!.HostId.Value.Should().Be(host.Id.Value);
  }

  [Fact]
  public void SetPrimaryHost_ShouldRaiseDomainEvent_WhenHostIsAdded()
  {
    // Arrange
    var draft = DraftFactory.CreateStandardDraft().Value;
    var host = HostsFactory.CreateHost().Value;

    // Act
    draft.SetPrimaryHost(host);

    var domainEvent = AssertDomainEventWasPublished<HostAddedDomainEvent>(draft);

    // Assert
    domainEvent.DraftId.Should().Be(draft.Id.Value);
    domainEvent.HostId.Should().Be(host.Id.Value);
  }

  [Fact]
  public void SetPrimaryHost_ShouldReturnFailureResult_WhenHostIsNull()
  {
    // Arrange
    var draft = DraftFactory.CreateStandardDraft().Value;

    // Act
    Action act = () => draft.SetPrimaryHost(null!);

    // Assert
    act.Should().Throw<ArgumentNullException>();
  }

  [Fact]
  public void SetPrimaryHost_ShouldReturnFailureResult_WhenPrimaryAlreadySet()
  {
    // Arrange
    var draft = DraftFactory.CreateStandardDraft().Value;
    var primaryHost = HostsFactory.CreateHost().Value;
    var extraHost = HostsFactory.CreateHost().Value;
    draft.SetPrimaryHost(primaryHost);

    // Act
    var result = draft.SetPrimaryHost(extraHost);

    // Assert
    result.IsSuccess.Should().BeFalse();
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(DraftErrors.PrimaryHostAlreadySet(primaryHost.Id.Value));
  }

  [Fact]
  public void AddCoHost_ShouldSucceed_WhenValidCoHostProvided()
  {
    // Arrange
    var draft = DraftFactory.CreateStandardDraft().Value;
    var primary = HostsFactory.CreateHost().Value;
    var co1 = HostsFactory.CreateHost().Value;
    draft.SetPrimaryHost(primary);

    // Act
    var r1 = draft.AddCoHost(co1);

    // Assert
    r1.IsSuccess.Should().BeTrue();
    draft.CoHosts.Select(h => h.Host).Should().BeEquivalentTo([co1]);
  }

  [Fact]
  public void AddCoHost_ShouldFail_WhenSameHostAlreadyOnDraft()
  {
    // Arrange
    var draft = DraftFactory.CreateStandardDraft().Value;
    var primary = HostsFactory.CreateHost().Value;
    var co = HostsFactory.CreateHost().Value;
    draft.SetPrimaryHost(primary);
    draft.AddCoHost(co);

    // Act
    var result = draft.AddCoHost(co);

    // Assert
    result.IsFailure.Should().BeTrue();
  }

  [Fact]
  public void StartDraft_ShouldChangeStatusToInProgress_WhenDraftIsNotStarted()
  {
    // Arrange
    var draft = DraftFactory.CreateStandardDraft().Value;
    for (int i = 0; i < draft.TotalDrafters; i++)
    {
      draft.AddDrafter(DrafterFactory.CreateDrafter());
    }

    var primaryHost = HostsFactory.CreateHost().Value;
    draft.SetPrimaryHost(primaryHost);

    for (int i = 0; i < draft.TotalHosts - 1; i++)
    {
      draft.AddCoHost(HostsFactory.CreateHost().Value);
    }

    // Act
    var result = draft.StartDraft();

    // Assert
    result.IsSuccess.Should().BeTrue();
    draft.DraftStatus.Should().Be(DraftStatus.InProgress);
  }

  [Fact]
  public void StartDraft_ShouldReturnFailure_WhenDraftIsAlreadyInProgress()
  {
    // Arrange
    var draft = DraftFactory.CreateStandardDraft().Value;
    for (int i = 0; i < draft.TotalDrafters; i++)
    {
      draft.AddDrafter(DrafterFactory.CreateDrafter());
    }

    var primaryHost = HostsFactory.CreateHost().Value;
    draft.SetPrimaryHost(primaryHost);

    for (int i = 0; i < draft.TotalHosts - 1; i++)
    {
      draft.AddCoHost(HostsFactory.CreateHost().Value);
    }
    draft.StartDraft();

    // Act
    var result = draft.StartDraft();

    // Assert
    result.IsFailure.Should().BeTrue();
    draft.DraftStatus.Should().Be(DraftStatus.InProgress);
  }

  [Fact]
  public void CompleteDraft_ShouldChangeStatusToCompleted_WhenDraftIsInProgress()
  {
    // Arrange
    var draft = SetupAndStartDraft();
    for (int i = 0; i < draft.TotalPicks; i++)
    {
      var pick = Pick.Create(
        i + 1,
        MovieFactory.CreateMovie().Value,
        DrafterFactory.CreateDrafter(),
        null,
        draft,
        i + 1).Value;

      draft.AddPick(pick);
    }

    // Act
    var result = draft.CompleteDraft();

    // Assert
    result.IsSuccess.Should().BeTrue();
    draft.DraftStatus.Should().Be(DraftStatus.Completed);
  }

  [Fact]
  public void CompleteDraft_ShouldReturnFailure_WhenDraftIsNotInProgress()
  {
    // Arrange
    var draft = DraftFactory.CreateStandardDraft().Value;
    for (int i = 0; i < draft.TotalDrafters; i++)
    {
      draft.AddDrafter(DrafterFactory.CreateDrafter());
    }
    var primaryHost = HostsFactory.CreateHost().Value;
    draft.SetPrimaryHost(primaryHost);

    for (int i = 0; i < draft.TotalHosts - 1; i++)
    {
      draft.AddCoHost(HostsFactory.CreateHost().Value);
    }

    // Act
    var result = draft.CompleteDraft();

    // Assert
    result.IsFailure.Should().BeTrue();
    draft.DraftStatus.Should().Be(DraftStatus.Created);
  }

  [Fact]
  public void AddTriviaResult_ShouldAddTriviaResultToList_WhenValidParametersAreProvided()
  {
    // Arrange
    var draft = SetupAndStartDraft();
    var position = 1;
    var questionsWon = 5;

    var drafter = draft.Drafters.FirstOrDefault();

    // Act
    var result = draft.AddTriviaResult(drafter, null, position, questionsWon);

    // Assert
    result.IsSuccess.Should().BeTrue();
    draft.TriviaResults.Should().ContainSingle();
    draft.TriviaResults.First().Position.Should().Be(position);
    draft.TriviaResults.First().QuestionsWon.Should().Be(questionsWon);
  }

  [Fact]
  public void ApplyRollover_ShouldAddRolloverToDraft_WhenValidParametersAreProvided()
  {
    // Arrange
    var draft = DraftFactory.CreateStandardDraft().Value;
    var drafter = DrafterFactory.CreateDrafter();
    draft.AddDrafter(drafter);

    // Act
    var result = draft.ApplyRollover(drafter.Id.Value, null, false);

    // Assert
    result.IsSuccess.Should().BeTrue();
  }

  [Fact]
  public void ApplyCommissionerOverride_ShouldOverridePick_WhenValidPickIsProvided()
  {
    // Arrange
    var draft = DraftFactory.CreateStandardDraft().Value;
    var drafter = DrafterFactory.CreateDrafter();
    var movie = MovieFactory.CreateMovie().Value;
    var pick = Pick.Create(
      Faker.Random.Number(1, 7),
      movie,
      drafter,
      null,
      draft,
      Faker.Random.Number(1, 9)).Value;

    // Act
    var result = draft.ApplyCommissionerOverride(pick);

    // Assert
    result.IsSuccess.Should().BeTrue();
  }

  [Fact]
  public void SetEpisodeNumber_ShouldUpdateEpisodeNumber_WhenValidNumberIsProvided()
  {
    // Arrange
    var draft = DraftFactory.CreateStandardDraft().Value;
    var episodeNumber = Faker.Random.Number(1, 100);

    // Act
    draft.SetEpisodeNumber(episodeNumber);

    // Assert
    draft.EpisodeNumber.Should().Be(episodeNumber);
  }

  [Fact]
  public void PauseDraft_ShouldChangeStatusToPaused_WhenDraftIsInProgress()
  {
    // Arrange
    var draft = DraftFactory.CreateStandardDraft().Value;
    for (int i = 0; i < draft.TotalDrafters; i++)
    {
      draft.AddDrafter(DrafterFactory.CreateDrafter());
    }
    var primaryHost = HostsFactory.CreateHost().Value;
    draft.SetPrimaryHost(primaryHost);

    for (int i = 0; i < draft.TotalHosts - 1; i++)
    {
      draft.AddCoHost(HostsFactory.CreateHost().Value);
    }
    draft.StartDraft();

    // Act
    var result = draft.PauseDraft();

    // Assert
    result.IsSuccess.Should().BeTrue();
    draft.DraftStatus.Should().Be(DraftStatus.Paused);
  }

  [Fact]
  public void AddReleaseDate_ShouldAddReleaseDateToList_WhenValidDateIsProvided()
  {
    // Arrange
    var draft = DraftFactory.CreateStandardDraft().Value;
    var releaseDate = DraftReleaseDate.Create(draft.Id, Faker.Date.PastDateOnly());

    // Act
    draft.AddReleaseDate(releaseDate);

    // Assert
    draft.ReleaseDates.Should().ContainSingle();
    draft.ReleaseDates.First().Should().Be(releaseDate);
  }

  [Fact]
  public void SetDraftStatus_ShouldUpdateStatus_WhenValidStatusIsProvided()
  {
    // Arrange
    var draft = DraftFactory.CreateStandardDraft().Value;
    var newStatus = DraftStatus.InProgress;

    // Act
    draft.SetDraftStatus(newStatus);

    // Assert
    draft.DraftStatus.Should().Be(newStatus);
  }

  [Fact]
  public void SetGameBoard_ShouldUpdateGameBoard_WhenValidGameBoardIsProvided()
  {
    // Arrange
    var draft = DraftFactory.CreateStandardDraft().Value;
    var gameBoard = GameBoard.Create(draft).Value;

    // Act
    draft.SetGameBoard(gameBoard);

    // Assert
    draft.GameBoard.Should().Be(gameBoard);
  }

  [Fact]
  public void SetPatreonOnly_ShouldUpdatePatreonOnlyFlag_WhenValidValueIsProvided()
  {
    // Arrange
    var draft = DraftFactory.CreateStandardDraft().Value;
    var isPatreonOnly = true;

    // Act
    draft.SetPatreonOnly(isPatreonOnly);

    // Assert
    draft.IsPatreonOnly.Should().Be(isPatreonOnly);
  }

  [Fact]
  public void SetNonCanonical_ShouldUpdateNonCanonicalFlag_WhenValidValueIsProvided()
  {
    // Arrange
    var draft = DraftFactory.CreateStandardDraft().Value;
    var nonCanonical = true;

    // Act
    draft.SetNonCanonical(nonCanonical);

    // Assert
    draft.NonCanonical.Should().Be(nonCanonical);
  }

  [Fact]
  public void RemoveDrafter_ShouldRemoveDrafterFromList_WhenDrafterExists()
  {
    // Arrange
    var draft = DraftFactory.CreateStandardDraft().Value;
    var drafter = DrafterFactory.CreateDrafter();
    draft.AddDrafter(drafter);

    // Act
    var result = draft.RemoveDrafter(drafter);

    // Assert
    result.IsSuccess.Should().BeTrue();
    draft.Drafters.Should().BeEmpty();
  }

  [Fact]
  public void RemoveCoHost_ShouldRemoveCoHostFromList_WhenHostExists()
  {
    // Arrange
    var draft = DraftFactory.CreateStandardDraft().Value;
    var host = HostsFactory.CreateHost().Value;
    draft.AddCoHost(host);

    // Act
    var result = draft.RemoveHost(host);

    // Assert
    result.IsSuccess.Should().BeTrue();
    draft.CoHosts.Should().BeEmpty();
  }

  [Fact]
  public void StartDraft_ShouldFail_WhenNoPrimaryHost()
  {
    var draft = DraftFactory.CreateStandardDraft().Value;
    for (int i = 0; i < draft.TotalDrafters; i++)
    {
      draft.AddDrafter(DrafterFactory.CreateDrafter());
    }
    // Intentionally add only cohosts
    for (int i = 0; i < draft.TotalHosts; i++)
    {
      draft.AddCoHost(HostsFactory.CreateHost().Value);
    }

    var result = draft.StartDraft();

    result.IsFailure.Should().BeTrue();
  }

  [Fact]
  public void StartDraft_ShouldFail_WhenHostCountDoesNotMatchTotalHosts()
  {
    var draft = DraftFactory.CreateStandardDraft().Value;
    for (int i = 0; i < draft.TotalDrafters; i++)
    {
      draft.AddDrafter(DrafterFactory.CreateDrafter());
    }
    draft.SetPrimaryHost(HostsFactory.CreateHost().Value);
    // Add fewer cohosts than required
    // Required = TotalHosts - 1
    var requiredCoHosts = draft.TotalHosts - 1;
    for (int i = 0; i < requiredCoHosts - 1; i++)
    {
      draft.AddCoHost(HostsFactory.CreateHost().Value);
    }

    var result = draft.StartDraft();

    result.IsFailure.Should().BeTrue();
  }

  public static Draft SetupAndStartDraft()
  {
    var draft = DraftFactory.CreateStandardDraft().Value;
    for (int i = 0; i < draft.TotalDrafters; i++)
    {
      draft.AddDrafter(DrafterFactory.CreateDrafter());
    }

    var primaryHost = HostsFactory.CreateHost().Value;
    draft.SetPrimaryHost(primaryHost);

    for (int i = 0; i < draft.TotalHosts - 1; i++)
    {
      draft.AddCoHost(HostsFactory.CreateHost().Value);
    }

    draft.StartDraft();
    return draft;
  }
}

