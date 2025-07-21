namespace ScreenDrafts.Modules.Drafts.Domain.Drafters;

public sealed class Drafter : AggrgateRoot<DrafterId, Guid>
{
  private readonly List<Veto> _vetoes = [];
  private readonly List<VetoOverride> _vetoOverrides = [];
  private readonly List<DrafterDraftStats> _draftStats = [];
  private readonly List<Draft> _drafts = [];
  private readonly List<Pick> _picks = [];


  private Drafter(
    Person person,
    DrafterId? id = null)
    : base(id ?? DrafterId.CreateUnique())
  {
    Person = person;
    PersonId = person.Id;
  }

  private Drafter()
  {
  }

  public int ReadableId { get; init; }

  public Person Person { get; private set; } = default!;

  public PersonId PersonId { get; private set; } = default!;

  public RolloverVeto? RolloverVeto { get; private set; } = default!;

  public RolloverVetoOverride? RolloverVetoOverride { get; private set; } = default!;

  public IReadOnlyCollection<Veto> Vetoes => _vetoes.AsReadOnly();

  public IReadOnlyCollection<VetoOverride> VetoOverrides => _vetoOverrides.AsReadOnly();

  public IReadOnlyCollection<DrafterDraftStats> DraftStats => _draftStats.AsReadOnly();

  public IReadOnlyCollection<Draft> Drafts => _drafts.AsReadOnly();

  public IReadOnlyCollection<Pick> Picks => _picks.AsReadOnly();

  public static Result<Drafter> Create(
    Person person,
    DrafterId? id = null)
  {
    ArgumentNullException.ThrowIfNull(person);

    var drafter = new Drafter(
      id: id,
      person: person);

    drafter.Raise(new DrafterCreatedDomainEvent(drafter.Id.Value));

    return drafter;
  }

  public void AddVeto(Veto veto)
  {
    _vetoes.Add(veto);
  }

  public void AddVetoOverride(VetoOverride vetoOverride)
  {
    _vetoOverrides.Add(vetoOverride);
  }

  public Result SetRolloverVeto(RolloverVeto rolloverVeto)
  {
    if (RolloverVeto != null)
    {
      return Result.Failure(DrafterErrors.RolloverVetoAlreadyExists);
    }

    RolloverVeto = rolloverVeto;

    return Result.Success();
  }

  public Result SetRolloverVetoOverride(RolloverVetoOverride rolloverVetoOverride)
  {

    if (RolloverVetoOverride != null)
    {
      return Result.Failure(DrafterErrors.RolloverVetoOverrideAlreadyExists);
    }

    RolloverVetoOverride = rolloverVetoOverride; 

    return Result.Success();
  }

  public void AddDraftStats(Draft draft)
  {
    _draftStats.Add(DrafterDraftStats.Create(this, null, draft));
  }

  public void AddDraft(Draft draft)
  {
    _drafts.Add(draft);
  }

  public void AddPick(Pick pick)
  {
    _picks.Add(pick);
  }
}
