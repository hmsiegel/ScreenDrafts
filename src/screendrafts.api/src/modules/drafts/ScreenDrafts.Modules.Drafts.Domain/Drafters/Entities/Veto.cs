namespace ScreenDrafts.Modules.Drafts.Domain.Drafters.Entities;

public sealed class Veto : Entity<VetoId>
{
  private Veto(
    VetoId id,
    Pick pick,
    ParticipantId? issuedBy,
    VetoIssuerKind issuerKind,
    string? note = null)
    : base(id)
  {
    Pick = pick;
    PickId = pick.Id;
    IssuedBy = issuedBy;
    IssuerKind = issuerKind;
    Note = note;
  }

  private Veto()
  {
  }

  public Pick Pick { get; private set; } = default!;
  public PickId PickId { get; private set; } = default!;

  public DraftPart DraftPart => Pick.DraftPart;
  public DraftPartId DraftPartId => Pick.DraftPartId;

  public ParticipantId? IssuedBy { get; private set; }
  public VetoIssuerKind IssuerKind { get; private set; } = default!;

  public bool IsOverridden { get; private set; }
  public ParticipantId? OverriddenBy { get; private set; }

  public DateTime OccuredOn { get; private set; } = DateTime.UtcNow;
  public string? Note { get; private set; } = default!;


  public static Result<Veto> Create(
    Pick pick,
    VetoIssuerKind issuerKind,
    ParticipantId? by,
    VetoId? id = null,
    string? note = null)
  {
    if (pick is null)
    {
      return Result.Failure<Veto>(VetoErrors.PickMustBeProvided);
    }

    ArgumentNullException.ThrowIfNull(pick);

    var veto = new Veto(
      pick: pick,
      issuerKind: issuerKind,
      issuedBy: by,
      note: note,
      id: id ?? VetoId.CreateUnique());

    veto.Raise(new VetoCreatedDomainEvent(
      veto.Id.Value,
      pick.Id.Value,
      by));

    return veto;
  }

  public Result Override(ParticipantId? by)
  {
    if (IsOverridden)
    {
      return Result.Failure(VetoErrors.VetoOverrideAlreadyUsed);
    }

    IsOverridden = true;
    OverriddenBy = by;

    Raise(new VetoOverriddenDomainEvent(
      Id.Value,
      Pick.Id.Value,
      by));

    return Result.Success();
  }
}
