namespace ScreenDrafts.Modules.Drafts.Domain.DraftParts.Entities;

public sealed class SubDraft : Entity<SubDraftId>
{
  private GameBoard? _gameBoard;
  private SubDraft(
    DraftPartId draftPartId,
    int index,
    SubDraftId? id = null)
    : base(id ?? SubDraftId.CreateUnique())
  {
    DraftPartId = draftPartId;
    Index = index;
  }

  private SubDraft()
  {

  }

  public string PublicId { get; private set; } = default!;
  public DraftPartId DraftPartId { get; private set; } = default!;
  public int Index { get; private set; }
  public SubDraftStatus Status { get; private set; } = SubDraftStatus.Pending;
  public SubjectKind? SubjectKind { get; private set; } = default!;
  public string? SubjectName { get; private set; } = default!;
  public GameBoard? GameBoard => _gameBoard;

  public static Result<SubDraft> Create(
    int index,
    DraftPartId draftPartId,
    string publicId,
    SubDraftId? id = null)
  {
    ArgumentNullException.ThrowIfNull(draftPartId);

    if (index < 0)
    {
      return Result.Failure<SubDraft>(DraftPartErrors.PartIndexIsOutOfRange);
    }

    var subDraft = new SubDraft(
      draftPartId: draftPartId,
      index: index,
      id: id
    )
    {
      PublicId = publicId
    };

    var boardResult = GameBoard.CreateForSubDraft(subDraft);

    if (boardResult.IsFailure)
    {
      return Result.Failure<SubDraft>(boardResult.Errors[0]);
    }

    subDraft.SetGameBoard(boardResult.Value);

    return Result.Success(subDraft);
  }

  private void SetGameBoard(GameBoard gameBoard) => _gameBoard = gameBoard;

  public int ComputeVetoRemainder(int startingVetoes, IEnumerable<(SubDraftId SubDraftId, bool IsOverridden)> vetoes)
  {
    var used = vetoes.Count(v => v.SubDraftId == Id);
    return Math.Max(0, startingVetoes - used);
  }

  public Result SetSubject(SubjectKind subjectKind, string subjectName)
  {
    if (string.IsNullOrWhiteSpace(subjectName))
    {
      return Result.Failure(SubDraftErrors.SubjectNameCannotBeEmpty);
    }

    SubjectKind = subjectKind;
    SubjectName = subjectName.Trim();
    return Result.Success();
  }

  public Result Activate()
  {
    if (Status != SubDraftStatus.Pending)
    {
      return Result.Failure(SubDraftErrors.CannotActivateSubDraft);
    }

    Status = SubDraftStatus.Active;
    return Result.Success();
  }

  public Result Complete()
  {
    if (Status != SubDraftStatus.Active)
    {
      return Result.Failure(SubDraftErrors.CannotCompleteSubDraft);
    }
    Status = SubDraftStatus.Completed;
    return Result.Success();
  }
}
