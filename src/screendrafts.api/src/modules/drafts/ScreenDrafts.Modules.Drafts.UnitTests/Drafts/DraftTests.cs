﻿namespace ScreenDrafts.Modules.Drafts.UnitTests.Drafts;

public class DraftTests : BaseTest
{
  [Fact]
  public void Create_ShouldReturnSuccessResult_WhenParametersAreValid()
  {
    // Arrange
    var title = new Title("Test Title");
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
  public void AddDrafter_ShouldReturnSuccessResult_WhenDrafterIsValid()
  {
    // Arrange
    var draft = DraftFactory.CreateStandardDraft().Value;
    var drafter = DrafterFactory.CreateDrafter();

    // Act
    var result = draft.AddDrafter(drafter);

    // Assert
    result.IsSuccess.Should().BeTrue();
    draft.Drafters.Should().Contain(drafter);
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
  public void AddDrafter_ShouldReturnFailureResult_WhenDrafterAlreadyAdded()
  {
    // Arrange
    var draft = DraftFactory.CreateStandardDraft().Value;
    var drafter = DrafterFactory.CreateDrafter();
    draft.AddDrafter(drafter);

    // Act
    var result = draft.AddDrafter(drafter);

    // Assert
    result.IsSuccess.Should().BeFalse();
    result.Errors[0].Should().Be(DraftErrors.DrafterAlreadyAdded(drafter.Id.Value));
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
  public void AddPick_ShouldReturnFailureResult_WhenPickPositionIsOutOfRange()
  {
    // Arrange
    var draft = SetupAndStartDraft();

    var position = draft.TotalPicks + 1;
    var playOrder = 1;
    var movie = MovieFactory.CreateMovie().Value;

    var drafter = draft.Drafters.FirstOrDefault();

    var pick = Pick.Create(position, movie, drafter!, null, draft, playOrder).Value;

    // Act
    var result = draft.AddPick(pick);

    // Assert
    result.IsSuccess.Should().BeFalse();
    result.Errors[0].Should().Be(DraftErrors.PickPositionIsOutOfRange);
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
  public void AddHost_ShouldReturnSuccessResult_WhenHostIsValid()
  {
    // Arrange
    var draft = DraftFactory.CreateStandardDraft().Value;
    var host = HostsFactory.CreateHost().Value;

    // Act
    var result = draft.AddHost(host);

    // Assert
    result.IsSuccess.Should().BeTrue();
    draft.Hosts.Should().Contain(host);
  }

  [Fact]
  public void AddHost_ShouldRaiseDomainEvent_WhenHostIsAdded()
  {
    // Arrange
    var draft = DraftFactory.CreateStandardDraft().Value;
    var host = HostsFactory.CreateHost().Value;

    // Act
    draft.AddHost(host);

    var domainEvent = AssertDomainEventWasPublished<HostAddedDomainEvent>(draft);

    // Assert
    domainEvent.DraftId.Should().Be(draft.Id.Value);
    domainEvent.HostId.Should().Be(host.Id.Value);
  }

  [Fact]
  public void AddHost_ShouldReturnFailureResult_WhenHostIsNull()
  {
    // Arrange
    var draft = DraftFactory.CreateStandardDraft().Value;

    // Act
    Action act = () => draft.AddHost(null!);

    // Assert
    act.Should().Throw<ArgumentNullException>();
  }

  [Fact]
  public void AddHost_ShouldReturnFailureResult_WhenTooManyHosts()
  {
    // Arrange
    var draft = DraftFactory.CreateStandardDraft().Value;
    for (int i = 0; i < draft.TotalHosts; i++)
    {
      draft.AddHost(HostsFactory.CreateHost().Value);
    }
    var extraHost = HostsFactory.CreateHost().Value;

    // Act
    var result = draft.AddHost(extraHost);

    // Assert
    result.IsSuccess.Should().BeFalse();
    result.Errors[0].Should().Be(DraftErrors.TooManyHosts);
  }

  [Fact]
  public void AddHost_ShouldReturnFailureResult_WhenHostAlreadyAdded()
  {
    // Arrange
    var draft = DraftFactory.CreateStandardDraft().Value;
    var host = HostsFactory.CreateHost().Value;
    draft.AddHost(host);

    // Act
    var result = draft.AddHost(host);

    // Assert
    result.IsSuccess.Should().BeFalse();
    result.Errors[0].Should().Be(DraftErrors.HostAlreadyAdded(host.Id.Value));
  }

  [Fact]
  public void StartDraft_ShouldReturnSuccessResult_WhenDraftIsCreated()
  {
    // Arrange
    var draft = DraftFactory.CreateStandardDraft().Value;
    for (int i = 0; i < draft.TotalDrafters; i++)
    {
      draft.AddDrafter(DrafterFactory.CreateDrafter());
    }
    for (int i = 0; i < draft.TotalHosts; i++)
    {
      draft.AddHost(HostsFactory.CreateHost().Value);
    }

    // Act
    var result = draft.StartDraft();

    // Assert
    result.IsSuccess.Should().BeTrue();
    draft.DraftStatus.Should().Be(DraftStatus.InProgress);
  }

  [Fact]
  public void StartDraft_ShouldReturnFailureResult_WhenDraftIsNotCreated()
  {
    // Arrange
    var draft = DraftFactory.CreateStandardDraft().Value;
    for (int i = 0; i < draft.TotalDrafters; i++)
    {
      draft.AddDrafter(DrafterFactory.CreateDrafter());
    }

    for (int i = 0; i < draft.TotalHosts; i++)
    {
      draft.AddHost(HostsFactory.CreateHost().Value);
    }
    draft.StartDraft();
    draft.CompleteDraft();

    // Act
    var result = draft.StartDraft();

    // Assert
    result.IsSuccess.Should().BeFalse();
    result.Errors[0].Should().Be(DraftErrors.DraftCanOnlyBeStartedIfItIsCreated);
  }

  [Fact]
  public void CompleteDraft_ShouldReturnSuccessResult_WhenDraftIsInProgress()
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
  public void CompleteDraft_ShouldReturnFailureResult_WhenDraftIsNotInProgress()
  {
    // Arrange
    var draft = DraftFactory.CreateStandardDraft().Value;

    // Act
    var result = draft.CompleteDraft();

    // Assert
    result.IsSuccess.Should().BeFalse();
    result.Errors[0].Should().Be(DraftErrors.CannotCompleteDraftIfItIsNotInProgress);
  }

  [Fact]
  public void AddTriviaResult_ShouldAddTriviaResult_WhenParametersAreValid()
  {
    // Arrange
    var draft = DraftFactory.CreateStandardDraft().Value;
    List<Drafter> drafters = [];
    List<Host> hosts = [];

    for (int i = 0; i < draft.TotalDrafters; i++)
    {
      var drafter = DrafterFactory.CreateDrafter();
      drafters.Add(drafter);
      draft.AddDrafter(drafter);
    }

    for (int i = 0; i < draft.TotalHosts; i++)
    {
      var host = HostsFactory.CreateHost().Value;
      hosts.Add(host);
      draft.AddHost(host);
    }

    draft.StartDraft();
    var position = 1;
    var questionsWon = 3;

    var usedDrafter =
      drafters.FirstOrDefault()
      ?? throw new InvalidOperationException("Drafter is null");

    // Act
    draft.AddTriviaResult(usedDrafter, null, position, questionsWon);

    // Assert
    draft.TriviaResults.Should().Contain(tr => tr.Drafter == usedDrafter && tr.Position == position && tr.QuestionsWon == questionsWon);
  }

  [Fact]
  public void ApplyRollover_ShouldReturnSuccessResult_WhenRolloverIsValid()
  {
    // Arrange
    var draft = SetupAndStartDraft();

    var drafter = draft.Drafters.FirstOrDefault();

    // Act
    var result = draft.ApplyRollover(drafter!.Id.Value, null, true);

    // Assert
    result.IsSuccess.Should().BeTrue();
  }

  [Fact]
  public void SetEpisodeNumber_ShouldReturnSuccessResult_WhenEpisodeNumberIsValid()
  {
    // Arrange
    var draft = DraftFactory.CreateStandardDraft().Value;
    var episodeNumber = "S01E01";

    // Act
    draft.SetEpisodeNumber(episodeNumber);

    // Assert
    draft.EpisodeNumber.Should().Be(episodeNumber);
  }

  [Fact]
  public void PauseDraft_ShouldReturnSuccessResult_WhenDraftIsInProgress()
  {
    // Arrange
    var draft = SetupAndStartDraft();

    // Act
    var result = draft.PauseDraft();

    // Assert
    result.IsSuccess.Should().BeTrue();
    draft.DraftStatus.Should().Be(DraftStatus.Paused);
  }

  [Fact]
  public void AddDraftReleaseDate_ShouldReturnSuccessResult_WhenReleaseDateIsValid()
  {
    // Arrange
    var draft = DraftFactory.CreateStandardDraft().Value;
    var releaseDate = new DateOnly(
      Faker.Date.Past(1).Year,
      Faker.Date.Past(1).Month,
      Faker.Date.Past(1).Day);

    // Act
    draft.AddReleaseDate(DraftReleaseDate.Create(draft.Id, releaseDate));

    // Assert
    draft.ReleaseDates.Should().Contain(rd => rd.ReleaseDate == releaseDate);
  }

  [Fact]
  public void SetDraftStatus_ShouldReturnSuccessResult_WhenDraftStatusIsValid()
  {
    // Arrange
    var draft = DraftFactory.CreateStandardDraft().Value;
    var draftStatus = DraftStatus.Completed;

    // Act
    draft.SetDraftStatus(draftStatus);

    // Assert
    draft.DraftStatus.Should().Be(draftStatus);
  }

  [Fact]
  public void SetGameBoard_ShouldReturnSuccessResult_WhenGameBoardIsValid()
  {
    // Arrange
    var draft = DraftFactory.CreateStandardDraft().Value;
    var gameBoard = GameBoard.Create(draft).Value;

    // Act
    draft.SetGameBoard(gameBoard);

    // Assert
    draft.GameBoard.Should().Be(gameBoard);
  }

  private static Draft SetupAndStartDraft()
  {
    var draft = DraftFactory.CreateStandardDraft().Value;
    for (int i = 0; i < draft.TotalDrafters; i++)
    {
      draft.AddDrafter(DrafterFactory.CreateDrafter());
    }

    for (int i = 0; i < draft.TotalHosts; i++)
    {
      draft.AddHost(HostsFactory.CreateHost().Value);
    }
    draft.StartDraft();
    return draft;
  }
}

