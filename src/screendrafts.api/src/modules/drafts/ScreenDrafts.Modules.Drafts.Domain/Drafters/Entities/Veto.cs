namespace ScreenDrafts.Modules.Drafts.Domain.Drafters.Entities;

public sealed class Veto : Entity<VetoId>
{
  private Veto(
    VetoId id,
    Pick pick,
    Drafter? drafter,
    DrafterTeam? drafterTeam)
    : base(id)
  {
    Pick = pick;
    PickId = pick.Id;

    Drafter = drafter;
    DrafterId = drafter?.Id;

    DrafterTeam = drafterTeam;
    DrafterTeamId = drafterTeam?.Id;
  }

  private Veto()
  {
  }

  public VetoOverride VetoOverride { get; private set; } = default!;

  public Pick Pick { get; private set; } = default!;
  public PickId PickId { get; private set; } = default!;

  public Drafter? Drafter { get; private set; } = default!;
  public DrafterId? DrafterId { get; private set; } = default!;

  public DrafterTeam? DrafterTeam { get; private set; } = default!;
  public DrafterTeamId? DrafterTeamId { get; private set; } = default!;

  public static Result<Veto> Create(
    Pick pick,
    Drafter? drafter,
    DrafterTeam? drafterTeam,
    VetoId? id = null)
  {
    ArgumentNullException.ThrowIfNull(pick);

    return new Veto(
      pick: pick,
      drafter: drafter,
      drafterTeam: drafterTeam,
      id: id ?? VetoId.CreateUnique());
  }
}
