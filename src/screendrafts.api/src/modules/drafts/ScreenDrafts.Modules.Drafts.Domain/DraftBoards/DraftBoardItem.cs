namespace ScreenDrafts.Modules.Drafts.Domain.DraftBoards;

public sealed class DraftBoardItem
{
  private DraftBoardItem(
    int tmdbId,
    string? notes,
    int? priority)
  {
    TmdbId = tmdbId;
    Notes = notes;
    Priority = priority;
  }

  private DraftBoardItem()
  {
  }

  public int TmdbId { get; private set; }
  public string? Notes { get; private set; }
  public int? Priority { get; private set; }

  public static Result<DraftBoardItem> Create(
    int tmdbId,
    string? notes,
    int? priority)
  {
    if (tmdbId <= 0)
    {
      return Result.Failure<DraftBoardItem>(DraftBoardErrors.InvalidTmdbId);
    }

    return new DraftBoardItem(tmdbId, notes, priority);
  }

  internal void Update(
    string? notes,
    int? priority)
  {
    Notes = notes;
    Priority = priority;
  }
}
