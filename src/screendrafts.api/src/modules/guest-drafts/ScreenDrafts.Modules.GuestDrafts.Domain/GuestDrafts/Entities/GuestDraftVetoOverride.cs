namespace ScreenDrafts.Modules.GuestDrafts.Domain.GuestDrafts.Entities;

public sealed class GuestDraftVetoOverride : Entity<GuestDraftVetoOverrideId>
{
  private GuestDraftVetoOverride(
    GuestDraftVeto veto,
    GuestDraftParticipant issuedByParticipant,
    string? actedByPublicId,
    string? note,
    bool spentFromFungiblePool,
    GuestDraftVetoOverrideId? id = null
  )
    : base(id ?? GuestDraftVetoOverrideId.CreateUnique())
  {
    Veto = veto;
    VetoId = veto.Id;

    IssuedByParticipant = issuedByParticipant;
    IssuedByParticipantId = issuedByParticipant.Id;

    ActedByPublicId = actedByPublicId;
    Note = note;
    SpentFromFungiblePool = spentFromFungiblePool;
  }

  private GuestDraftVetoOverride() { }

  public GuestDraftVetoId VetoId { get; private set; } = default!;
  public GuestDraftVeto Veto { get; private set; } = default!;

  public GuestDraftParticipant IssuedByParticipant { get; private set; } = default!;
  public GuestDraftParticipantId IssuedByParticipantId { get; private set; } = default!;

  public string? ActedByPublicId { get; private set; }
  public bool SpentFromFungiblePool { get; private set; }
  public string? Note { get; private set; }

  public static Result<GuestDraftVetoOverride> Create(
    GuestDraftVeto veto,
    GuestDraftParticipant issuedByParticipant,
    string? actedByPublicId = null,
    string? note = null,
    bool spentFromFungiblePool = false,
    GuestDraftVetoOverrideId? id = null
  )
  {
    ArgumentNullException.ThrowIfNull(veto);
    ArgumentNullException.ThrowIfNull(issuedByParticipant);

    return new GuestDraftVetoOverride(
      veto: veto,
      issuedByParticipant: issuedByParticipant,
      actedByPublicId: actedByPublicId,
      note: note,
      spentFromFungiblePool: spentFromFungiblePool,
      id: id
    );
  }
}
