namespace ScreenDrafts.Modules.GuestDrafts.Domain.GuestDrafterTeams;

/// <summary>
/// Mirrors canonical DrafterTeam exactly. Shape built now for consistency with
/// the rest of the participant model; team-management commands (create team,
/// add/remove members) are still deferred -- nothing in Features constructs one
/// of these yet.
/// </summary>
public sealed class GuestDrafterTeam : Entity<GuestDrafterTeamId>
{
  public const int TeamNameMaxLength = 100;

  private readonly List<GuestDrafter> _drafters = [];

  private GuestDrafterTeam(string name, string publicId, GuestDrafterTeamId? id = null)
    : base(id ?? GuestDrafterTeamId.CreateUnique())
  {
    Name = name;
    PublicId = publicId;
  }

  private GuestDrafterTeam() { }

  public string Name { get; private set; } = default!;
  public string PublicId { get; private set; } = default!;
  public int NumberOfDrafters => _drafters.Count;

  public IReadOnlyCollection<GuestDrafter> Drafters => _drafters.AsReadOnly();

  public static Result<GuestDrafterTeam> Create(
    string name,
    string publicId,
    GuestDrafterTeamId? id = null
  )
  {
    if (string.IsNullOrWhiteSpace(name))
    {
      return Result.Failure<GuestDrafterTeam>(GuestDrafterTeamErrors.InvalidName);
    }

    if (string.IsNullOrWhiteSpace(publicId))
    {
      return Result.Failure<GuestDrafterTeam>(GuestDrafterTeamErrors.InvalidPublicId);
    }

    return new GuestDrafterTeam(name, publicId, id);
  }

  public Result AddDrafter(GuestDrafter drafter)
  {
    ArgumentNullException.ThrowIfNull(drafter);

    if (_drafters.Any(x => x.Id == drafter.Id))
    {
      return Result.Failure(GuestDrafterErrors.AlreadyAdded(drafter.Id.Value));
    }

    _drafters.Add(drafter);
    return Result.Success();
  }

  public Result RemoveDrafter(GuestDrafter drafter)
  {
    ArgumentNullException.ThrowIfNull(drafter);

    if (!_drafters.Contains(drafter))
    {
      return Result.Failure(GuestDrafterErrors.NotFound(drafter.Id.Value));
    }

    if (_drafters.Count == 1)
    {
      return Result.Failure(GuestDrafterTeamErrors.NotEnoughDrafters);
    }

    _drafters.Remove(drafter);
    return Result.Success();
  }

  public Result UpdateName(string name)
  {
    if (string.IsNullOrWhiteSpace(name))
    {
      return Result.Failure(GuestDrafterTeamErrors.InvalidName);
    }

    Name = name;
    return Result.Success();
  }
}
