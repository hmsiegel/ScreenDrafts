namespace ScreenDrafts.Modules.Drafts.Domain.DraftParts.Entities;

public sealed class DraftRelease
{
  private DraftRelease(
    DraftPartId partId,
    ReleaseChannel releaseChannel,
    DateOnly releaseDate,
    DateTime createdOnUtc)
  {
    PartId = partId;
    ReleaseChannel = releaseChannel;
    ReleaseDate = releaseDate;
    CreatedOnUtc = createdOnUtc;
  }

  private DraftRelease()
  {
    // For EF
  }
  public DraftPartId PartId { get; private set; } = default!;
  public DraftPart DraftPart { get; private set; } = default!;
  public ReleaseChannel ReleaseChannel { get; private set; } = default!;
  public DateOnly ReleaseDate { get; private set; }
  public DateTime CreatedOnUtc { get; private set; } = DateTime.UtcNow;

  public static Result<DraftRelease> Create(
    DraftPartId partId,
    ReleaseChannel releaseChannel,
    DateOnly releaseDate)
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
      createdOnUtc: DateTime.UtcNow);

    return Result.Success(draftRelease);
  }

  internal Result SetReleaseDate(DateOnly releaseDate)
  {
    if (releaseDate == default)
    {
      return Result.Failure(DraftErrors.ReleaseDateIsRequired);
    }

    if (ReleaseDate == releaseDate)
    {
      return Result.Success();
    }

    ReleaseDate = releaseDate;

    return Result.Success();
  }
}
