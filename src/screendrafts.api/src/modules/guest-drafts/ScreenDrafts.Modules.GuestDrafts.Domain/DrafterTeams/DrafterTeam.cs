namespace ScreenDrafts.Modules.GuestDrafts.Domain.DrafterTeams;

/// <summary>
/// Mirrors canonical DrafterTeam exactly. Shape built now for consistency with
/// the rest of the participant model; team-management commands (create team,
/// add/remove members) are still deferred -- nothing in Features constructs one
/// of these yet.
/// </summary>
public sealed class DrafterTeam : Entity<DrafterTeamId>
{
  public const int TeamNameMaxLength = 100;

  private readonly List<Drafter> _drafters = [];

  private DrafterTeam(string name, string publicId, DrafterTeamId? id = null)
    : base(id ?? DrafterTeamId.CreateUnique())
  {
    Name = name;
    PublicId = publicId;
  }

  private DrafterTeam() { }

  public string Name { get; private set; } = default!;
  public string PublicId { get; private set; } = default!;
  public int NumberOfDrafters => _drafters.Count;

  public IReadOnlyCollection<Drafter> Drafters => _drafters.AsReadOnly();

  public static Result<DrafterTeam> Create(string name, string publicId, DrafterTeamId? id = null)
  {
    if (string.IsNullOrWhiteSpace(name))
    {
      return Result.Failure<DrafterTeam>(DrafterTeamErrors.InvalidName);
    }

    if (string.IsNullOrWhiteSpace(publicId))
    {
      return Result.Failure<DrafterTeam>(DrafterTeamErrors.InvalidPublicId);
    }

    return new DrafterTeam(name, publicId, id);
  }

  public Result AddDrafter(Drafter drafter)
  {
    ArgumentNullException.ThrowIfNull(drafter);

    if (_drafters.Any(x => x.Id == drafter.Id))
    {
      return Result.Failure(DrafterErrors.AlreadyAdded(drafter.Id.Value));
    }

    _drafters.Add(drafter);
    return Result.Success();
  }

  public Result RemoveDrafter(Drafter drafter)
  {
    ArgumentNullException.ThrowIfNull(drafter);

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
}
