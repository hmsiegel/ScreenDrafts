namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.Entities;

public sealed class DraftChannelRelease
{
  private DraftChannelRelease(
    DraftId draftId,
    SeriesId seriesId,
    ReleaseChannel releaseChannel,
    DateTime createdOnUtc,
    int? episodeNumber = null)
  {
    DraftId = draftId;
    SeriesId = seriesId;
    ReleaseChannel = releaseChannel;
    EpisodeNumber = episodeNumber;
    CreatedOnUtc = createdOnUtc;
  }

  private DraftChannelRelease()
  {
    // For EF
  }

  public Draft Draft { get; private set; } = default!;
  public DraftId DraftId { get; private set; } = default!;

  public Series Series { get; private set; } = default!;
  public SeriesId SeriesId { get; private set; } = default!;

  public ReleaseChannel ReleaseChannel { get; private set; } = default!;
  public int? EpisodeNumber { get; private set; }
  public DateTime CreatedOnUtc { get; private set; } = DateTime.UtcNow;

  internal static Result<DraftChannelRelease> Create(
    DraftId draftId,
    SeriesId seriesId,
    ReleaseChannel releaseChannel,
    int? episodeNumber = null)
  {
    if (draftId is null)
    {
      return Result.Failure<DraftChannelRelease>(DraftErrors.DraftIsRequired);
    }

    if (seriesId is null)
    {
      return Result.Failure<DraftChannelRelease>(DraftErrors.SeriesIsRequired);
    }

    if (episodeNumber.HasValue && episodeNumber.Value <= 0)
    {
      return Result.Failure<DraftChannelRelease>(DraftErrors.EpisodeNumberMustBeGreaterThanZero);
    }

    var draftChannelRelease = new DraftChannelRelease(
      draftId: draftId,
      seriesId: seriesId,
      releaseChannel: releaseChannel,
      episodeNumber: episodeNumber,
      createdOnUtc: DateTime.UtcNow);

    return Result.Success(draftChannelRelease);
  }

  internal Result SetEpisodeNumber(int episodeNumber)
  {
    if (episodeNumber <= 0)
    {
      return Result.Failure(DraftErrors.EpisodeNumberMustBeGreaterThanZero);
    }

    if (EpisodeNumber == episodeNumber)
    {
      return Result.Success();
    }

    EpisodeNumber = episodeNumber;

    return Result.Success();
  }

  internal Result ClearEpisodeNumber()
  {
    if (!EpisodeNumber.HasValue)
    {
      return Result.Success();
    }
    EpisodeNumber = null;
    return Result.Success();
  }
}
