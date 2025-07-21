namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Queries.GetDraftPicks;

public sealed record DraftPickResponse(
  int Position,
  int PlayOrder,
  Guid MovieId,
  string MovieTitle,
  Guid? DrafterId,
  string? DrafterName,
  bool IsVetoed = false,
  Guid? DrafterTeamId = null,
  string? DrafterTeamName = null)
{
  public DraftPickResponse()
    : this(
        Position: 0,
        PlayOrder: 0,
        MovieId: Guid.Empty,
        MovieTitle: string.Empty,
        DrafterId: Guid.Empty,
        IsVetoed: false,
        DrafterName: string.Empty,
        DrafterTeamId: null,
        DrafterTeamName: null)
  {
  }
}

