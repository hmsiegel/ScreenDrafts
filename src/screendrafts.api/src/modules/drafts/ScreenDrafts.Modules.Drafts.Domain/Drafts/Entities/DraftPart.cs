namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.Entities;

public sealed class DraftPart : Entity<DraftPartId>
{
  private readonly List<DraftRelease> _releases = [];

  private DraftPart(
    Draft draft,
    int partIndex,
    DraftPartId? id = null)
    : base(id ?? DraftPartId.CreateUnique())
  {
    DraftId = draft.Id;
    Draft = draft;
    PartIndex = partIndex;
  }

  private DraftPart()
  {
    // For EF
  }

  public DraftId DraftId { get; private set; } = default!;

  public Draft Draft { get; private set; } = default!;

  public int PartIndex { get; private set; }

  public IReadOnlyCollection<DraftRelease> Releases => _releases.AsReadOnly();

  public static Result<DraftPart> Create(
    Draft draft,
    int partIndex)
  {
    if (draft == null)
    {
      return Result.Failure<DraftPart>(DraftErrors.DraftIsRequired);
    }

    if (partIndex < 1)
    {
      return Result.Failure<DraftPart>(DraftErrors.PartIndexMustBeGreaterThanZero);
    }

    var draftPart = new DraftPart(
      draft: draft,
      partIndex: partIndex);

    return Result.Success(draftPart);
  }

  public DraftRelease AddRelease(ReleaseChannel channel, DateOnly date)
  {
    var release = DraftRelease.Create(Id, channel, date).Value;
    _releases.Add(release);
    return release;
  }
}
