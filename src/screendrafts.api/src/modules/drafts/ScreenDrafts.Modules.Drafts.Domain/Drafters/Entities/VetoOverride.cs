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


  public static VetoOverride Create(
    Veto veto,
    Drafter? drafter,
    DrafterTeam? drafterTeam,
    VetoOverrideId? id = null)
  {
    ArgumentNullException.ThrowIfNull(veto);

    return new VetoOverride(
      id: id ?? VetoOverrideId.CreateUnique(),
      veto: veto,
      drafter: drafter,
      drafterTeam: drafterTeam);
  }
}
