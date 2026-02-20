namespace ScreenDrafts.Modules.Drafts.Domain.DrafterTeams;

public sealed class DrafterTeam : Entity<DrafterTeamId>
{
  private readonly List<Drafter> _drafters = [];

  public const int TeamNameMaxLength = 100;

  private DrafterTeam(
    string name,
    string publicId,
    DrafterTeamId? id = null)
    : base(id ?? DrafterTeamId.CreateUnique())
  {
    Name = Guard.Against.NullOrEmpty(name);
    PublicId = publicId;
  }

  private DrafterTeam()
  {
  }

  public string Name { get; private set; } = default!;
  public string PublicId { get; private set; } = default!;
  public int NumberOfDrafters { get; private set; } = 2;

  public IReadOnlyCollection<Drafter> Drafters => _drafters.AsReadOnly();

  public static Result<DrafterTeam> Create(
    string name,
    string publicId,
    DrafterTeamId? id = null)
  {
    if (string.IsNullOrWhiteSpace(name))
    {
      return Result.Failure<DrafterTeam>(DrafterTeamErrors.InvalidName);
    }

    if (string.IsNullOrWhiteSpace(publicId))
    {
      return Result.Failure<DrafterTeam>(DrafterTeamErrors.InvalidPublicId);
    }

    var drafterTeam = new DrafterTeam(
      id: id,
      name: name,
      publicId: publicId);
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
