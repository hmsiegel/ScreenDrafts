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

  public RolloverVeto? RolloverVeto { get; private set; } = default!;

  public RolloverVetoOverride? RolloverVetoOverride { get; private set; } = default!;

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

  public void AddVeto(Guid pickId)
  {
    Guard.Against.Null(pickId);
    _vetoes.Add(Veto.Create(Id.Value, pickId));
  }

  public void AddVetoOverride(Guid vetoId)
  {
    Guard.Against.Null(vetoId);
    _vetoOverrides.Add(VetoOverride.Create(Id.Value, vetoId));
  }

  public Result SetRolloverVeto()
  {
    if (RolloverVeto != null)
    {
      return Result.Failure(DrafterErrors.RolloverVetoAlreadyExists);
    }

    RolloverVeto = RolloverVeto.Create(Id.Value, DraftId);

    return Result.Success();
  }

  public Result SetRolloverVetoOverride()
  {
    if (RolloverVetoOverride != null)
    {
      return Result.Failure(DrafterErrors.RolloverVetoOverrideAlreadyExists);
    }
    RolloverVetoOverride = RolloverVetoOverride.Create(Id.Value, DraftId);

    return Result.Success();
  }
}
