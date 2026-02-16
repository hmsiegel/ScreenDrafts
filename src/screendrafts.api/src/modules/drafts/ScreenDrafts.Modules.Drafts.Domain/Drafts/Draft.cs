namespace ScreenDrafts.Modules.Drafts.Domain.Drafts;

using Series = Series;

public sealed partial class Draft : AggregateRoot<DraftId, Guid>
{
  private readonly List<DraftPart> _parts = [];
  private readonly List<DraftCategory> _draftCategories = [];
  private readonly List<DraftChannelRelease> _channelReleases = [];

  private Draft(
  DraftId id,
  string publicId,
  Title title,
  DraftType draftType,
  Series series,
  DateTime createdAtUtc)
  : base(id)
  {
    PublicId = publicId;
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
  public string PublicId { get; private set; } = default!;
  public Title Title { get; private set; } = default!;
  public string? Description { get; private set; }
  public DateTime CreatedAtUtc { get; private set; }
  public DateTime? UpdatedAtUtc { get; private set; }

  // Policy Snapshots
  public Series Series { get; private set; } = default!;
  public SeriesId SeriesId { get; private set; } = default!;
  public DraftType DraftType { get; private set; } = default!;

  public DraftStatus DraftStatus { get; private set; } = DraftStatus.Created;


  // Relationships
  public IReadOnlyCollection<DraftPart> Parts => _parts.AsReadOnly();
  public IReadOnlyCollection<DraftCategory> DraftCategories => _draftCategories.AsReadOnly();
  public IReadOnlyCollection<DraftChannelRelease> ChannelReleases => _channelReleases.AsReadOnly();

  public Guid? CampaignId { get; private set; }
  public Campaign? Campaign { get; private set; }

  public uint Version { get; private set; } = default!;

  // Rollups
  public int TotalParts => _parts.Count;
  public int TotalPicks => _parts.Sum(p => p.Picks.Count);
  public int TotalParticipants => _parts.Sum(p => p.TotalDrafters) + _parts.Sum(p => p.TotalDrafterTeams);
  public int TotalHosts => _parts.Sum(p => p.TotalHosts);



  public static Result<Draft> Create(
  Title title,
  string publicId,
  DraftType draftType,
  Series series,
  DraftId? id = null)
  {
    ArgumentNullException.ThrowIfNull(series);

    var draft = new Draft(
      id: id ?? DraftId.CreateUnique(),
      title: title,
      publicId: publicId,
      draftType: draftType,
      series: series,
      createdAtUtc: DateTime.UtcNow);

    draft.Raise(new DraftCreatedDomainEvent(draft.Id.Value));

    return draft;
  }

  public void Update(
    string? title,
    string? description,
    int draftTypeValue)
  {
    // Update the draft's title if provided
    if (!string.IsNullOrEmpty(title))
    {
      Title = new Title(title);
    }

    // Update the draft's description if provided
    if (description != null)
    {
      Description = description;
    }

    // Update the draft type
    DraftType = DraftType.FromValue(draftTypeValue);

    // Update the updated timestamp
    UpdatedAtUtc = DateTime.UtcNow;
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

  public void ClearDraftCategories()
  {
    _draftCategories.Clear();
    UpdatedAtUtc = DateTime.UtcNow;
  }

  public void ReplaceCategories(IReadOnlyList<Category> categories)
  {
    Guard.Against.Null(categories);

    var desiredIds = categories.Select(c => c.Id).ToHashSet();
    var existingIds = _draftCategories.Select(dc => dc.CategoryId).ToHashSet();

    _draftCategories.RemoveAll(dc => !desiredIds.Contains(dc.CategoryId));

    // Add new categories
    foreach (var category in categories)
    {
      if (existingIds.Contains(category.Id))
      {
        continue;
      }

      _draftCategories.Add(DraftCategory.Create(this, category));
    }

    UpdatedAtUtc = DateTime.UtcNow;
  }

  // Campaigns
  public Result SetCampaign(Campaign campaign)
  {
    ArgumentNullException.ThrowIfNull(campaign);
    Campaign = campaign;
    CampaignId = campaign.Id;
    UpdatedAtUtc = DateTime.UtcNow;
    return Result.Success();
  }

  public Result ClearCampaign()
  {
    Campaign = null;
    CampaignId = null;
    UpdatedAtUtc = DateTime.UtcNow;
    return Result.Success();
  }

  // Channel Releases
  public Result UpsertChannelRelease(ReleaseChannel channel, int? episodeNumber = null)
  {
    var existingRelease = _channelReleases.FirstOrDefault(cr => cr.ReleaseChannel == channel);

    if (existingRelease is null)
    {
      var createResult = DraftChannelRelease.Create(
        draftId: Id,
        seriesId: SeriesId,
        releaseChannel: channel,
        episodeNumber: episodeNumber);

      if (createResult.IsFailure)
      {
        return Result.Failure<DraftChannelRelease>(createResult.Errors);
      }

      _channelReleases.Add(createResult.Value);

      return Result.Success();
    }

    if (episodeNumber.HasValue)
    {
      var setEpisodeResult = existingRelease.SetEpisodeNumber(episodeNumber.Value);
      if (setEpisodeResult.IsFailure)
      {
        return Result.Failure<DraftChannelRelease>(setEpisodeResult.Errors);
      }
    }

    return Result.Success();
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

  public void DeriveDraftStatus(DateTime utcNow)
  {
    if (_parts.Count > 0 && _parts.All(p => p.Status == DraftPartStatus.Cancelled))
    {
      DraftStatus = DraftStatus.Cancelled;
      return;
    }

    if (_parts.Any(p => p.Status == DraftPartStatus.InProgress))
    {
      DraftStatus = DraftStatus.InProgress;
      return;
    }

    var anyCompleted = _parts.Any(p => p.Status == DraftPartStatus.Completed);
    var anyScheduled = _parts.Any(p => p.IsScheduled(utcNow));

    if (anyCompleted && anyScheduled)
    {
      DraftStatus = DraftStatus.Paused;
      return;
    }

    if (anyScheduled)
    {
      DraftStatus = DraftStatus.Created;
      return;
    }

    if (_parts.Count > 0 &&
        _parts.All(p => p.Status == DraftPartStatus.Completed ||
                          p.Status == DraftPartStatus.Cancelled))
    {
      DraftStatus = DraftStatus.Completed;
      return;
    }

    DraftStatus = DraftStatus.Created;

  }

  public DraftLifecycleView GetLifecycleView(DateTime utcNow)
  {
    if (DraftStatus == DraftStatus.InProgress)
    {
      return DraftLifecycleView.InProgress;
    }

    if (DraftStatus == DraftStatus.Paused)
    {
      return DraftLifecycleView.Paused;
    }

    if (DraftStatus == DraftStatus.Completed)
    {
      return DraftLifecycleView.Completed;
    }

    if (DraftStatus == DraftStatus.Cancelled)
    {
      return DraftLifecycleView.Cancelled;
    }

    return _parts.Any(p => p.IsScheduled(utcNow))
      ? DraftLifecycleView.Scheduled
      : DraftLifecycleView.Created;
  }
}
