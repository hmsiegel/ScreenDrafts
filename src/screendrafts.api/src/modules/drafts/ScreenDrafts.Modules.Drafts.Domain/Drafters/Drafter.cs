using ScreenDrafts.Modules.Drafts.Domain.Drafters.Errors;
using ScreenDrafts.Modules.Drafts.Domain.Drafts;

namespace ScreenDrafts.Modules.Drafts.Domain.Drafters;

public sealed class Drafter : AggrgateRoot<DrafterId, Guid>
{
  private readonly List<Veto> _vetoes = [];
  private readonly List<VetoOverride> _vetoOverrides = [];
  private readonly List<DrafterDraftStats> _draftStats = [];


  private Drafter(
    DrafterId id,
    Guid userId,
    string name)
    : base(id)
  {
    UserId = userId;
    Name = Guard.Against.NullOrEmpty(name);
  }

  private Drafter()
  {
  }

  public int ReadableId { get; init; }

  public Guid UserId { get; private set; }

  public string Name { get; private set; } = default!;


  public Pick Pick { get; private set; } = default!;

  public RolloverVeto? RolloverVeto { get; private set; } = default!;

  public RolloverVetoOverride? RolloverVetoOverride { get; private set; } = default!;

  public IReadOnlyCollection<Veto> Vetoes => _vetoes.AsReadOnly();

  public IReadOnlyCollection<VetoOverride> VetoOverrides => _vetoOverrides.AsReadOnly();

  public IReadOnlyCollection<DrafterDraftStats> DraftStats => _draftStats.AsReadOnly();

  public static Result<Drafter> Create(
    string name,
    Guid userId,
    DrafterId? id = null)
  {
    Guard.Against.Null(id);

    var drafter = new Drafter(
      id: id ?? DrafterId.CreateUnique(),
      userId: userId,
      name: name);

    drafter.Raise(new DrafterCreatedDomainEvent(drafter.Id.Value));

    return drafter;
  }

  public void AddVeto(Pick pick)
  {
    _vetoes.Add(Veto.Create(pick));
  }

  public void AddVetoOverride(Veto veto)
  {
    _vetoOverrides.Add(VetoOverride.Create(veto));
  }

  public Result SetRolloverVeto()
  {
    if (RolloverVeto != null)
    {
      return Result.Failure(DrafterErrors.RolloverVetoAlreadyExists);
    }

    return Result.Success();
  }

  public Result SetRolloverVetoOverride()
  {
    if (RolloverVetoOverride != null)
    {
      return Result.Failure(DrafterErrors.RolloverVetoOverrideAlreadyExists);
    }
    return Result.Success();
  }

  public void AddDraftStats(Draft draft)
  {
    _draftStats.Add(DrafterDraftStats.Create(this, draft));
  }

  public void SetDrafterName(string firstName, string lastName, string? middleName)
  {
    Name = $"{firstName} {middleName} {lastName}";
  }
}
