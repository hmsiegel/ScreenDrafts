namespace ScreenDrafts.Modules.Drafts.Domain.Drafters;

public sealed class Drafter : AggrgateRoot<DrafterId, Guid>
{
  private readonly List<Veto> _vetoes = [];
  private readonly List<VetoOverride> _vetoOverrides = [];
  private readonly List<DrafterDraftStats> _draftStats = [];
  private readonly List<DraftPart> _draftParts = [];
  private readonly List<Pick> _picks = [];


  private Drafter(
    Person person,
    string publicId,
    DrafterId? id = null)
    : base(id ?? DrafterId.CreateUnique())
  {
    Person = person;
    PublicId = publicId;
    PersonId = person.Id;
  }

  private Drafter()
  {
  }

  public string PublicId { get; init; } = default!;

  public Person Person { get; private set; } = default!;

  public PersonId PersonId { get; private set; } = default!;

  public bool IsRetired { get; private set; } = default!;

  public DateTime? RetiredAtUtc { get; private set; } = default!;

  public RolloverVeto? RolloverVeto { get; private set; } = default!;

  public RolloverVetoOverride? RolloverVetoOverride { get; private set; } = default!;

  public IReadOnlyCollection<Veto> Vetoes => _vetoes.AsReadOnly();

  public IReadOnlyCollection<VetoOverride> VetoOverrides => _vetoOverrides.AsReadOnly();

  public IReadOnlyCollection<DrafterDraftStats> DraftStats => _draftStats.AsReadOnly();

  public IReadOnlyCollection<DraftPart> DraftParts => _draftParts.AsReadOnly();

  public IReadOnlyCollection<Pick> Picks => _picks.AsReadOnly();

  public static Result<Drafter> Create(
    Person person,
    string publicId,
    DrafterId? id = null)
  {
    ArgumentNullException.ThrowIfNull(person);

    var drafter = new Drafter(
      id: id,
      publicId: publicId,
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

  public Result AddDraftStats(DrafterDraftStats draftStats)
  {
    Guard.Against.Null(draftStats);
    _draftStats.Add(draftStats);
    return Result.Success();
  }

  public void AddDraftPart(DraftPart draftPart)
  {
    ArgumentNullException.ThrowIfNull(draftPart);
    _draftParts.Add(draftPart);
  }

  public void ClearDraftParts()
  {
    _draftParts.Clear();
  }

  public void ClearPicks()
  {
    _picks.Clear();
  }

  public void RemovePick(Pick pick)
  {
    _picks.Remove(pick);
  }

  public void RemoveDraftPart(DraftPart draftPart)
  {
    _draftParts.Remove(draftPart);
  }

  public void AddPick(Pick pick)
  {
    _picks.Add(pick);
  }

  public Result RetireDrafter()
  {
    if (IsRetired)
    {
      return Result.Failure(DrafterErrors.AlreadyRetired);
    }

    IsRetired = true;
    RetiredAtUtc = DateTime.UtcNow;

    return Result.Success();
  }
}
