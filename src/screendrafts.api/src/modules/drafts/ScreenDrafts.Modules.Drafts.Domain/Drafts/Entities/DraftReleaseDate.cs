namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.Entities;

public sealed class DraftReleaseDate
{
  private DraftReleaseDate()
  {
  }

  private DraftReleaseDate(DraftId draftId, DateOnly releaseDate)
  {
    DraftId = draftId;
    ReleaseDate = releaseDate;
  }

  public DraftId DraftId { get; private set; } = default!;

  public DateOnly ReleaseDate { get; private set; }

  public Draft Draft { get; private set; } = default!;

  public static DraftReleaseDate Create(DraftId draftId, DateOnly releaseDate)
  {
    return new DraftReleaseDate(draftId, releaseDate);
  }
}
