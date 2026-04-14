namespace ScreenDrafts.Modules.Drafts.Domain.DraftParts.Entities;

public sealed class GameBoard : Entity<GameBoardId>
{
  private readonly Collection<DraftPosition> _draftPositions = [];

  private GameBoard(
    DraftPart draftPart,
    GameBoardId? id = null) :
    base(id ?? GameBoardId.CreateUnique())
  {
    DraftPart = draftPart;
    DraftPartId = draftPart.Id;
  }

  private GameBoard(
    SubDraft subDraft,
    GameBoardId? id = null) :
    base(id ?? GameBoardId.CreateUnique())
  {
    SubDraftId = subDraft.Id;
  }

  private GameBoard()
  {
  }

  public DraftPart? DraftPart { get; private set; } = default!;
  public DraftPartId? DraftPartId { get; private set; } = default!;
  public SubDraftId? SubDraftId { get; private set; }

  public IReadOnlyCollection<DraftPosition> DraftPositions => _draftPositions;

  public static Result<GameBoard> Create(
    DraftPart draftPart,
    GameBoardId? id = null)
  {
    if (draftPart is null)
    {
      return Result.Failure<GameBoard>(GameBoardErrors.GameBoardCreationFailed);
    }

    ArgumentNullException.ThrowIfNull(draftPart);

    var gameBoard = new GameBoard(
      draftPart: draftPart,
      id: id);

    return Result.Success(gameBoard);
  }
  public static Result<GameBoard> CreateForSubDraft(SubDraft subDraft, GameBoardId? id = null)
  {
    if (subDraft is null)
    {
      return Result.Failure<GameBoard>(GameBoardErrors.GameBoardCreationFailed);
    }

    ArgumentNullException.ThrowIfNull(subDraft);

    var gameBoard = new GameBoard(
      subDraft: subDraft,
      id: id);

    return Result.Success(gameBoard);
  }

  public Result AssignDraftPositions(ICollection<DraftPosition> draftPositions, int participantCount)
  {
    if (draftPositions is null)
    {
      return Result.Failure(GameBoardErrors.DraftPositionsMissing);
    }

    if (draftPositions.Count < 1 || draftPositions.Count != participantCount)
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

}
