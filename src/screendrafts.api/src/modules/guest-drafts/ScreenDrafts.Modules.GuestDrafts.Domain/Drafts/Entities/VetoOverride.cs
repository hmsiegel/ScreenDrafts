using ScreenDrafts.Modules.GuestDrafts.Domain.Drafts.ValueObjects;

namespace ScreenDrafts.Modules.GuestDrafts.Domain.Drafts.Entities;

public sealed class VetoOverride : Entity<VetoOverrideId>
{
  private VetoOverride(
    Veto veto,
    DraftParticipant issuedByParticipant,
    string? actedByPublicId,
    string? note,
    bool spentFromFungiblePool,
    VetoOverrideId? id = null
  )
    : base(id ?? VetoOverrideId.CreateUnique())
  {
    Veto = veto;
    VetoId = veto.Id;

    IssuedByParticipant = issuedByParticipant;
    IssuedByParticipantId = issuedByParticipant.Id;

    ActedByPublicId = actedByPublicId;
    Note = note;
    SpentFromFungiblePool = spentFromFungiblePool;
  }

  private VetoOverride() { }

  public VetoId VetoId { get; private set; } = default!;
  public Veto Veto { get; private set; } = default!;

  public DraftParticipant IssuedByParticipant { get; private set; } = default!;
  public DraftParticipantId IssuedByParticipantId { get; private set; } = default!;

  public string? ActedByPublicId { get; private set; }
  public bool SpentFromFungiblePool { get; private set; }
  public string? Note { get; private set; }

  public static Result<VetoOverride> Create(
    Veto veto,
    DraftParticipant issuedByParticipant,
    string? actedByPublicId = null,
    string? note = null,
    bool spentFromFungiblePool = false,
    VetoOverrideId? id = null
  )
  {
    ArgumentNullException.ThrowIfNull(veto);
    ArgumentNullException.ThrowIfNull(issuedByParticipant);

    return new VetoOverride(
      veto: veto,
      issuedByParticipant: issuedByParticipant,
      actedByPublicId: actedByPublicId,
      note: note,
      spentFromFungiblePool: spentFromFungiblePool,
      id: id
    );
  }
}
