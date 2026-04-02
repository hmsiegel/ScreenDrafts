namespace ScreenDrafts.Modules.Drafts.Domain.DraftParts.Entities;

public sealed class SubDraft : Entity<SubDraftId>
{
  private GameBoard? _gameBoard;
  private SubDraft(
    DraftPartId draftPartId,
    int index,
    SubjectKind subjectKind,
    string subjectName,
    SubDraftId? id = null)
    : base(id ?? SubDraftId.CreateUnique())
  {
    DraftPartId = draftPartId;
    Index = index;
    SubjectKind = subjectKind;
    SubjectName = subjectName;
  }

  private SubDraft()
  {

  }

  public string PublicId { get; private set; } = default!;
  public DraftPartId DraftPartId { get; private set; } = default!;
  public int Index { get; private set; }
  public SubjectKind SubjectKind { get; private set; } = default!;
  public string SubjectName { get; private set; } = default!;
  public GameBoard? GameBoard => _gameBoard;

  public static Result<SubDraft> Create(
    int index,
    SubjectKind subjectKind,
    string subjectName,
    DraftPartId draftPartId,
    string publicId,
    SubDraftId? id = null)
  {
    if (index < 0)
    {
      return Result.Failure<SubDraft>(DraftPartErrors.PartIndexIsOutOfRange);
    }

    var subDraft = new SubDraft(
      draftPartId: draftPartId,
      index: index,
      subjectKind: subjectKind,
      subjectName: subjectName,
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

  public int ComputeVetoRemainder(int startingVetoes, IEnumerable<Veto> vetoes)
  {
    var used = vetoes.Count(v => v.SubDraftId == Id);
    return Math.Max(0, startingVetoes - used);
  }
}
