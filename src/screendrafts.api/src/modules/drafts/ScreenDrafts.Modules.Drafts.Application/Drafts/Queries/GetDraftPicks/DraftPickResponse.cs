namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Queries.GetDraftPicks;

public sealed record DraftPickResponse(
  int Position,
  Guid MovieId,
  string MovieTitle,
  Guid DrafterId,
  string DrafterName)
{
  public DraftPickResponse()
    : this(
        Position: 0,
        MovieId: Guid.Empty,
        MovieTitle: string.Empty,
        DrafterId: Guid.Empty,
        DrafterName: string.Empty)
  {
  }
}

