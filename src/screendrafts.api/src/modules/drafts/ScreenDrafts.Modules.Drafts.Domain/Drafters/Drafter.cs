using ScreenDrafts.Modules.Drafts.Domain.Drafters.Entities;

namespace ScreenDrafts.Modules.Drafts.Domain.Drafters;

public sealed class Drafter : AggrgateRoot<DrafterId, Guid>
{
  private readonly List<Veto> _vetoes = [];
  private readonly List<VetoOverride> _vetoOverrides = [];


  private Drafter(
    DrafterId id,
    Guid userId,
    string name,
    Guid draftId)
    : base(id)
  {
    UserId = userId;
    Name = Guard.Against.NullOrEmpty(name);
    DraftId = draftId;
  }

  private Drafter()
  {
  }

  public int ReadableId { get; init; }

  public Guid UserId { get; private set; }

  public string Name { get; private set; } = default!;

  public Guid DraftId { get; private set; }

  public Draft Draft { get; private set; } = default!;

  public RolloverVeto RolloverVeto { get; private set; } = default!;

  public RolloverVetoOverride RolloverVetoOverride { get; private set; } = default!;

  public IReadOnlyCollection<Veto> Vetoes => _vetoes.AsReadOnly();

  public IReadOnlyCollection<VetoOverride> VetoOverrides => _vetoOverrides.AsReadOnly();

  public static Result<Drafter> Create(
    string name,
    Guid userId,
    Guid draftId,
    DrafterId? id = null)
  {
    Guard.Against.Null(id);

    var drafter = new Drafter(
      id: id ?? DrafterId.CreateUnique(),
      userId: userId,
      draftId: draftId,
      name: name);

    drafter.Raise(new DrafterCreatedDomainEvent(drafter.Id.Value));

    return drafter;
  }

  public void AddVeto(Veto veto)
  {
    Guard.Against.Null(veto);
    _vetoes.Add(veto);
  }

  public void AddVetoOverride(VetoOverride vetoOverride)
  {
    Guard.Against.Null(vetoOverride);
    _vetoOverrides.Add(vetoOverride);
  }

  public void SetRolloverVeto(RolloverVeto rolloverVeto)
  {
    Guard.Against.Null(rolloverVeto);
    RolloverVeto = rolloverVeto;
  }

  public void SetRolloverVetoOverride(RolloverVetoOverride rolloverVetoOverride)
  {
    Guard.Against.Null(rolloverVetoOverride);
    RolloverVetoOverride = rolloverVetoOverride;
  }
}
