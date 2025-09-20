namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.Entities;

public sealed class DraftRelease
{
  private DraftRelease(
    DraftPartId partId,
    ReleaseChannel releaseChannel,
    DateOnly releaseDate,
    DateTime createdOnUtc, 
    int? episodeNumber = null)
  {
    PartId = partId;
    ReleaseChannel = releaseChannel;
    ReleaseDate = releaseDate;
    CreatedOnUtc = createdOnUtc;
    EpisodeNumber = episodeNumber;
  }

  private DraftRelease()
  {
    // For EF
  }

  public DraftPartId PartId { get; private set; } = default!;
  public DraftPart DraftPart { get; private set; } = default!;
  public ReleaseChannel ReleaseChannel { get; private set; } = default!;
  public int? EpisodeNumber { get; private set; }
  public DateOnly ReleaseDate { get; private set; }
  public DateTime CreatedOnUtc { get; private set; } = DateTime.UtcNow;

  public static Result<DraftRelease> Create(
    DraftPartId partId,
    ReleaseChannel releaseChannel,
    DateOnly releaseDate,
    int? episodeNumber = null)
  {
    if (partId is null)
    {
      return Result.Failure<DraftRelease>(DraftErrors.DraftPartIsRequired);
    }

    if (releaseDate == default)
    {
      return Result.Failure<DraftRelease>(DraftErrors.ReleaseDateIsRequired);
    }

    var draftRelease = new DraftRelease(
      partId: partId,
      releaseChannel: releaseChannel,
      releaseDate: releaseDate,
      createdOnUtc: DateTime.UtcNow,
      episodeNumber: episodeNumber);

    return Result.Success(draftRelease);
  }

  public Result SetEpisodeNumber(int episodeNumber)
  {
    Guard.Against.NegativeOrZero(
      episodeNumber,
      message: DraftErrors.EpisodeNumberMustBeGreaterThanZero.Description.ToString());

    EpisodeNumber = episodeNumber;

    return Result.Success();
  }
}
