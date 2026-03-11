namespace ScreenDrafts.Modules.Drafts.Domain.DraftBoards;

public sealed class DraftBoard : AggregateRoot<DraftBoardId, Guid>
{
  private readonly List<DraftBoardItem> _items = [];

  private DraftBoard(
    DraftId draftId,
    Participant participant,
    string publicId,
    DraftBoardId? id = null)
    : base(id ?? DraftBoardId.CreateUnique())
  {
    DraftId = draftId;
    Participant = participant;
    PublicId = publicId;
  }

  private DraftBoard()
  {
  }

  public DraftId DraftId { get; private set; } = default!;
  public Participant Participant { get; private set; } = default!;
  public string PublicId { get; private set; } = default!;
  public DateTime CreatedAtUtc { get; private set; } = default!;
  public DateTime? UpdatedAtUtc { get; private set; }

  public IReadOnlyCollection<DraftBoardItem> Items => _items.AsReadOnly();

  public static Result<DraftBoard> Create(
    DraftId draftId,
    Participant participant,
    string publicId)
  {
    ArgumentNullException.ThrowIfNull(draftId);

    if (string.IsNullOrWhiteSpace(publicId))
    {
      return Result.Failure<DraftBoard>(DraftBoardErrors.InvalidPublicId);
    }

    var board = new DraftBoard(
      draftId: draftId,
      participant: participant,
      publicId: publicId)
    {
      CreatedAtUtc = DateTime.UtcNow
    };

    board.Raise(new DraftBoardCreatedDomainEvent(
      board.Id.Value,
      draftId.Value,
      participant.Value));

    return Result.Success(board);
  }

  public Result AddItem(
    int tmdbId,
    string? notes,
    int? priority)
  {
    if (_items.Any(i => i.TmdbId == tmdbId))
    {
      return Result.Failure(DraftBoardErrors.MovieAlreadyOnTheBoard(tmdbId));
    }

    var itemResult = DraftBoardItem.Create(tmdbId, notes, priority);

    if (itemResult.IsFailure)
    {
      return Result.Failure(itemResult.Errors);
    }

    _items.Add(itemResult.Value);

    UpdatedAtUtc = DateTime.UtcNow;

    Raise(new MovieAddedToDraftBoardDomainEvent(
      boardId: Id.Value,
      draftId: DraftId.Value,
      participantId: Participant.Value));

    return Result.Success();
  }

  public Result RemoveItem(int tmdbId)
  {
    var item = _items.FirstOrDefault(i => i.TmdbId == tmdbId);

    if (item is null)
    {
      return Result.Failure(DraftBoardErrors.MovieNotFoundOnTheBoard(tmdbId));
    }

    _items.Remove(item);

    UpdatedAtUtc = DateTime.UtcNow;

    Raise(new MovieRemovedFromDraftBoardDomainEvent(
      boardId: Id.Value,
      draftId: DraftId.Value,
      participantId: Participant.Value));

    return Result.Success();
  }

  public Result UpdateItem(
    int tmdbId,
    string? notes,
    int? priority)
  {
    var item = _items.FirstOrDefault(i => i.TmdbId == tmdbId);

    if (item is null)
    {
      return Result.Failure(DraftBoardErrors.MovieNotFoundOnTheBoard(tmdbId));
    }

    item.Update(notes, priority);

    UpdatedAtUtc = DateTime.UtcNow;

    return Result.Success();
  }
}


