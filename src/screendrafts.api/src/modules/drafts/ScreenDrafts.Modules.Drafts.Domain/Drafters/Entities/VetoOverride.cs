namespace ScreenDrafts.Modules.Drafts.Domain.Drafters.Entities;

public sealed class VetoOverride : Entity<VetoOverrideId>
{
  private VetoOverride(
    VetoOverrideId id,
    Veto veto,
    ParticipantId issuedBy,
    VetoOverrideIssuerKind issuerKind)
    : base(id)
  {
    Veto = veto;
    IssuedBy = issuedBy;
    IssuerKind = issuerKind;
    VetoId = veto.Id;

  }

  private VetoOverride()
  {
  }

  public VetoId VetoId { get; private set; } = default!;
  public Veto Veto { get; private set; } = default!;

  public ParticipantId IssuedBy { get; private set; } = default!;
  public VetoOverrideIssuerKind IssuerKind { get; private set; } = default!;



  public static Result<VetoOverride> Create(
    Veto veto,
    ParticipantId issuedBy,
    VetoOverrideIssuerKind issuerKind,
    VetoOverrideId? id = null)
  {
    if (issuedBy.HasNoValue)
    {
      return Result.Failure<VetoOverride>(VetoOverrideErrors.IssuedByMustBeProvided);
    }

    if (veto is null)
    {
      return Result.Failure<VetoOverride>(VetoOverrideErrors.VetoMustBeProvided);
    }

    ArgumentNullException.ThrowIfNull(veto);

    var vetoOverride = new VetoOverride(
      id: id ?? VetoOverrideId.CreateUnique(),
      veto: veto,
      issuedBy: issuedBy,
      issuerKind: issuerKind);

    vetoOverride.Raise(new VetoOverrideCreatedDomainEvent(
      vetoOverride.Id.Value,
      veto.Id.Value,
      issuedBy));

    return vetoOverride;
  }
}
