namespace ScreenDrafts.Modules.Drafts.UnitTests.DraftBoards;

public class DraftBoardTests : DraftsBaseTest
{
  // ========================================
  // Create Tests
  // ========================================

  [Fact]
  public void Create_ShouldReturnSuccess_WhenValidParametersProvided()
  {
    var draftId = DraftId.CreateUnique();
    var participant = new Participant(Guid.NewGuid(), ParticipantKind.Drafter);
    var publicId = Faker.Random.AlphaNumeric(10);

    var result = DraftBoard.Create(draftId, participant, publicId);

    result.IsSuccess.Should().BeTrue();
    result.Value.DraftId.Should().Be(draftId);
    result.Value.Participant.Should().Be(participant);
    result.Value.PublicId.Should().Be(publicId);
    result.Value.Items.Should().BeEmpty();
  }

  [Fact]
  public void Create_ShouldReturnFailure_WhenPublicIdIsEmpty()
  {
    var draftId = DraftId.CreateUnique();
    var participant = new Participant(Guid.NewGuid(), ParticipantKind.Drafter);

    var result = DraftBoard.Create(draftId, participant, string.Empty);

    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(DraftBoardErrors.InvalidPublicId);
  }

  [Fact]
  public void Create_ShouldReturnFailure_WhenPublicIdIsWhitespace()
  {
    var draftId = DraftId.CreateUnique();
    var participant = new Participant(Guid.NewGuid(), ParticipantKind.Drafter);

    var result = DraftBoard.Create(draftId, participant, "   ");

    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(DraftBoardErrors.InvalidPublicId);
  }

  [Fact]
  public void Create_ShouldThrow_WhenDraftIdIsNull()
  {
    var participant = new Participant(Guid.NewGuid(), ParticipantKind.Drafter);

    Action act = () => DraftBoard.Create(null!, participant, Faker.Random.AlphaNumeric(10));

    act.Should().Throw<ArgumentNullException>();
  }

  // ========================================
  // AddItem Tests
  // ========================================

  [Fact]
  public void AddItem_ShouldSucceed_WhenMovieNotAlreadyOnBoard()
  {
    var board = CreateBoard();
    var tmdbId = Faker.Random.Int(1, 1_000_000);

    var result = board.AddItem(tmdbId, notes: null, priority: null);

    result.IsSuccess.Should().BeTrue();
    board.Items.Should().Contain(i => i.TmdbId == tmdbId);
  }

  [Fact]
  public void AddItem_ShouldFail_WhenMovieAlreadyOnBoard()
  {
    var board = CreateBoard();
    var tmdbId = Faker.Random.Int(1, 1_000_000);
    board.AddItem(tmdbId, notes: null, priority: null);

    var result = board.AddItem(tmdbId, notes: null, priority: null);

    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(DraftBoardErrors.MovieAlreadyOnTheBoard(tmdbId));
  }

  [Fact]
  public void AddItem_ShouldFail_WhenTmdbIdIsZeroOrLess()
  {
    var board = CreateBoard();

    var result = board.AddItem(0, notes: null, priority: null);

    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(DraftBoardErrors.InvalidTmdbId);
  }

  [Fact]
  public void AddItem_ShouldPersistNotesAndPriority()
  {
    var board = CreateBoard();
    var tmdbId = Faker.Random.Int(1, 1_000_000);
    var notes = Faker.Lorem.Sentence();
    var priority = Faker.Random.Int(1, 100);

    board.AddItem(tmdbId, notes, priority);

    var item = board.Items.First(i => i.TmdbId == tmdbId);
    item.Notes.Should().Be(notes);
    item.Priority.Should().Be(priority);
  }

  [Fact]
  public void AddItem_ShouldTrackMultipleItems()
  {
    var board = CreateBoard();

    board.AddItem(100, null, null);
    board.AddItem(200, null, null);
    board.AddItem(300, null, null);

    board.Items.Should().HaveCount(3);
  }

  // ========================================
  // RemoveItem Tests
  // ========================================

  [Fact]
  public void RemoveItem_ShouldSucceed_WhenItemExists()
  {
    var board = CreateBoard();
    var tmdbId = Faker.Random.Int(1, 1_000_000);
    board.AddItem(tmdbId, null, null);

    var result = board.RemoveItem(tmdbId);

    result.IsSuccess.Should().BeTrue();
    board.Items.Should().NotContain(i => i.TmdbId == tmdbId);
  }

  [Fact]
  public void RemoveItem_ShouldFail_WhenItemNotFound()
  {
    var board = CreateBoard();
    var tmdbId = Faker.Random.Int(1, 1_000_000);

    var result = board.RemoveItem(tmdbId);

    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(DraftBoardErrors.MovieNotFoundOnTheBoard(tmdbId));
  }

  [Fact]
  public void RemoveItem_ShouldOnlyRemoveTargetItem()
  {
    var board = CreateBoard();
    board.AddItem(100, null, null);
    board.AddItem(200, null, null);

    board.RemoveItem(100);

    board.Items.Should().ContainSingle(i => i.TmdbId == 200);
  }

  // ========================================
  // UpdateItem Tests
  // ========================================

  [Fact]
  public void UpdateItem_ShouldSucceed_WhenItemExists()
  {
    var board = CreateBoard();
    var tmdbId = Faker.Random.Int(1, 1_000_000);
    board.AddItem(tmdbId, null, null);
    var newNotes = Faker.Lorem.Sentence();
    var newPriority = Faker.Random.Int(1, 100);

    var result = board.UpdateItem(tmdbId, newNotes, newPriority);

    result.IsSuccess.Should().BeTrue();
    var item = board.Items.First(i => i.TmdbId == tmdbId);
    item.Notes.Should().Be(newNotes);
    item.Priority.Should().Be(newPriority);
  }

  [Fact]
  public void UpdateItem_ShouldFail_WhenItemNotFound()
  {
    var board = CreateBoard();
    var tmdbId = Faker.Random.Int(1, 1_000_000);

    var result = board.UpdateItem(tmdbId, null, null);

    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(DraftBoardErrors.MovieNotFoundOnTheBoard(tmdbId));
  }

  [Fact]
  public void UpdateItem_ShouldClearNotesAndPriority_WhenNullProvided()
  {
    var board = CreateBoard();
    var tmdbId = Faker.Random.Int(1, 1_000_000);
    board.AddItem(tmdbId, "original notes", 5);

    board.UpdateItem(tmdbId, null, null);

    var item = board.Items.First(i => i.TmdbId == tmdbId);
    item.Notes.Should().BeNull();
    item.Priority.Should().BeNull();
  }

  // ========================================
  // Helpers
  // ========================================

  private static DraftBoard CreateBoard() =>
    DraftBoard.Create(
      DraftId.CreateUnique(),
      new Participant(Guid.NewGuid(), ParticipantKind.Drafter),
      Faker.Random.AlphaNumeric(10)).Value;
}
