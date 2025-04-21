namespace ScreenDrafts.Modules.Drafts.Domain.Drafters;

public sealed class DrafterTeam : Entity<DrafterTeamId>
{
  private readonly List<Drafter> _drafters = [];
  private readonly List<Draft> _drafts = [];
  private readonly List<Veto> _vetoes = [];
  private readonly List<VetoOverride> _vetoOverrides = [];
  private readonly List<DrafterDraftStats> _draftStats = [];
  private readonly List<Pick> _picks = [];

  private DrafterTeam(
    string name,
    DrafterTeamId? id = null)
    : base(id ?? DrafterTeamId.CreateUnique())
  {
    Name = Guard.Against.NullOrEmpty(name);
  }

  private DrafterTeam()
  {
  }

  public string Name { get; private set; } = default!;
  public int NumberOfDrafters { get; private set; } = 2;

  public RolloverVeto? RolloverVeto { get; private set; } = default!;

  public RolloverVetoOverride? RolloverVetoOverride { get; private set; } = default!;

  public IReadOnlyCollection<Drafter> Drafters => _drafters.AsReadOnly();

  public IReadOnlyCollection<Veto> Vetoes => _vetoes.AsReadOnly();

  public IReadOnlyCollection<VetoOverride> VetoOverrides => _vetoOverrides.AsReadOnly();

  public IReadOnlyCollection<DrafterDraftStats> DraftStats => _draftStats.AsReadOnly();

  public IReadOnlyCollection<Pick> Picks => _picks.AsReadOnly();

  public IReadOnlyCollection<Draft> Drafts => _drafts.AsReadOnly();

  public static Result<DrafterTeam> Create(
    string name,
    DrafterTeamId? id = null)
  {
    if (string.IsNullOrWhiteSpace(name))
    {
      return Result.Failure<DrafterTeam>(DrafterTeamErrors.InvalidName);
    }

    var drafterTeam = new DrafterTeam(
      id: id,
      name: name);
    drafterTeam.Raise(new DrafterTeamCreatedDomainEvent(drafterTeam.Id.Value));
    return drafterTeam;
  }

  public Result AddDrafter(Drafter drafter)
  {
    Guard.Against.Null(drafter);

    if (_drafters.Any(x => x.Id == drafter.Id))
    {
      return Result.Failure(DrafterErrors.AlreadyAdded(drafter.Id.Value));
    }

    _drafters.Add(drafter);
    return Result.Success();
  }

  public Result AddDraft(Draft draft)
  {
    Guard.Against.Null(draft);

    if (_drafts.Any(x => x.Id == draft.Id))
    {
      return Result.Failure(DraftErrors.AlreadyAdded(draft.Id.Value));
    }

    _drafts.Add(draft);
    return Result.Success();
  }

  public Result AddVeto(Veto veto)
  {
    Guard.Against.Null(veto);
    _vetoes.Add(veto);
    return Result.Success();
  }

  public Result AddVetoOverride(VetoOverride vetoOverride)
  {
    Guard.Against.Null(vetoOverride);
    _vetoOverrides.Add(vetoOverride);
    return Result.Success();
  }

  public Result AddDrafterDraftStats(DrafterDraftStats drafterDraftStats)
  {
    Guard.Against.Null(drafterDraftStats);
    _draftStats.Add(drafterDraftStats);
    return Result.Success();
  }

  public Result AddPick(Pick pick)
  {
    Guard.Against.Null(pick);
    _picks.Add(pick);
    return Result.Success();
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

  public Result RemoveDrafter(Drafter drafter)
  {
    Guard.Against.Null(drafter);

    if (!_drafters.Contains(drafter))
    {
      return Result.Failure(DrafterErrors.NotFound(drafter.Id.Value));
    }

    if (_drafters.Count == 1)
    {
      return Result.Failure(DrafterTeamErrors.NotEnoughDrafters);
    }

    _drafters.Remove(drafter);
    return Result.Success();
  }

  public Result RemoveVeto(Veto veto)
  {
    Guard.Against.Null(veto);
    _vetoes.Remove(veto);
    return Result.Success();
  }

  public Result RemoveVetoOverride(VetoOverride vetoOverride)
  {
    Guard.Against.Null(vetoOverride);
    _vetoOverrides.Remove(vetoOverride);
    return Result.Success();
  }

  public Result RemoveDrafterDraftStats(DrafterDraftStats drafterDraftStats)
  {
    Guard.Against.Null(drafterDraftStats);
    _draftStats.Remove(drafterDraftStats);
    return Result.Success();
  }

  public Result RemovePick(Pick pick)
  {
    Guard.Against.Null(pick);
    _picks.Remove(pick);
    return Result.Success();
  }

  public Result UpdateName(string name)
  {
    if (string.IsNullOrWhiteSpace(name))
    {
      return Result.Failure(DrafterTeamErrors.InvalidName);
    }

    Name = name;

    return Result.Success();
  }

  public Result UpdateDrafter(Drafter drafter)
  {
    Guard.Against.Null(drafter);
    var existingDrafter = _drafters.FirstOrDefault(x => x.Id == drafter.Id);
    if (existingDrafter is null)
    {
      return Result.Failure(DrafterErrors.NotFound(existingDrafter!.Id.Value));
    }
    _drafters.Remove(existingDrafter);
    _drafters.Add(drafter);
    return Result.Success();
  }

  public Result UpdateNumberOfDrafters(int numberOfDrafters)
  {
    if (numberOfDrafters < 1)
    {
      return Result.Failure(DrafterTeamErrors.InvalidNumberOfDrafters);
    }
    NumberOfDrafters = numberOfDrafters;
    return Result.Success();
  }
}
