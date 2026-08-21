namespace ScreenDrafts.Modules.Drafts.Domain.DraftParts.Entities;

public sealed class Veto : Entity<VetoId>
{
  private Veto(
    Pick pick,
    DraftPartParticipant issuedByParticipant,
    string actedByPublicId,
    int sequence,
    bool spentFromFungiblePool = false,
    DateTime? occurredOn = null,
    string? note = null,
    VetoId? id = null
  )
    : base(id ?? VetoId.CreateUnique())
  {
    TargetPick = pick;
    TargetPickId = pick.Id;

    SubDraftId = pick.SubDraftId;

    Sequence = sequence;
    SpentFromFungiblePool = spentFromFungiblePool;
    IssuedByParticipant = issuedByParticipant;
    IssuedByParticipantId = issuedByParticipant.Id;

    ActedByPublicId = actedByPublicId;
    OccurredOn = occurredOn;
    Note = note;
  }

  private Veto() { }

  public Pick TargetPick { get; private set; } = default!;
  public PickId TargetPickId { get; private set; } = default!;

  public DraftPart DraftPart => TargetPick.DraftPart;
  public DraftPartId DraftPartId => TargetPick.DraftPartId;

  public SubDraftId? SubDraftId { get; private set; } = default!;

  /// <summary>
  /// 1-based position of this veto in its pick's history. Normally 1.
  /// A second veto on the same pick (sequence 2) only occurs when the first veto
  /// was overridden and the resulting override was itself overridden, re-vetoing
  /// the pick. Peristed explicitly because collection load order from EF is not guaranteed to match insertion order.
  /// </summary>
  public int Sequence { get; private set; }

  public DraftPartParticipant IssuedByParticipant { get; private set; } = default!;
  public DraftPartParticipantId IssuedByParticipantId { get; private set; } = default!;

  public string? ActedByPublicId { get; private set; }

  public bool SpentFromFungiblePool { get; private set; }

  public bool IsOverridden { get; private set; }

  public VetoOverride? VetoOverride { get; private set; } = default!;

  public DateTime? OccurredOn { get; private set; }
  public string? Note { get; private set; } = default!;

  public static Result<Veto> Create(
    Pick pick,
    DraftPartParticipant issuedByParticipant,
    string? actedByPublicId = null,
    VetoId? id = null,
    string? note = null,
    bool spentFromFungiblePool = false
  )
  {
    if (pick is null)
    {
      return Result.Failure<Veto>(VetoErrors.PickMustBeProvided);
    }

    ArgumentNullException.ThrowIfNull(issuedByParticipant);
    ArgumentNullException.ThrowIfNull(pick);

    var veto = new Veto(
      pick: pick,
      issuedByParticipant: issuedByParticipant,
      actedByPublicId: actedByPublicId ?? string.Empty,
      sequence: pick.Vetoes.Count + 1,
      occurredOn: DateTime.UtcNow,
      note: note,
      spentFromFungiblePool: spentFromFungiblePool,
      id: id ?? VetoId.CreateUnique()
    );

    return veto;
  }

  internal static Result<Veto> SeedCreate(
    Pick pick,
    DraftPartParticipant issuedByParticipant,
    DateTime occurredOn,
    VetoId? id = null,
    string? note = null,
    bool spentFromFungiblePool = false
  )
  {
    var veto = new Veto(
      id: id,
      pick: pick,
      occurredOn: occurredOn,
      sequence: pick.Vetoes.Count + 1,
      issuedByParticipant: issuedByParticipant,
      actedByPublicId: string.Empty,
      note: note,
      spentFromFungiblePool: spentFromFungiblePool
    );
    return veto;
  }

  internal void SeedMarkOverridden()
  {
    IsOverridden = true;
  }

  public Result Override(
    Participant by,
    string? actedByPublicId = null,
    string? note = null,
    bool spentFromFungiblePool = false
  )
  {
    if (IsOverridden)
    {
      return Result.Failure(VetoErrors.VetoOverrideAlreadyUsed);
    }

    var participant = DraftPart.GetParticipantRequired(by);

    IsOverridden = true;
    VetoOverride = VetoOverride
      .Create(
        veto: this,
        issuedByParticipant: participant,
        actedByPublicId: actedByPublicId ?? string.Empty,
        note: note,
        spentFromFungiblePool: spentFromFungiblePool
      )
      .Value;

    return Result.Success();
  }
}
