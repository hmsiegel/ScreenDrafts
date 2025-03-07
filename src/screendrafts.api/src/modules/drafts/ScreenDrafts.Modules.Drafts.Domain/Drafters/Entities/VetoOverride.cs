using ScreenDrafts.Modules.Drafts.Domain.Drafters.Errors;

namespace ScreenDrafts.Modules.Drafts.Domain.Drafters.Entities;

public sealed class VetoOverride : Entity<VetoOverrideId>
{
  private VetoOverride(
    VetoOverrideId id,
    Veto veto)
    : base(id)
  {
    IsUsed = true;
    Veto = veto;
  }

  private VetoOverride()
  {
  }

  public VetoId VetoId { get; private set; } = default!;

  public Veto Veto { get; private set; } = default!;

  public bool IsUsed { get; private set; }

  public static VetoOverride Create(
    Veto veto,
    VetoOverrideId? id = null)
  {
    return new VetoOverride(
      id: id ?? VetoOverrideId.CreateUnique(),
      veto: veto);
  }
}
