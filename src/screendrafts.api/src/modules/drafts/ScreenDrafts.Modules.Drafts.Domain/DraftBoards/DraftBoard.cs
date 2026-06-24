namespace ScreenDrafts.Modules.Drafts.Domain.DraftBoards;

public sealed class DraftBoard : AggregateRoot<DraftBoardId, Guid>
{
  private readonly List<DraftBoardItem> _items = [];

  private DraftBoard(
    DraftId draftId,
    Participant participant,
    string publicId,
    bool isPoolSourced = false,
    DraftBoardId? id = null
  )
    : base(id ?? DraftBoardId.CreateUnique())
  {
    DraftId = draftId;
    Participant = participant;
    PublicId = publicId;
    IsPoolSourced = isPoolSourced;
  }

  private DraftBoard() { }

  public DraftId DraftId { get; private set; } = default!;
  public Participant Participant { get; private set; } = default!;
  public string PublicId { get; private set; } = default!;
  public DateTime CreatedAtUtc { get; private set; } = default!;
  public DateTime? UpdatedAtUtc { get; private set; }

  /// <summary>
  /// True for boards that mirror a Draft Pool (Super Draft). When true:
  /// - Only the pool sync may add or remove items (direct calls are blocked).
  /// - Once the pool is locked, all adds and removes are blocked.
  /// </summary>
  public bool IsPoolSourced { get; private set; }

  /// <summary>
  /// True once the pool that sources this board is locked. No further
  /// adds or removes are permitted after this point.
  /// </summary>
  public bool IsLocked { get; private set; }

  public IReadOnlyCollection<DraftBoardItem> Items => _items.AsReadOnly();

  public static Result<DraftBoard> Create(
    DraftId draftId,
    Participant participant,
    string publicId,
    bool isPoolSourced = false
  )
  {
    ArgumentNullException.ThrowIfNull(draftId);

    if (string.IsNullOrWhiteSpace(publicId))
    {
      return Result.Failure<DraftBoard>(DraftBoardErrors.InvalidPublicId);
    }

    var board = new DraftBoard(
      draftId: draftId,
      participant: participant,
      publicId: publicId,
      isPoolSourced: isPoolSourced
    )
    {
      CreatedAtUtc = DateTime.UtcNow,
    };

    board.Raise(new DraftBoardCreatedDomainEvent(board.Id.Value, draftId.Value, participant.Value));

    return Result.Success(board);
  }

  /// <summary>
  /// Adds a movie to the board. Pool-sourced boards reject direct adds —
  /// use SyncAddItem for pool-driven additions.
  /// </summary>
  public Result AddItem(int tmdbId, string? notes, int? priority)
  {
    if (IsPoolSourced)
    {
      return Result.Failure(DraftBoardErrors.PoolSourcedBoardCannotBeModifiedDirectly);
    }

    return AddItemInternal(tmdbId, notes, priority);
  }

  /// <summary>
  /// Adds a movie to the board as a pool or candidate list sync action.
  /// Blocked once the pool is locked.
  /// </summary>
  public Result SyncAddItem(int tmdbId)
  {
    if (IsLocked)
    {
      return Result.Failure(DraftBoardErrors.BoardIsLocked);
    }

    if (_items.Any(i => i.TmdbId == tmdbId))
    {
      return Result.Success(); // idempotent
    }

    return AddItemInternal(tmdbId, notes: null, priority: null);
  }

  /// <summary>
  /// Removes a movie from the board as a pool sync action.
  /// Blocked once the pool is locked.
  /// </summary>
  public Result SyncRemoveItem(int tmdbId)
  {
    if (IsLocked)
    {
      return Result.Failure(DraftBoardErrors.BoardIsLocked);
    }

    return RemoveItemInternal(tmdbId, mustExist: false);
  }

  /// <summary>
  /// Removes a movie from the board. Pool-sourced boards reject direct removes —
  /// use SyncRemoveItem for pool-driven removals.
  /// </summary>
  public Result RemoveItem(int tmdbId)
  {
    if (IsPoolSourced)
    {
      return Result.Failure(DraftBoardErrors.PoolSourcedBoardCannotBeModifiedDirectly);
    }

    return RemoveItemInternal(tmdbId, mustExist: true);
  }

  public Result UpdateItem(int tmdbId, string? notes, int? priority)
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

  /// <summary>
  /// Locks this board. Called when the sourcing pool is locked.
  /// No further adds or removes are permitted after this point.
  /// </summary>
  public Result Lock()
  {
    if (IsLocked)
    {
      return Result.Success();
    }

    IsLocked = true;
    UpdatedAtUtc = DateTime.UtcNow;

    return Result.Success();
  }

  private Result AddItemInternal(int tmdbId, string? notes, int? priority)
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

    Raise(
      new MovieAddedToDraftBoardDomainEvent(
        boardId: Id.Value,
        draftId: DraftId.Value,
        participantId: Participant.Value
      )
    );

    return Result.Success();
  }

  private Result RemoveItemInternal(int tmdbId, bool mustExist)
  {
    var item = _items.FirstOrDefault(i => i.TmdbId == tmdbId);

    if (item is null)
    {
      return mustExist
        ? Result.Failure(DraftBoardErrors.MovieNotFoundOnTheBoard(tmdbId))
        : Result.Success();
    }

    _items.Remove(item);

    UpdatedAtUtc = DateTime.UtcNow;

    Raise(
      new MovieRemovedFromDraftBoardDomainEvent(
        boardId: Id.Value,
        draftId: DraftId.Value,
        participantId: Participant.Value
      )
    );

    return Result.Success();
  }
}
