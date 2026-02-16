namespace ScreenDrafts.Modules.Drafts.Domain.DraftParts.Entities;

public sealed class VetoOverride : Entity<VetoOverrideId>
{
  private VetoOverride(
    Veto veto,
    DraftPartParticipant issuedByParticipant,
    VetoOverrideId? id = null)
    : base(id ?? VetoOverrideId.CreateUnique())
  {
    Veto = veto;
    VetoId = veto.Id;
    IssuedByParticipant = issuedByParticipant;
    IssuedByParticipantId = issuedByParticipant.Id;
  }

  private VetoOverride()
  {
  }

  public VetoId VetoId { get; private set; } = default!;
  public Veto Veto { get; private set; } = default!;

  public DraftPartParticipant IssuedByParticipant { get; private set; } = default!;
  public DraftPartParticipantId IssuedByParticipantId { get; private set; } = default!;


  public static Result<VetoOverride> Create(
    Veto veto,
    DraftPartParticipant issuedByParticipant,
    VetoOverrideId? id = null)
  {
    ArgumentNullException.ThrowIfNull(issuedByParticipant);
    ArgumentNullException.ThrowIfNull(veto);

    if (issuedByParticipant.ParticipantId.HasNoValue)
    {
      return Result.Failure<VetoOverride>(VetoOverrideErrors.IssuedByMustBeProvided);
    }

    if (veto is null)
    {
      return Result.Failure<VetoOverride>(VetoOverrideErrors.VetoMustBeProvided);
    }

    var vetoOverride = new VetoOverride(
      id: id,
      veto: veto,
    issuedByParticipant: issuedByParticipant);

    vetoOverride.Raise(new VetoOverrideCreatedDomainEvent(
      vetoOverride.Id.Value,
      veto.Id.Value,
      issuedByParticipant.ParticipantId));

    return vetoOverride;
  }

  internal static Result<VetoOverride> SeedCreate(
      Veto veto,
      DraftPartParticipant issuedByParticipant,
      VetoOverrideId? id = null)
  {
    var vetoOverride = new VetoOverride(
      id: id,
      veto: veto,
    issuedByParticipant: issuedByParticipant);
    return vetoOverride;
  }
}
