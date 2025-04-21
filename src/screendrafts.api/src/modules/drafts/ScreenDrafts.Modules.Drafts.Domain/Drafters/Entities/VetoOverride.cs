namespace ScreenDrafts.Modules.Drafts.Domain.Drafters.Entities;

public sealed class VetoOverride : Entity<VetoOverrideId>
{
  private VetoOverride(
    VetoOverrideId id,
    Veto veto,
    Drafter? drafter,
    DrafterTeam? drafterTeam)
    : base(id)
  {
    Veto = veto;
    VetoId = veto.Id;

    Drafter = drafter;
    DrafterId = drafter?.Id;

    DrafterTeam = drafterTeam;
    DrafterTeamId = drafterTeam?.Id;
  }

  private VetoOverride()
  {
  }

  public VetoId VetoId { get; private set; } = default!;
  public Veto Veto { get; private set; } = default!;

  public DrafterId? DrafterId { get; private set; } = default!;
  public Drafter? Drafter { get; private set; } = default!;

  public DrafterTeam? DrafterTeam { get; private set; } = default!;
  public DrafterTeamId? DrafterTeamId { get; private set; } = default!;


  public static Result<VetoOverride> Create(
    Veto veto,
    Drafter? drafter,
    DrafterTeam? drafterTeam,
    VetoOverrideId? id = null)
  {
    if (drafter is null && drafterTeam is null)
    {
      return Result.Failure<VetoOverride>(VetoOverrideErrors.DrafterOrTeamMustBeProvided);
    }

    if (veto is null)
    {
      return Result.Failure<VetoOverride>(VetoOverrideErrors.VetoMustBeProvided);
    }

    if (drafter is not null && drafterTeam is not null)
    {
      return Result.Failure<VetoOverride>(VetoOverrideErrors.DrafterAndTeamCannotBeProvided);
    }

    ArgumentNullException.ThrowIfNull(veto);

    var vetoOverride = new VetoOverride(
      id: id ?? VetoOverrideId.CreateUnique(),
      veto: veto,
      drafter: drafter,
      drafterTeam: drafterTeam);

    vetoOverride.Raise(new VetoOverrideCreatedDomainEvent(
      vetoOverride.Id.Value,
      drafter?.Id.Value,
      drafterTeam?.Id.Value,
      veto.Id.Value));

    return vetoOverride;
  }
}
