namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.Entities;

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
  public DraftPartId PartId { get; init; } = default!;
  public DraftPart DraftPart { get; init; } = default!;
  public ReleaseChannel ReleaseChannel { get; init; } = default!;
  public DateOnly ReleaseDate { get; init; }
  public DateTime CreatedOnUtc { get; init; } = DateTime.UtcNow;

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
}
