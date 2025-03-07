namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Queries.GetTriviaResultsForDrafter;

public sealed record GetTriviaResultsForDrafterQuery(Guid DraftId, Guid DrafterId) : IQuery<TriviaResultDto>;
