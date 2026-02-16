namespace ScreenDrafts.Modules.Drafts.Contracts.GameBoards;

public sealed record DraftPositionResponse(
  Guid Id,
  string Name,
  string Picks,
  bool HasBonusVeto,
  bool HasBonusVetoOveride,
  Guid? DrafterId,
  Guid? DrafterTeamId)
{
  public DraftPositionResponse()
    : this(
        Id: Guid.Empty,
        Name: string.Empty,
        Picks: string.Empty,
        HasBonusVeto: false,
        HasBonusVetoOveride: false,
        DrafterId: Guid.Empty,
        DrafterTeamId: Guid.Empty)
  {
  }
}
