namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.Entities;

public sealed class Series : Entity<SeriesId>
{
  private Series(
    string name,
    CanonicalPolicy canonicalPolicy,
    ContinuityScope continuityScope,
    ContinuityDateRule continuityDateRule,
    SeriesKind kind,
    SeriesId? id = null)
    : base(id ?? SeriesId.CreateUnique())
  {
    Name = name;
    CanonicalPolicy = canonicalPolicy;
    ContinuityScope = continuityScope;
    ContinuityDateRule = continuityDateRule;
    Kind = kind;
  }

  private Series()
  {
    // For EF
  }

  public string Name { get; private set; } = default!;
  public SeriesKind Kind { get; private set; } = SeriesKind.Regular;

  // Eligibility canon & continuity controls
  public CanonicalPolicy CanonicalPolicy { get; private set; } = CanonicalPolicy.Always;
  public ContinuityScope ContinuityScope { get; private set; } = ContinuityScope.Global;
  public ContinuityDateRule ContinuityDateRule { get; private set; } = ContinuityDateRule.AnyChannelFirstRelease;

  // Format guidance
  public DraftType? RequiredDraftType { get; private set; } = default!;
  public DraftType? DefaultDraftType { get; private set; } = DraftType.Standard;

  public DraftTypeMask AllowedDraftTypes { get; private set; } = DraftTypeMask.Standard
    | DraftTypeMask.MiniMega
    | DraftTypeMask.Mega
    | DraftTypeMask.Super
    | DraftTypeMask.MiniSuper;


  public static Result<Series> Create(
    string name,
    CanonicalPolicy canonicalPolicy,
    ContinuityScope continuityScope,
    ContinuityDateRule continuityDateRule,
    SeriesKind kind)
  {
    if (string.IsNullOrWhiteSpace(name))
    {
      return Result.Failure<Series>(DraftErrors.SeriesNameIsRequired);
    }

    var series = new Series(
      name: name,
      canonicalPolicy: canonicalPolicy,
      continuityScope: continuityScope,
      continuityDateRule: continuityDateRule,
      kind: kind);

    return Result.Success(series);
  }

  public bool Allows(DraftType draftType)
  {
    ArgumentNullException.ThrowIfNull(draftType);

    return (AllowedDraftTypes & (DraftTypeMask)(1 << draftType.Value)) != 0 &&
     (RequiredDraftType is null || RequiredDraftType.Value == draftType);
  }
}
