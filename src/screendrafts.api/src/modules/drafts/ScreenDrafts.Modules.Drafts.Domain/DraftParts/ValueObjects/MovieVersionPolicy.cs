namespace ScreenDrafts.Modules.Drafts.Domain.DraftParts.ValueObjects;

public sealed record MovieVersionPolicy
{
  private readonly HashSet<string> _allowed;

  private MovieVersionPolicy(
    bool allowsFreeformPickVersion,
    bool requiresPickLevelVersion,
    IEnumerable<string>? allowedVersions)
  {
    AllowsFreeformPickVersion = allowsFreeformPickVersion;
    RequiresPickLevelVersion = requiresPickLevelVersion;

    _allowed = new HashSet<string>(
      (allowedVersions ?? [])
        .Where(v => !string.IsNullOrWhiteSpace(v))
        .Select(v => v.Trim()),
      StringComparer.OrdinalIgnoreCase);
  }

  /// <summary>
  /// If true, pick-level versions may be any string (still capped at 100).
  /// If false, and the Movie has defined versions, the pick must match one.
  /// </summary>
  public bool AllowsFreeformPickVersion { get; }

  /// <summary>
  /// If true, a pick must specify MovieVersionName (rare; not your current real-world default).
  /// </summary>
  public bool RequiresPickLevelVersion { get; }

  /// <summary>
  /// Optional allow-list gate. If set, the pick version must be in this list (case-insensitive).
  /// </summary>
  public IReadOnlyCollection<string> AllowedVersions => _allowed;

  public static MovieVersionPolicy DefaultTheatrical() =>
    new(
      allowsFreeformPickVersion: false,
      requiresPickLevelVersion: false,
      allowedVersions: null);

  public static MovieVersionPolicy AllowFreeform() =>
    new(
      allowsFreeformPickVersion: true,
      requiresPickLevelVersion: false,
      allowedVersions: null);

  public static MovieVersionPolicy RequireFromAllowList(IEnumerable<string> allowedVersions) =>
    new(
      allowsFreeformPickVersion: false,
      requiresPickLevelVersion: false,
      allowedVersions: allowedVersions);

  public static MovieVersionPolicy RequirePickLevelFromAllowList(IEnumerable<string> allowedVersions) =>
    new(
      allowsFreeformPickVersion: false,
      requiresPickLevelVersion: true,
      allowedVersions: allowedVersions);

  public Result Validate(string pickVersionName)
  {
    if (_allowed.Count == 0)
    {
      return Result.Success();
    }

    if (_allowed.Contains(pickVersionName))
    {
      return Result.Success();
    }

    return Result.Failure(MovieErrors.VersionNotAllowedByPolicy);
  }
}
