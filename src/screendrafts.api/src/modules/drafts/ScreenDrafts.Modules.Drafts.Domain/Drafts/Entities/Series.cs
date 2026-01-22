namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.Entities;

public sealed class Series : Entity<SeriesId>
{
  public const int MaxNameLength = 200;

  private Series(
    string name,
    string publicId,
    CanonicalPolicy canonicalPolicy,
    ContinuityScope continuityScope,
    ContinuityDateRule continuityDateRule,
    DraftType? defaultDraftType,
    DraftTypeMask allowedDraftTypes,
    SeriesKind kind,
    SeriesId? id = null)
    : base(id ?? SeriesId.CreateUnique())
  {
    Name = name;
    PublicId = publicId;
    CanonicalPolicy = canonicalPolicy;
    ContinuityScope = continuityScope;
    ContinuityDateRule = continuityDateRule;
    DefaultDraftType = defaultDraftType;
    AllowedDraftTypes = allowedDraftTypes;
    Kind = kind;
  }

  private Series()
  {
    // For EF
  }

  public string Name { get; private set; } = default!;
  public string PublicId { get; private set; } = default!;
  public SeriesKind Kind { get; private set; } = SeriesKind.Regular;

  // Eligibility canon & continuity controls
  public CanonicalPolicy CanonicalPolicy { get; private set; } = CanonicalPolicy.Always;
  public ContinuityScope ContinuityScope { get; private set; } = ContinuityScope.Global;
  public ContinuityDateRule ContinuityDateRule { get; private set; } = ContinuityDateRule.AnyChannelFirstRelease;

  // Format guidance
  public DraftType? DefaultDraftType { get; private set; } = DraftType.Standard;

  public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;
  public DateTime? UpdatedAtUtc { get; private set; } = default!;

  public DraftTypeMask AllowedDraftTypes { get; private set; } = DraftTypeMask.All;


  public static Result<Series> Create(
    string name,
    string publicId,
    CanonicalPolicy canonicalPolicy,
    ContinuityScope continuityScope,
    ContinuityDateRule continuityDateRule,
    SeriesKind kind,
    DraftType? defaultDraftType,
    DraftTypeMask allowedDraftTypes)
  {
    if (string.IsNullOrWhiteSpace(name))
    {
      return Result.Failure<Series>(SeriesErrors.SeriesNameIsRequired);
    }

    if (allowedDraftTypes == DraftTypeMask.None)
    {
      return Result.Failure<Series>(SeriesErrors.AllowedDraftTypesCannotBeNone);
    }

    if (defaultDraftType is not null && !allowedDraftTypes.Allows(defaultDraftType))
    {
      return Result.Failure<Series>(SeriesErrors.DefaultDraftTypeMustBeIncludedInAllowedDraftTypes);
    }

    var series = new Series(
      name: name,
      publicId: publicId,
      canonicalPolicy: canonicalPolicy,
      continuityScope: continuityScope,
      continuityDateRule: continuityDateRule,
      kind: kind,
      defaultDraftType: defaultDraftType,
      allowedDraftTypes: allowedDraftTypes);

    return Result.Success(series);
  }

  public Result Rename(string newName)
  {
    if (string.IsNullOrWhiteSpace(newName))
    {
      return Result.Failure(SeriesErrors.SeriesNameIsRequired);
    }
    Name = newName;
    return Result.Success();
  }

  public Result UpdatePolicies(CanonicalPolicy canonicalPolicy,
    ContinuityScope continuityScope,
    ContinuityDateRule continuityDateRule,
    SeriesKind kind)
  {
    CanonicalPolicy = canonicalPolicy;
    ContinuityScope = continuityScope;
    ContinuityDateRule = continuityDateRule;
    Kind = kind;
    UpdatedAtUtc = DateTime.UtcNow;
    return Result.Success();
  }

  public bool HasRequiredDraftType() => AllowedDraftTypes.IsSingleFlag();

  public bool Allows(DraftType draftType)
  {
    ArgumentNullException.ThrowIfNull(draftType);

    return AllowedDraftTypes.HasFlag(ToMask(draftType));
  }

  private static DraftTypeMask ToMask(DraftType draftType) =>
    draftType.Value switch
    {
      0 => DraftTypeMask.Standard,
      1 => DraftTypeMask.MiniMega,
      2 => DraftTypeMask.Mega,
      3 => DraftTypeMask.Super,
      4 => DraftTypeMask.MiniSuper,
      5 => DraftTypeMask.SpeedDraft,
      _ => DraftTypeMask.None
    };

  public Result UpdateFormatRules(DraftTypeMask allowedDraftTypes, DraftType? defaultDraftType)
  {
    if (allowedDraftTypes == DraftTypeMask.None)
    {
      return Result.Failure(SeriesErrors.AllowedDraftTypesCannotBeNone);
    }

    if (defaultDraftType is not null && !allowedDraftTypes.Allows(defaultDraftType))
    {
      return Result.Failure(SeriesErrors.DefaultDraftTypeMustBeIncludedInAllowedDraftTypes);
    }

    AllowedDraftTypes = allowedDraftTypes;
    DefaultDraftType = defaultDraftType;
    UpdatedAtUtc = DateTime.UtcNow;
    return Result.Success();
  }
}
