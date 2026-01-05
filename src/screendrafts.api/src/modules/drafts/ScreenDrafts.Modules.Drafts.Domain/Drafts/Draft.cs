namespace ScreenDrafts.Modules.Drafts.Domain.Drafts;

public sealed partial class Draft : AggrgateRoot<DraftId, Guid>
{
  private readonly List<DraftPart> _parts = [];
  private readonly List<DraftCategory> _draftCategories = [];
  private readonly Dictionary<ParticipantId, DrafterVetoAccount> _accounts = [];

  private Draft(
  DraftId id,
  Title title,
  DraftType draftType,
  Series series,
  DateTime createdAtUtc)
  : base(id)
  {
    Title = title;
    DraftType = draftType;
    Series = series;
    SeriesId = series.Id;
    CreatedAtUtc = createdAtUtc;
  }

  private Draft()
  {
  }

  // Identity and Metadata
  public int ReadableId { get; init; }
  public Title Title { get; private set; } = default!;
  public string? Description { get; private set; }
  public DateTime CreatedAtUtc { get; private set; }
  public DateTime? UpdatedAtUtc { get; private set; }

  // Policy Snapshots
  public Series? Series { get; private set; } = default!;
  public SeriesId? SeriesId { get; private set; } = default!;
  public DraftType DraftType { get; private set; } = default!;

  public DraftStatus DraftStatus { get; private set; } = DraftStatus.Created;


  // Relationships
  public IReadOnlyCollection<DraftPart> Parts => _parts.AsReadOnly();
  public Campaign? Campaign { get; private set; } = default!;
  public IReadOnlyCollection<DraftCategory> DraftCategories => _draftCategories.AsReadOnly();

  // Rollups
  public int TotalParts => _parts.Count;
  public int TotalPicks => _parts.Sum(p => p.Picks.Count);
  public int TotalDrafters => _parts.Sum(p => p.TotalDrafters) + _parts.Sum(p => p.TotalDrafterTeams);
  public int TotalHosts => _parts.Sum(p => p.TotalHosts);



  public static Result<Draft> Create(
  Title title,
  DraftType draftType,
  Series series,
  DraftId? id = null)
  {
    ArgumentNullException.ThrowIfNull(series);

    var draft = new Draft(
      title: title,
      draftType: draftType,
      series: series,
      createdAtUtc: DateTime.UtcNow,
      id: id ?? DraftId.CreateUnique())
    {
      Series = series,
      SeriesId = series.Id
    };

    draft.Raise(new DraftCreatedDomainEvent(draft.Id.Value));

    return draft;
  }

  public Result EditDraft(
    Title title,
    DraftType draftType,
    string? description)
  {
    Guard.Against.Null(title);
    Guard.Against.Null(draftType);
    Guard.Against.NullOrWhiteSpace(description);

    Title = title;
    DraftType = draftType;
    UpdatedAtUtc = DateTime.UtcNow;
    Description = description;

    Raise(new DraftEditedDomainEvent(Id.Value, title.Value));

    return Result.Success();
  }

  // Categories
  public Result AddCategory(Category category)
  {
    Guard.Against.Null(category);

    if (_draftCategories.Any(dc => dc.CategoryId == category.Id))
    {
      return Result.Failure(CategoryErrors.CategoryAlreadyAdded(category.Id.Value));
    }

    var draftCategory = DraftCategory.Create(this, category);

    _draftCategories.Add(draftCategory);

    UpdatedAtUtc = DateTime.UtcNow;

    Raise(new CategoryAddedDomainEvent(Id.Value, category.Id.Value));
    return Result.Success();
  }

  public Result RemoveCategory(Category category)
  {
    Guard.Against.Null(category);

    if (!_draftCategories.Any(dc => dc.CategoryId == category.Id))
    {
      return Result.Failure(CategoryErrors.CannotRemoveACategoryThatIsNotAdded(category.Id.Value));
    }

    var draftCategory = _draftCategories.FirstOrDefault(dc => dc.CategoryId == category.Id) ?? null;

    if (draftCategory != null)
    {
      _draftCategories.Remove(draftCategory);
    }

    UpdatedAtUtc = DateTime.UtcNow;

    Raise(new CategoryRemovedDomainEvent(Id.Value, category.Id.Value));

    return Result.Success();
  }

  // Campaigns
  public void AddCampaign(Campaign campaign)
  {
    ArgumentNullException.ThrowIfNull(campaign);
    Campaign = campaign;
    UpdatedAtUtc = DateTime.UtcNow;
  }

  public void RemoveCampaign()
  {
    Campaign = null;
    UpdatedAtUtc = DateTime.UtcNow;
  }

  // Releases
  public Result<DraftRelease> AddRelease(DraftPart part, ReleaseChannel channel, DateOnly date)
  {
    ArgumentNullException.ThrowIfNull(part);

    if (part.DraftId != Id)
    {
      return Result.Failure<DraftRelease>(DraftErrors.DraftPartDoesNotBelongToThisDraft);
    }

    UpdatedAtUtc = DateTime.UtcNow;

    return part.AddRelease(channel, date);
  }

  // Series
  public Result<Series> LinkSeries(Series series)
  {
    Guard.Against.Null(series);

    if (SeriesId == series.Id)
    {
      return Result.Failure<Series>(DraftErrors.SeriesAlreadyLinked(SeriesId.Value));
    }

    Series = series;
    SeriesId = series.Id;
    UpdatedAtUtc = DateTime.UtcNow;

    Raise(new SeriesLinkedDomainEvent(Id.Value, series.Id.Value));
    return Result.Success(series);
  }

  public Result UnlinkSeries()
  {
    if (SeriesId == null)
    {
      return Result.Failure(DraftErrors.NoSeriesLinked);
    }

    var oldSeriesId = SeriesId.Value;
    Series = null;
    SeriesId = null;
    UpdatedAtUtc = DateTime.UtcNow;
    Raise(new SeriesUnlinkedDomainEvent(Id.Value, oldSeriesId));
    return Result.Success();
  }

  public void DeriveDraftStatus()
  {
    if (_parts.Any(p => p.Status == DraftPartStatus.InProgress))
    {
      DraftStatus = DraftStatus.InProgress;
      return;
    }

    if (_parts.Count > 0 && _parts.All(p => p.Status == DraftPartStatus.Completed))
    {
      DraftStatus = DraftStatus.Completed;
      return;
    }

    if (_parts[0].Status == DraftPartStatus.Completed && _parts.Skip(1).Any(p => p.Status == DraftPartStatus.Scheduled))
    {
      DraftStatus = DraftStatus.Paused;
      return;
    }


    if (_parts.All(p => p.Status == DraftPartStatus.Created))
    {
      DraftStatus = DraftStatus.Created;
    }

    DraftStatus = DraftStatus.Scheduled;
  }
}
