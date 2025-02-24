namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Queries.GetDraftPicks;

public sealed record DraftPickResponse(
  int Position,
  Guid DraftId,
  Guid MovieId,
  Guid DrafterId)
{
  public DraftPickResponse()
    : this(
        Position: 0,
        DraftId: Guid.Empty,
        MovieId: Guid.Empty,
        DrafterId: Guid.Empty)
  {
  }
}

