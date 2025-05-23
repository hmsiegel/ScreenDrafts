﻿namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.Entities;

public sealed class GameBoard : Entity<GameBoardId>
{
  private readonly Collection<DraftPosition> _draftPositions = [];

  private GameBoard(
    Draft draft,
    GameBoardId? id = null) :
    base(id ?? GameBoardId.CreateUnique())
  {
    Draft = draft;
    DraftId = draft.Id;
  }

  private GameBoard()
  {
  }

  public DraftId DraftId { get; private set; } = default!;

  public Draft Draft { get; private set; } = default!;

  public ICollection<DraftPosition> DraftPositions => _draftPositions.AsReadOnly();

  public static Result<GameBoard> Create(
    Draft draft)
  {
    if (draft is null)
    {
      return Result.Failure<GameBoard>(GameBoardErrors.GameBoardCreationFailed);
    }

    ArgumentNullException.ThrowIfNull(draft);

    var gameBoard = new GameBoard(draft);

    return Result.Success(gameBoard);
  }

  public Result AssignDraftPositions(ICollection<DraftPosition> draftPositions)
  {
    if (draftPositions is null)
    {
      return Result.Failure(GameBoardErrors.DraftPositionsMissing);
    }

    if (draftPositions.Count < 1)
    {
      return Result.Failure(GameBoardErrors.InvalidNumberOfDrafters);
    }

    if (draftPositions.Count != Draft.TotalDrafters)
    {
      return Result.Failure(GameBoardErrors.InvalidNumberOfDrafters);
    }

    foreach (var position in draftPositions)
    {
      _draftPositions.Add(position);
    }

    return Result.Success();
  }
}
