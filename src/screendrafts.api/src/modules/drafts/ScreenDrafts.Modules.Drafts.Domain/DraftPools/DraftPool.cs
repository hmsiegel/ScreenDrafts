namespace ScreenDrafts.Modules.Drafts.Domain.DraftPools;

public sealed class DraftPool : AggregateRoot<DraftPoolId, Guid>
{
  private readonly List<DraftPoolItem> _tmdbIds = [];

  private DraftPool(
    DraftId draftId,
    string publicId,
    DraftPoolId? id = null)
    : base(id ?? DraftPoolId.CreateUnique())
  {
    DraftId = draftId;
    PublicId = publicId;
    IsLocked = false;
    CreatedAtUtc = DateTime.UtcNow;
  }

  private DraftPool()
  {

  }

  public DraftId DraftId { get; private set; } = default!;
  public string PublicId { get; private set; } = default!;
  public bool IsLocked { get; private set; }
  public DateTime CreatedAtUtc { get; private set; }
  public DateTime? UpdatedAtUtc { get; private set; }

  public IReadOnlyList<DraftPoolItem> TmdbIds => _tmdbIds.AsReadOnly();

  public static Result<DraftPool> Create(
    DraftId draftId,
    string publicId)
  {
    ArgumentNullException.ThrowIfNull(draftId);

    if (string.IsNullOrWhiteSpace(publicId))
    {
      return Result.Failure<DraftPool>(DraftPoolErrors.InvalidPublicId);
    }

    var draftPool = new DraftPool(draftId, publicId);

    draftPool.Raise(new DraftPoolCreatedDomainEvent(draftPool.Id.Value, draftId.Value));

    return Result.Success(draftPool);
  }

  public Result AddMovie(int tmdbId)
  {
    if (IsLocked)
    {
      return Result.Failure(DraftPoolErrors.PoolIsLocked);
    }

    if (_tmdbIds.Any(x => x.TmdbId == tmdbId))
    {
      return Result.Failure(DraftPoolErrors.MovieAlreadyExists(tmdbId));
    }

    _tmdbIds.Add(DraftPoolItem.Create(tmdbId));

    UpdatedAtUtc = DateTime.UtcNow;

    Raise(new MovieAddedToDraftPoolDomainEvent(Id.Value, DraftId.Value, tmdbId));

    return Result.Success();
  }

  public Result RestoreMovie(int tmdbId)
  {
    if (_tmdbIds.Any(x => x.TmdbId == tmdbId))
    {
      return Result.Success();
    }

    _tmdbIds.Add(DraftPoolItem.Create(tmdbId));

    UpdatedAtUtc = DateTime.UtcNow;

    return Result.Success();
  }

  public Result RemoveMovie(int tmdbId)
  {
    var item = _tmdbIds.FirstOrDefault(x => x.TmdbId == tmdbId);

    if (item is not null)
    {
      _tmdbIds.Remove(item);
    }

    UpdatedAtUtc = DateTime.UtcNow;

    return Result.Success();
  }

  public Result Lock()
  {
    if (IsLocked)
    {
      return Result.Success();
    }

    IsLocked = true;
    UpdatedAtUtc = DateTime.UtcNow;
    Raise(new DraftPoolLockedDomainEvent(Id.Value, DraftId.Value));
    return Result.Success();
  }
}
