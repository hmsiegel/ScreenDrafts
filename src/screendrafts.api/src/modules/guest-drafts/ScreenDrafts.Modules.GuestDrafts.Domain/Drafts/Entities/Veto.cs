using ScreenDrafts.Modules.GuestDrafts.Domain.Drafts.Errors;
using ScreenDrafts.Modules.GuestDrafts.Domain.Drafts.ValueObjects;

namespace ScreenDrafts.Modules.GuestDrafts.Domain.Drafts.Entities;

public sealed class Veto : Entity<VetoId>
{
  private Veto(
    Pick pick,
    DraftParticipant issuedByParticipant,
    int sequence,
    string? actedByPublicId,
    bool spentFromFungiblePool,
    DateTime occurredOn,
    string? note,
    VetoId? id = null
  )
    : base(id ?? VetoId.CreateUnique())
  {
    TargetPick = pick;
    TargetPickId = pick.Id;

    Sequence = sequence;

    IssuedByParticipant = issuedByParticipant;
    IssuedByParticipantId = issuedByParticipant.Id;

    ActedByPublicId = actedByPublicId;
    SpentFromFungiblePool = spentFromFungiblePool;
    OccurredOn = occurredOn;
    Note = note;
  }

  private Veto() { }

  public Pick TargetPick { get; private set; } = default!;
  public PickId TargetPickId { get; private set; } = default!;

  /// <summary>
  /// 1-based position of this veto in its pick's history. Normally 1. A second veto
  /// (sequence 2) only occurs when the first veto was overridden and the resulting
  /// override was itself overridden. Persisted explicitly because EF collection
  /// load order isn't guaranteed -- same reasoning as canonical Veto.Sequence.
  /// </summary>
  public int Sequence { get; private set; }

  public DraftParticipant IssuedByParticipant { get; private set; } = default!;
  public DraftParticipantId IssuedByParticipantId { get; private set; } = default!;

  public string? ActedByPublicId { get; private set; }
  public bool SpentFromFungiblePool { get; private set; }
  public bool IsOverridden { get; private set; }
  public VetoOverride? VetoOverride { get; private set; }

  public DateTime OccurredOn { get; private set; }
  public string? Note { get; private set; }

  public static Result<Veto> Create(
    Pick pick,
    DraftParticipant issuedByParticipant,
    string? actedByPublicId = null,
    string? note = null,
    bool spentFromFungiblePool = false,
    VetoId? id = null
  )
  {
    ArgumentNullException.ThrowIfNull(pick);
    ArgumentNullException.ThrowIfNull(issuedByParticipant);

    return new Veto(
      pick: pick,
      issuedByParticipant: issuedByParticipant,
      sequence: pick.Vetoes.Count + 1,
      actedByPublicId: actedByPublicId,
      spentFromFungiblePool: spentFromFungiblePool,
      occurredOn: DateTime.UtcNow,
      note: note,
      id: id
    );
  }

  public Result Override(
    DraftParticipant by,
    string? actedByPublicId = null,
    string? note = null,
    bool spentFromFungiblePool = false
  )
  {
    if (IsOverridden)
    {
      return Result.Failure(DraftErrors.VetoOverrideAlreadyUsed);
    }

    var overrideResult = VetoOverride.Create(
      veto: this,
      issuedByParticipant: by,
      actedByPublicId: actedByPublicId,
      note: note,
      spentFromFungiblePool: spentFromFungiblePool
    );

    if (overrideResult.IsFailure)
    {
      return Result.Failure(overrideResult.Errors);
    }

    IsOverridden = true;
    VetoOverride = overrideResult.Value;

    return Result.Success();
  }
}
