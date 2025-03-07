namespace ScreenDrafts.Modules.Drafts.Domain.Drafters.Entities;

public sealed class Veto : Entity<VetoId>
{
  private Veto(
    VetoId id,
    Pick pick)
    : base(id)
  {
    IsUsed = true;
    Pick = pick;
  }

  private Veto()
  {
  }

  public VetoOverride VetoOverride { get; private set; } = default!;

  public Pick Pick { get; private set; } = default!;

  public bool IsUsed { get; private set; }

  public static Result<Veto> Create(
    Pick pick,
    VetoId? id = null)
  {
    return new Veto(
      pick: pick,
      id: id ?? VetoId.CreateUnique());
  }
}
