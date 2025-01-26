namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Queries.ListUpcomingDrafts;
public sealed record ListUpcomingDraftsQuery() : IQuery<IReadOnlyList<UpcomingDraftResponse>>;

public sealed record UpcomingDraftResponse(
    Guid DraftId,
    string Title,
    DateOnly ReleaseDate);
