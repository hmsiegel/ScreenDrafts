namespace ScreenDrafts.Modules.Reporting.Domain.Drafts;

public class DraftPartRelease : Entity<DraftPartReleaseId>
{
  private DraftPartRelease(
    Guid draftId,
    string draftPartPublicId,
    string releaseChannel,
    DateOnly releaseDate,
    DraftPartReleaseId? id = null
  )
    : base(id ?? DraftPartReleaseId.CreateUnique())
  {
    DraftId = draftId;
    DraftPartPublicId = draftPartPublicId;
    ReleaseChannel = releaseChannel;
    ReleaseDate = releaseDate;
  }

  private DraftPartRelease() { }

  public Guid DraftId { get; private set; }
  public string DraftPartPublicId { get; private set; } = default!;
  public string ReleaseChannel { get; private set; } = default!;
  public DateOnly ReleaseDate { get; private set; }

  public static DraftPartRelease Create(
    Guid draftId,
    string draftPartPublicId,
    string releaseChannel,
    DateOnly releaseDate
  )
  {
    return new DraftPartRelease(
      draftId: draftId,
      draftPartPublicId: draftPartPublicId,
      releaseChannel: releaseChannel,
      releaseDate: releaseDate
    );
  }

  public void UpdateReleaseDate(DateOnly releaseDate)
  {
    ReleaseDate = releaseDate;
  }
}
