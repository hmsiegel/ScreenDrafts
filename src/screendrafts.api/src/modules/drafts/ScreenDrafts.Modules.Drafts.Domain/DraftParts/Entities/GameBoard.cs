namespace ScreenDrafts.Modules.Drafts.Domain.DraftParts.Entities;

public sealed class GameBoard : Entity<GameBoardId>
{
  private readonly List<DraftPosition> _draftPositions = [];

  private GameBoard(DraftPart draftPart, GameBoardId? id = null)
    : base(id ?? GameBoardId.CreateUnique())
  {
    DraftPart = draftPart;
    DraftPartId = draftPart.Id;
  }

  private GameBoard(SubDraft subDraft, GameBoardId? id = null)
    : base(id ?? GameBoardId.CreateUnique())
  {
    SubDraftId = subDraft.Id;
  }

  private GameBoard() { }

  public DraftPart? DraftPart { get; private set; } = default!;
  public DraftPartId? DraftPartId { get; private set; } = default!;
  public SubDraftId? SubDraftId { get; private set; }

  public IReadOnlyCollection<DraftPosition> DraftPositions => _draftPositions;

  public static Result<GameBoard> Create(DraftPart draftPart, GameBoardId? id = null)
  {
    if (draftPart is null)
    {
      return Result.Failure<GameBoard>(GameBoardErrors.GameBoardCreationFailed);
    }

    ArgumentNullException.ThrowIfNull(draftPart);

    var gameBoard = new GameBoard(draftPart: draftPart, id: id);

    return Result.Success(gameBoard);
  }

  public static Result<GameBoard> CreateForSubDraft(SubDraft subDraft, GameBoardId? id = null)
  {
    if (subDraft is null)
    {
      return Result.Failure<GameBoard>(GameBoardErrors.GameBoardCreationFailed);
    }

    ArgumentNullException.ThrowIfNull(subDraft);

    var gameBoard = new GameBoard(subDraft: subDraft, id: id);

    return Result.Success(gameBoard);
  }

  public Result AssignDraftPositions(ICollection<DraftPosition> draftPositions)
  {
    if (draftPositions is null)
    {
      return Result.Failure(GameBoardErrors.DraftPositionsMissing);
    }

    var participantCount = (DraftPart?.TotalDrafters ?? 0) + (DraftPart?.TotalDrafterTeams ?? 0);
    if (draftPositions.Count == 0 || draftPositions.Count != participantCount)
    {
      return Result.Failure(GameBoardErrors.InvalidNumberOfParticipants);
    }

    var allPicks = draftPositions.SelectMany(p => p.Picks).ToList();
    if (allPicks.Count != allPicks.Distinct().Count())
    {
      return Result.Failure(GameBoardErrors.DuplicatePickSlots);
    }

    foreach (var position in draftPositions)
    {
      _draftPositions.Add(position);
    }

    return Result.Success();
  }

  public void ClearPositions()
  {
    _draftPositions.Clear();
  }

  /// <summary>
  /// Adds the community position to the board without participant count validation. The
  /// community position is managed separately from human drafter positions and
  /// is excluded from trivia assignment.
  /// </summary>
  /// <param name="draftPosition"></param>
  internal void AddCommunityPosition(DraftPosition draftPosition)
  {
    _draftPositions.Add(draftPosition);
  }
}
