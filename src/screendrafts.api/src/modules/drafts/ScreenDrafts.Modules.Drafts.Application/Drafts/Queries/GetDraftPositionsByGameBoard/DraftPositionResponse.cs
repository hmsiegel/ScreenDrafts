
namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Queries.GetDraftPositionsByGameBoard;

public sealed record DraftPositionResponse(
  Guid Id,
  string Name,
  string Picks,
  bool HasBonusVeto,
  bool HasBonusVetoOveride,
  Guid DrafterId)
{
  public DraftPositionResponse()
    : this(
        Id: Guid.Empty,
        Name: string.Empty,
        Picks: string.Empty,
        HasBonusVeto: false,
        HasBonusVetoOveride: false,
        DrafterId: Guid.Empty)
  {
  }
}
