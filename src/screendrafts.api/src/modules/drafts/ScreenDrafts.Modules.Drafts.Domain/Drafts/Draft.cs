namespace ScreenDrafts.Modules.Drafts.Domain.Drafts;

public sealed class Draft : AggrgateRoot<DraftId, Guid>
{
  private readonly List<DraftPart> _parts = [];
  private readonly List<Campaign> _campaigns = [];
  private readonly List<DraftCategory> _draftCategories = [];

  private Draft(
  DraftId id,
  Title title,
  DraftType draftType,
  DateTime createdAtUtc)
  : base(id)
  {
    Title = title;
    DraftType = draftType;
    CreatedAtUtc = createdAtUtc;
  }

  private Draft()
  {
  }

  public int ReadableId { get; init; }

  public Title Title { get; private set; } = default!;

  public DraftType DraftType { get; private set; } = default!;

  public DraftStatus DraftStatus => DeriveDraftStatus();

  public DateTime CreatedAtUtc { get; private set; }

  public DateTime? UpdatedAtUtc { get; private set; }

  public bool IsScreamDrafts => _campaigns.Any(c => c.Slug == CampaignSlugs.ScreamDrafts);

  public string? Description { get; private set; }

  public SeriesId? SeriesId { get; private set; } = default!;

  public Series? Series { get; private set; } = default!;

  // Relationships
  public IReadOnlyCollection<DraftPart> Parts => _parts.AsReadOnly();

  public IReadOnlyCollection<Campaign> Campaigns => _campaigns.AsReadOnly();

  public IReadOnlyCollection<DraftCategory> DraftCategories => _draftCategories.AsReadOnly();

  public static Result<Draft> Create(
  Title title,
  DraftType draftType,
  DraftId? id = null)
  {
    var draft = new Draft(
      title: title,
      draftType: draftType,
      createdAtUtc: DateTime.UtcNow,
      id: id ?? DraftId.CreateUnique());

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
    Guard.Against.Null(campaign);
    if (_campaigns.Any(c => c.Id == campaign.Id))
    {
      return;
    }
    _campaigns.Add(campaign);
  }

  public void RemoveCampaign(Campaign campaign)
  {
    Guard.Against.Null(campaign);
    if (!_campaigns.Any(c => c.Id == campaign.Id))
    {
      return;
    }
    _campaigns.Remove(campaign);
  }

  public Result<DraftPart> AddPart(int partIndex, int totalPicks, int totalDrafters, int totalDrafterTeams, int totalHosts)
  {
    if (partIndex <= 0)
    {
      return Result.Failure<DraftPart>(DraftErrors.PartIndexMustBeGreaterThanZero);
    }

    if (_parts.Any(p => p.PartIndex == partIndex))
    {
      return Result.Failure<DraftPart>(DraftErrors.DraftPartWithIndexAlreadyExists(partIndex));
    }

    var part = DraftPart.Create(
      draft: this,
      partIndex: partIndex,
      totalPicks: totalPicks,
      totalDrafters: totalDrafters,
      totalDrafterTeams: totalDrafterTeams,
      totalHosts: totalHosts).Value;
    _parts.Add(part);
    Raise(new DraftPartAddedDomainEvent(Id.Value, part.Id.Value));
    return part;
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

  public DraftStatus DeriveDraftStatus()
  {
    if (_parts.All(p => p.DraftStatus == DraftStatus.Completed))
    {
      return DraftStatus.Completed;
    }

    if (_parts.All(p => p.DraftStatus == DraftStatus.Paused))
    {
      return DraftStatus.Paused;
    }

    if (_parts.All(p => p.DraftStatus == DraftStatus.Created))
    {
      return DraftStatus.Created;
    }

    return DraftStatus.InProgress;
  }
}
