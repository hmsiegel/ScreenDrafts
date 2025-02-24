
namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Queries.GetDraftPositionsByGameBoard;

public sealed record GetDraftPositionsByGameBoardQuery(Guid GameBoardId) : IQuery<IReadOnlyCollection<DraftPositionResponse>>;
