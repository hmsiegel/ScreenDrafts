namespace ScreenDrafts.Modules.Drafts.Domain.Drafters;

public sealed class Drafter : AggrgateRoot<DrafterId, Guid>
{
  private readonly List<Veto> _vetoes = [];
  private readonly List<VetoOverride> _vetoOverrides = [];
  private readonly List<DrafterDraftStats> _draftStats = [];
  private readonly List<Draft> _drafts = [];
  private readonly List<Pick> _picks = [];


  private Drafter(
    string name,
    DrafterId? id = null,
    Guid? userId = null)
    : base(id ?? DrafterId.CreateUnique())
  {
    UserId = userId;
    Name = Guard.Against.NullOrEmpty(name);
  }

  private Drafter()
  {
  }

  public int ReadableId { get; init; }

  public Guid? UserId { get; private set; }

  public string Name { get; private set; } = default!;


  public RolloverVeto? RolloverVeto { get; private set; } = default!;

  public RolloverVetoOverride? RolloverVetoOverride { get; private set; } = default!;

  public IReadOnlyCollection<Veto> Vetoes => _vetoes.AsReadOnly();

  public IReadOnlyCollection<VetoOverride> VetoOverrides => _vetoOverrides.AsReadOnly();

  public IReadOnlyCollection<DrafterDraftStats> DraftStats => _draftStats.AsReadOnly();

  public IReadOnlyCollection<Draft> Drafts => _drafts.AsReadOnly();

  public IReadOnlyCollection<Pick> Picks => _picks.AsReadOnly();

  public static Result<Drafter> Create(
    string name,
    Guid? userId = null,
    DrafterId? id = null)
  {
    var drafter = new Drafter(
      id: id,
      userId: userId,
      name: name);

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
    _draftStats.Add(DrafterDraftStats.Create(this, draft));
  }

  public void SetDrafterName(string firstName, string lastName, string? middleName)
  {
    Name = $"{firstName} {middleName} {lastName}";
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
