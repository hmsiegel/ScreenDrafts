namespace ScreenDrafts.Modules.Drafts.Application._Legacy.Drafts.Queries.GetTriviaResultsForDrafter;

public sealed record GetTriviaResultsForDrafterQuery(Guid DraftId, Guid DrafterId) : IQuery<TriviaResultDto>;
